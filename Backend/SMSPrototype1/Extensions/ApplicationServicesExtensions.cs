using SMSServices.Services;
using SMSServices.ServicesInterfaces;

namespace SMSPrototype1.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Domain Services
            services.AddScoped<ISchoolService, SchoolService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ISchoolClassServices, SchoolClassServices>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ITeacherAttendanceService, TeacherAttendanceService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<ICombinedDetailsServices, CombinedDetailsServices>();
            services.AddScoped<IChatService, ChatService>();

            return services;
        }

        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            // Security & Authentication Services
            services.AddSingleton<IRoomAccessTokenService, RoomAccessTokenService>();
            services.AddSingleton<IMessageEncryptionService, MessageEncryptionService>();
            services.AddSingleton<IVideoRecordingService, VideoRecordingService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Infrastructure Services
            services.AddSingleton<IErrorLogService, ErrorLogService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddSingleton<IErrorSanitizationService, ErrorSanitizationService>();
            services.AddSingleton<IRateLimitService>(sp => new RateLimitService(10, 100)); // 10/min, 100/day
            services.AddSingleton<IGeminiCacheService, GeminiCacheService>();

            // Add HttpContextAccessor for audit logging
            services.AddHttpContextAccessor();

            // Add HttpClientFactory for external API calls
            services.AddHttpClient();

            // Add Memory Cache for token blacklist
            services.AddMemoryCache();

            return services;
        }
    }
}
