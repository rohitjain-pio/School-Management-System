using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSServices.Hubs;
using SMSServices.Services;
using SMSServices.ServicesInterfaces;
using SMSRepository.Repository;
using SMSRepository.RepositoryInterfaces;
using SMSPrototype1.Middleware;
using SMSPrototype1.Authorization;
using SMSPrototype1.Filters;
using System.Text;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Repositories
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<ITeacherAttendanceRepository, TeacherAttendanceRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<ICombinedDetailsRepository, CombinedDetailsRepository>();

// Register Services
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ISchoolClassServices, SchoolClassServices>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ITeacherAttendanceService, TeacherAttendanceService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<ICombinedDetailsServices, CombinedDetailsServices>();

// Register Security Services
builder.Services.AddSingleton<IRoomAccessTokenService, RoomAccessTokenService>();
builder.Services.AddSingleton<IMessageEncryptionService, MessageEncryptionService>();
builder.Services.AddSingleton<IVideoRecordingService, VideoRecordingService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

// Add HttpContextAccessor for audit logging
builder.Services.AddHttpContextAccessor();

// Add Memory Cache for token blacklist
builder.Services.AddMemoryCache();

// Configure DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
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

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
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

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
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
builder.Services.AddScoped<IAuthorizationHandler, SameSchoolHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ResourceOwnerHandler>();

// Configure CORS
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                      "https://localhost:5173", 
                      "https://localhost:5174", 
                      "https://localhost:3000",
                      "https://localhost:8080",
                      "http://localhost:5173",
                      "http://localhost:5174",
                      "http://localhost:3000",
                      "http://localhost:8080")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
    else
    {
        // Production CORS policy - restrict to actual domain
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("https://sms.edu", "https://www.sms.edu")
                  .WithHeaders("Content-Type", "Authorization")
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .AllowCredentials();
        });
    }
});

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
})
.AddJsonProtocol(options =>
{
    // Keep PascalCase for method names (don't convert to camelCase)
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

// Configure AutoMapper if needed
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Global exception handler for production
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

// Add HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors("AllowFrontend");

// Add security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Add rate limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();

// Add JWT blacklist middleware after authentication
app.UseMiddleware<JwtBlacklistMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<ChatHub>("/chatHub");
app.MapHub<VideoCallHub>("/videoCallHub");

app.Run();
