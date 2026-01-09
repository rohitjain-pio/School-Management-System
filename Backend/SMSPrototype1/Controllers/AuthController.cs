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
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditLogService _auditLogService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditLogService auditLogService,
            IPasswordResetService passwordResetService,
            ITokenBlacklistService tokenBlacklistService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _refreshTokenService = refreshTokenService;
            _auditLogService = auditLogService;
            _passwordResetService = passwordResetService;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                SchoolId = model.SchoolId,
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                await _auditLogService.LogActionAsync(
                    "Register",
                    "User",
                    null,
                    false,
                    string.Join("; ", result.Errors.Select(e => e.Description))
                );

                return BadRequest(new
                {
                    isSuccess = false,
                    errorMessage = string.Join("; ", result.Errors.Select(e => e.Description))
                });
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

            user.ResetFailedLoginAttempts();
            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIp = GetIpAddress();
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);
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

            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, GetIpAddress());

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

            await _auditLogService.LogLoginAttemptAsync(model.UserName, true);

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

                await _auditLogService.LogActionAsync(
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

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            {
                return Unauthorized(new { message = "Refresh token not found" });
            }

            var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, GetIpAddress());
            if (newRefreshToken == null)
            {
                await _auditLogService.LogActionAsync(
                    "RefreshToken",
                    "Auth",
                    null,
                    false,
                    null,
                    "Invalid or expired refresh token"
                );
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var user = await _userManager.FindByIdAsync(newRefreshToken.UserId.ToString());
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
                Path = "/api/Auth/refresh",
                Domain = null
            });

            await _auditLogService.LogActionAsync(
                "RefreshToken",
                "Auth",
                user.Id.ToString(),
                true
            );

            return Ok(new { message = "Token refreshed successfully" });
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                await _auditLogService.LogActionAsync(
                    "RequestPasswordReset",
                    "Auth",
                    null,
                    false,
                    $"Password reset requested for non-existent email: {model.Email}"
                );
                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }

            var resetToken = await _passwordResetService.GeneratePasswordResetTokenAsync(user.Id, GetIpAddress());

            await _auditLogService.LogActionAsync(
                "RequestPasswordReset",
                "Auth",
                user.Id.ToString(),
                true,
                $"Password reset token generated for {user.Email}"
            );

            return Ok(new 
            { 
                message = "If the email exists, a password reset link has been sent.",
                resetToken = resetToken.Token
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var resetToken = await _passwordResetService.ValidateTokenAsync(model.Token);
            if (resetToken == null || !resetToken.IsValid || resetToken.UserId != user.Id)
            {
                await _auditLogService.LogActionAsync(
                    "ResetPassword",
                    "Auth",
                    user.Id.ToString(),
                    false,
                    null,
                    "Invalid or expired reset token"
                );
                return BadRequest(new { message = "Invalid or expired reset token" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                await _auditLogService.LogActionAsync(
                    "ResetPassword",
                    "Auth",
                    user.Id.ToString(),
                    false,
                    string.Join("; ", result.Errors.Select(e => e.Description))
                );
                return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });
            }

            await _passwordResetService.MarkTokenAsUsedAsync(resetToken.Id);
            await _refreshTokenService.RevokeAllUserTokensAsync(user.Id, GetIpAddress());

            await _auditLogService.LogActionAsync(
                "ResetPassword",
                "Auth",
                user.Id.ToString(),
                true,
                "Password reset successfully"
            );

            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                await _auditLogService.LogActionAsync(
                    "ChangePassword",
                    "Auth",
                    user.Id.ToString(),
                    false,
                    string.Join("; ", result.Errors.Select(e => e.Description))
                );
                return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });
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
                "ChangePassword",
                "Auth",
                user.Id.ToString(),
                true,
                "Password changed successfully"
            );

            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            return Ok(new { message = "Password changed successfully. Please login again." });
        }
    }
}
