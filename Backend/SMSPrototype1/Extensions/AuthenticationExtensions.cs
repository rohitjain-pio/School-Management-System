using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSPrototype1.Authorization;
using System.Text;

namespace SMSPrototype1.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            // Configure Identity
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("JWT Key is not configured"))),
                    ClockSkew = TimeSpan.Zero // No tolerance for token expiration
                };

                // Configure cookie-based JWT for SignalR and token from query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        // Allow access token in query string for SignalR hubs
                        if (!string.IsNullOrEmpty(accessToken) && 
                            (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/videoCallHub")))
                        {
                            context.Token = accessToken;
                        }
                        else if (context.Request.Cookies.ContainsKey("auth_token"))
                        {
                            context.Token = context.Request.Cookies["auth_token"];
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Admin-only policy - requires Admin role
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                // School Admin policy - requires SchoolAdmin role
                options.AddPolicy("SchoolAdminOnly", policy =>
                    policy.RequireRole("SchoolAdmin"));

                // Admin or School Admin policy
                options.AddPolicy("AdminOrSchoolAdmin", policy =>
                    policy.RequireRole("Admin", "SchoolAdmin"));

                // Teacher or above policy - requires Teacher, SchoolAdmin, or Admin role
                options.AddPolicy("TeacherOrAbove", policy =>
                    policy.RequireRole("Teacher", "SchoolAdmin", "Admin"));

                // Student or above policy - requires any authenticated user
                options.AddPolicy("StudentOrAbove", policy =>
                    policy.RequireRole("Student", "Teacher", "SchoolAdmin", "Admin"));

                // Same school policy - user must belong to the same school
                options.AddPolicy("SameSchool", policy =>
                    policy.AddRequirements(new SameSchoolRequirement()));

                // Resource owner policy - user must own the resource
                options.AddPolicy("ResourceOwner", policy =>
                    policy.AddRequirements(new ResourceOwnerRequirement()));

                // Combined policies
                options.AddPolicy("TeacherInSameSchool", policy =>
                {
                    policy.RequireRole("Teacher", "SchoolAdmin", "Admin");
                    policy.AddRequirements(new SameSchoolRequirement());
                });

                options.AddPolicy("StudentInSameSchool", policy =>
                {
                    policy.RequireRole("Student", "Teacher", "SchoolAdmin", "Admin");
                    policy.AddRequirements(new SameSchoolRequirement());
                });
            });

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, SameSchoolHandler>();
            services.AddScoped<IAuthorizationHandler, ResourceOwnerHandler>();

            return services;
        }
    }
}
