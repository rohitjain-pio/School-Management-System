using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.ServicesInterfaces;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLogService;

        public RegistrationController(
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _auditLogService = auditLogService;
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
                // Fire-and-forget audit log
                _ = _auditLogService.LogActionAsync(
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

            // Fire-and-forget audit log
            _ = _auditLogService.LogActionAsync(
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
    }
}
