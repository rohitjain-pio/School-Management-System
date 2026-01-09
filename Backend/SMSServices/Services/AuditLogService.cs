using Microsoft.AspNetCore.Http;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using System.Security.Claims;

namespace SMSServices.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string action, string resource, string? resourceId = null, bool success = true, string? details = null, string? errorMessage = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId != null ? Guid.Parse(userId) : null,
                Action = action,
                Resource = resource,
                ResourceId = resourceId,
                Success = success,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow,
                Details = details,
                ErrorMessage = errorMessage
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogLoginAttemptAsync(string userName, bool success, string? errorMessage = null)
        {
            await LogActionAsync(
                action: "Login",
                resource: "Authentication",
                resourceId: userName,
                success: success,
                details: success ? "User logged in successfully" : "Login attempt failed",
                errorMessage: errorMessage
            );
        }

        public async Task LogDataAccessAsync(string resource, string resourceId, bool success = true)
        {
            await LogActionAsync(
                action: "DataAccess",
                resource: resource,
                resourceId: resourceId,
                success: success,
                details: $"Accessed {resource} with ID {resourceId}"
            );
        }

        public async Task LogUnauthorizedAccessAsync(string resource, string? resourceId = null)
        {
            await LogActionAsync(
                action: "UnauthorizedAccess",
                resource: resource,
                resourceId: resourceId,
                success: false,
                details: "Attempted to access resource without authorization"
            );
        }
    }
}
