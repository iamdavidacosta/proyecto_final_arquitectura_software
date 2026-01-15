using AuthService.API.Extensions;
using AuthService.Application;
using AuthService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "auth-service")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.AuthDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Log.Information("Database schema ensured/created successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error ensuring database schema");
    }
}

// Configure pipeline
app.UseApiMiddleware();

app.Run();
