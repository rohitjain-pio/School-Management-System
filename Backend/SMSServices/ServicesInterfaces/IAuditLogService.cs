namespace SMSServices.ServicesInterfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string action, string resource, string? resourceId = null, bool success = true, string? details = null, string? errorMessage = null);
        Task LogLoginAttemptAsync(string userName, bool success, string? errorMessage = null);
        Task LogDataAccessAsync(string resource, string resourceId, bool success = true);
        Task LogUnauthorizedAccessAsync(string resource, string? resourceId = null);
    }
}
