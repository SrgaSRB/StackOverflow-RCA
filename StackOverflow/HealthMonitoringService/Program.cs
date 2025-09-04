using HealthMonitoringService;
using HealthMonitoringService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Register services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<DistributedLockService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
