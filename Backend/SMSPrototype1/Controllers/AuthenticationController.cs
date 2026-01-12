using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.ServicesInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditLogService _auditLogService;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditLogService auditLogService,
            ITokenBlacklistService tokenBlacklistService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _auditLogService = auditLogService;
            _tokenBlacklistService = tokenBlacklistService;
        }

        private string GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                schoolId = user.SchoolId,
                roles = roles
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            
            if (user == null)
            {
                await _auditLogService.LogLoginAttemptAsync(model.UserName, false, "User not found");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (user.IsLockedOut)
            {
                await _auditLogService.LogLoginAttemptAsync(model.UserName, false, "Account locked");
                return StatusCode(423, new { message = "Account is locked. Please try again later." });
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                user.IncrementFailedLoginAttempts();
                await _userManager.UpdateAsync(user);
                
                await _auditLogService.LogLoginAttemptAsync(model.UserName, false, "Invalid password");
                
                var attemptsRemaining = 5 - user.FailedLoginAttempts;
                if (attemptsRemaining > 0)
                {
                    return Unauthorized(new { message = $"Invalid credentials. {attemptsRemaining} attempts remaining." });
                }
                else
                {
                    return StatusCode(423, new { message = "Account locked due to too many failed attempts. Try again in 30 minutes." });
                }
            }

            // Parallelize independent operations
            var rolesTask = _userManager.GetRolesAsync(user);
            var refreshTokenTask = _refreshTokenService.GenerateRefreshTokenAsync(user.Id, GetIpAddress());

            // Update user state
            user.ResetFailedLoginAttempts();
            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIp = GetIpAddress();
            await _userManager.UpdateAsync(user);

            // Wait for parallel operations
            var userRoles = await rolesTask;
            var refreshToken = await refreshTokenTask;

            // Generate JWT token
            var jti = Guid.NewGuid().ToString();
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            if (user.SchoolId != null)
            {
                authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()!));
            }

            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                expires: DateTime.UtcNow.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Set cookies
            Response.Cookies.Append("auth_token", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = token.ValidTo,
                Path = "/",
                Domain = null
            });

            Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiresAt,
                Path = "/api/Auth/refresh",
                Domain = null
            });

            // Fire-and-forget audit log
            _ = _auditLogService.LogLoginAttemptAsync(model.UserName, true);

            return Ok(new 
            { 
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    schoolId = user.SchoolId,
                    roles = userRoles
                }
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (!string.IsNullOrEmpty(userId))
            {
                await _refreshTokenService.RevokeAllUserTokensAsync(Guid.Parse(userId), GetIpAddress());

                if (!string.IsNullOrEmpty(jti))
                {
                    var expiration = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
                    if (!string.IsNullOrEmpty(expiration))
                    {
                        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration)).UtcDateTime;
                        var timeToExpiry = expiryDate - DateTime.UtcNow;
                        if (timeToExpiry > TimeSpan.Zero)
                        {
                            await _tokenBlacklistService.AddToBlacklistAsync(jti, timeToExpiry);
                        }
                    }
                }

                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
                    "Logout",
                    "Auth",
                    userId,
                    true
                );
            }

            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logout successful" });
        }
    }
}
