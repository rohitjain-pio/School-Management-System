using Serilog;
using SMSPrototype1.Extensions;

// Configure Serilog from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting SMS Prototype application");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services organized by domain
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddSecurityServices();
builder.Services.AddInfrastructureServices();

// Add API services
builder.Services.AddApiServices();
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Environment, builder.Configuration);
builder.Services.AddSignalRServices();
builder.Services.AddAutoMapperServices();
builder.Services.AddHealthCheckServices(builder.Configuration);

// Add authentication & authorization
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

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

// Configure middleware pipeline
app.UseCustomMiddleware(app.Environment);

// Map endpoints
app.MapEndpoints();

    Log.Information("SMS Prototype application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
