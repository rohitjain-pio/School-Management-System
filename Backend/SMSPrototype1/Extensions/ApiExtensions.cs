using FluentValidation;
using SMSPrototype1.Filters;

namespace SMSPrototype1.Extensions
{
    public static class ApiExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Add controllers with validation filter
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });

            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<Program>();

            // Add API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Response Caching
            services.AddResponseCaching();

            // Configure Redis Distributed Cache (Optional - requires Redis server)
            var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                try
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnectionString;
                        options.InstanceName = configuration.GetSection("Redis:InstanceName").Value ?? "SMSPrototype_";
                    });
                    Console.WriteLine($"Redis distributed cache configured: {redisConnectionString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not configure Redis cache: {ex.Message}");
                    // Fallback to memory cache if Redis is not available
                    services.AddDistributedMemoryCache();
                }
            }
            else
            {
                // Use in-memory distributed cache if Redis is not configured
                services.AddDistributedMemoryCache();
                Console.WriteLine("Using in-memory distributed cache (Redis not configured)");
            }

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                var allowedOrigins = environment.IsDevelopment()
                    ? configuration.GetSection("CORS:Development:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>()
                    : configuration.GetSection("CORS:Production:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (environment.IsDevelopment())
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins)
                              .WithHeaders("Content-Type", "Authorization")
                              .WithMethods("GET", "POST", "PUT", "DELETE")
                              .AllowCredentials();
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            })
            .AddJsonProtocol(options =>
            {
                // Keep PascalCase for method names (don't convert to camelCase)
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            return services;
        }

        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection")!,
                    name: "sql-server",
                    tags: new[] { "database", "sql", "ready" })
                .AddCheck("api-self", () => 
                    Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"),
                    tags: new[] { "api", "live" });

            return services;
        }

        public static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
