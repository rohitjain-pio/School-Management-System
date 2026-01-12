using SMSPrototype1.Middleware;
using SMSServices.Hubs;
using Serilog;

namespace SMSPrototype1.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // Add Serilog request logging
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                    
                    // Add user information if authenticated
                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        diagnosticContext.Set("UserId", httpContext.User.FindFirst("userId")?.Value);
                        diagnosticContext.Set("UserEmail", httpContext.User.FindFirst("email")?.Value);
                    }
                };
            });

            // Add HSTS in production
            if (!environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseCors("AllowFrontend");

            // Add Correlation ID middleware (must be early in pipeline)
            app.UseCorrelationId();

            // Add Response Caching middleware (must be before Authentication)
            app.UseResponseCaching();

            // Add security headers middleware
            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Add error logging middleware (development only)
            if (environment.IsDevelopment())
            {
                app.UseMiddleware<ErrorLoggingMiddleware>();
            }

            // Add rate limiting middleware
            app.UseMiddleware<RateLimitingMiddleware>();

            app.UseAuthentication();

            // Add JWT blacklist middleware after authentication
            app.UseMiddleware<JwtBlacklistMiddleware>();

            app.UseAuthorization();

            return app;
        }

        public static IApplicationBuilder MapEndpoints(this IApplicationBuilder app)
        {
            var webApp = (WebApplication)app;

            webApp.MapControllers();

            // Map SignalR hubs
            webApp.MapHub<ChatHub>("/chatHub");
            webApp.MapHub<VideoCallHub>("/videoCallHub");

            // Map Health Check endpoints
            webApp.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => true // All health checks
            });

            webApp.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready") // Only checks tagged with "ready"
            });

            webApp.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live") // Only checks tagged with "live"
            });

            return app;
        }
    }
}
