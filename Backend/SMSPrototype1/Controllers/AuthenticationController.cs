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
        private readonly IPasswordResetService _passwordResetService;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditLogService auditLogService,
            ITokenBlacklistService tokenBlacklistService,
            IPasswordResetService passwordResetService,
            RoleManager<IdentityRole<Guid>> roleManager,
            ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _auditLogService = auditLogService;
            _tokenBlacklistService = tokenBlacklistService;
            _passwordResetService = passwordResetService;
            _roleManager = roleManager;
            _logger = logger;
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
            try
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

            if (user.SchoolId != Guid.Empty)
            {
                authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {UserName}", model.UserName);
                return StatusCode(500, new { message = "An error occurred during login. Please try again." });
            }
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    SchoolId = model.SchoolId,
                    EmailConfirmed = true,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    return BadRequest(new { message = $"Role '{model.Role}' does not exist" });
                }

                await _userManager.AddToRoleAsync(user, model.Role);

                await _auditLogService.LogActionAsync(
                    "Register",
                    "User",
                    user.Id.ToString(),
                    true,
                    $"User {model.UserName} registered with role {model.Role}"
                );

                return Ok(new
                {
                    isSuccess = true,
                    message = "Registration successful!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {UserName}", model.UserName);
                return StatusCode(500, new { message = "An error occurred during registration. Please try again." });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { message = "Refresh token not found" });
                }

                var storedToken = await _refreshTokenService.GetRefreshTokenAsync(refreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var jti = Guid.NewGuid().ToString();

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                };

                if (user.SchoolId != Guid.Empty)
                {
                    authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
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

                var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, GetIpAddress());

                Response.Cookies.Append("auth_token", tokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = token.ValidTo,
                    Path = "/",
                    Domain = null
                });

                Response.Cookies.Append("refresh_token", newRefreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newRefreshToken.ExpiresAt,
                    Path = "/api/Authentication/refresh",
                    Domain = null
                });

                return Ok(new { message = "Token refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "An error occurred during token refresh. Please try again." });
            }
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return Ok(new { message = "If your email is registered, you will receive a password reset link." });
                }

                var token = await _passwordResetService.GeneratePasswordResetTokenAsync(user.Id, GetIpAddress());

                await _auditLogService.LogActionAsync(
                    "PasswordResetRequest",
                    "Auth",
                    user.Id.ToString(),
                    true
                );

                return Ok(new
                {
                    message = "If your email is registered, you will receive a password reset link.",
                    token = token.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request for email {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                var tokenEntity = await _passwordResetService.ValidateTokenAsync(request.Token);

                if (tokenEntity == null || !tokenEntity.IsValid)
                {
                    return BadRequest(new { message = "Invalid or expired reset token" });
                }

                var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());

                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                await _passwordResetService.MarkTokenAsUsedAsync(tokenEntity.Id);
                await _refreshTokenService.RevokeAllUserTokensAsync(user.Id, GetIpAddress());

                await _auditLogService.LogActionAsync(
                    "PasswordReset",
                    "Auth",
                    user.Id.ToString(),
                    true
                );

                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                await _refreshTokenService.RevokeAllUserTokensAsync(user.Id, GetIpAddress());

                var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
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

                await _auditLogService.LogActionAsync(
                    "PasswordChange",
                    "Auth",
                    userId,
                    true
                );

                Response.Cookies.Delete("auth_token");
                Response.Cookies.Delete("refresh_token");

                return Ok(new { message = "Password changed successfully. Please login again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }
    }
}
