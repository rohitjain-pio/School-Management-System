using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditLogService _auditLogService;

        public TokenController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _auditLogService = auditLogService;
        }

        private string GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

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
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
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

            // Fire-and-forget audit log
            _ = _auditLogService.LogActionAsync(
                "RefreshToken",
                "Auth",
                user.Id.ToString(),
                true
            );

            return Ok(new { message = "Token refreshed successfully" });
        }
    }
}
