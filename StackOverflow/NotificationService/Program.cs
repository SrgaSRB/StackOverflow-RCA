using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Add web services
builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure health endpoint
app.MapGet("/health-monitoring", () =>
{
    var healthStatus = new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Service = "NotificationService",
        Version = "1.0.0"
    };
    return Results.Ok(healthStatus);
});

app.MapControllers();

app.Run();
