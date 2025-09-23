using Common.Models;
using MailKit.Security;
using Microsoft.Azure;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitoringServiceWorker
{
    public class EmailAlertService
    {

        public async Task SendHealthAlertAsync(List<AlertEmail> recipients, HealthCheck healthCheck)
        {
            try
            {
                foreach (var recipient in recipients)
                {
                    await SendEmailAsync(recipient.Email, healthCheck);
                }

                Trace.TraceInformation($"Health alert sent to {recipients.Count} recipients for {healthCheck.ServiceName}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error sending health alerts: {ex.Message}");
            }
        }


        private async Task<bool> SendEmailAsync(string toEmail, HealthCheck healthCheck)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    CloudConfigurationManager.GetSetting("FromName"),
                    CloudConfigurationManager.GetSetting("FromEmail")
                ));

                message.To.Add(new MailboxAddress("Admin", toEmail));
                message.Subject = $"ALERT: {healthCheck.ServiceName} Health Check Failed";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = CreateAlertEmailBody(healthCheck)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.CheckCertificateRevocation = false;

                    client.SslProtocols = SslProtocols.Tls12;

                    await client.ConnectAsync(
                        "smtp.gmail.com",
                        465,
                        SecureSocketOptions.SslOnConnect
                    );

                    await client.AuthenticateAsync(
                        CloudConfigurationManager.GetSetting("SmtpUsername"),
                        CloudConfigurationManager.GetSetting("SmtpPassword")
                    );

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Trace.TraceInformation($"Health alert email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to send health alert email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private string CreateAlertEmailBody(HealthCheck healthCheck)
        {
            return $@"
                <html>
                <body>
                    <h2 style='color: red;'>SERVICE HEALTH ALERT</h2>
                    <p><strong>Service:</strong> {healthCheck.ServiceName}</p>
                    <p><strong>Status:</strong> {healthCheck.Status}</p>
                    <p><strong>Time:</strong> {healthCheck.CheckDateTime:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p><strong>Response Time:</strong> {healthCheck.ResponseTimeMs}ms</p>
                    
                    {(string.IsNullOrEmpty(healthCheck.ErrorMessage) ? "" : $"<p><strong>Error:</strong> {healthCheck.ErrorMessage}</p>")}
                    
                    <p>Please check the service status and take appropriate action.</p>
                    
                    <p>Best regards,<br/>Health Monitoring System</p>
                </body>
                </html>";
        }

    }
}
