using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthMonitoringService.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAlertEmailAsync(string toEmail, string toName, string serviceName, string errorMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"], 
                    _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = $"Health Check Alert - {serviceName} is DOWN";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h2>Service Health Alert</h2>
                        <p><strong>Service:</strong> {serviceName}</p>
                        <p><strong>Status:</strong> NOT_OK</p>
                        <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                        <p><strong>Error:</strong> {errorMessage}</p>
                        <hr>
                        <p><em>This is an automated message from StackOverflow Health Monitoring Service.</em></p>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Use STARTTLS for port 587
                await client.ConnectAsync(
                    _configuration["Email:SmtpHost"], 
                    int.Parse(_configuration["Email:SmtpPort"]!), 
                    SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(
                    _configuration["Email:SmtpUsername"], 
                    _configuration["Email:SmtpPassword"]);
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Alert email sent to {Email} for service {ServiceName}", toEmail, serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert email to {Email} for service {ServiceName}", toEmail, serviceName);
            }
        }
    }
}
