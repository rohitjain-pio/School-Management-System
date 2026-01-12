using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.ServicesInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IAuditLogService _auditLogService;

        public PasswordController(
            UserManager<ApplicationUser> userManager,
            IPasswordResetService passwordResetService,
            IRefreshTokenService refreshTokenService,
            ITokenBlacklistService tokenBlacklistService,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _passwordResetService = passwordResetService;
            _refreshTokenService = refreshTokenService;
            _tokenBlacklistService = tokenBlacklistService;
            _auditLogService = auditLogService;
        }

        private string GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
                    "RequestPasswordReset",
                    "Auth",
                    null,
                    false,
                    $"Password reset requested for non-existent email: {model.Email}"
                );
                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }

            var resetToken = await _passwordResetService.GeneratePasswordResetTokenAsync(user.Id, GetIpAddress());

            // Fire-and-forget audit log
            _ = _auditLogService.LogActionAsync(
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

        [HttpPost("reset")]
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
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
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
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
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

            // Fire-and-forget audit log
            _ = _auditLogService.LogActionAsync(
                "ResetPassword",
                "Auth",
                user.Id.ToString(),
                true,
                "Password reset successfully"
            );

            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }

        [HttpPost("change")]
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
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
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

            // Fire-and-forget audit log
            _ = _auditLogService.LogActionAsync(
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
