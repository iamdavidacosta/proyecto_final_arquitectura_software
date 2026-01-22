using FileIngestionService.API.Extensions;
using FileIngestionService.API.Hubs;
using FileIngestionService.API.Middleware;
using FileIngestionService.Application;
using FileIngestionService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Service", "file-ingestion-service")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileIngestionService.Infrastructure.Persistence.FileDbContext>();
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

// Configure middleware pipeline
app.UseCorrelationId();
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FileUploadHub>("/hubs/file-upload");
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting File Ingestion Service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
