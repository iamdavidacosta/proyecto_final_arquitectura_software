using Serilog;
using SoapCore;
using SoapService.Contracts;
using SoapService.Infrastructure;
using SoapService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Service", "soap-service")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add SOAP Service
builder.Services.AddScoped<IFileShareService, FileShareService>();
builder.Services.AddSoapCore();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// SOAP endpoint
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<IFileShareService>("/soap/files", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);
    endpoints.MapHealthChecks("/health");
});

try
{
    Log.Information("Starting SOAP Service");
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
