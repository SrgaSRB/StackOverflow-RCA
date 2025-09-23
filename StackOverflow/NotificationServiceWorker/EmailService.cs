using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    public class EmailService
    {
        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();

                string fromEmail = CloudConfigurationManager.GetSetting("FromEmail");
                if (string.IsNullOrEmpty(fromEmail))
                {
                    Trace.TraceError("FromEmail configuration is missing");
                    return false;
                }

                string fromName = CloudConfigurationManager.GetSetting("FromName");
                if (string.IsNullOrEmpty(fromName))
                {
                    Trace.TraceError("FromName configuration is missing");
                    return false;
                }

                message.From.Add(new MailboxAddress(fromName, fromEmail));

                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
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

                Trace.TraceInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to send email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public string CreateNotificationEmailBody(string questionTitle, string bestAnswerAuthor, string bestAnswerContent)
        {
            return $@"
                <html>
                <body>
                    <h2>Pitanje je zatvoreno - Najbolji odgovor je označen</h2>
                    <p>Poštovani,</p>
                    <p>Pitanje na koje ste odgovorili je uspešno zatvoreno jer je označen najbolji odgovor.</p>
                    
                    <h3>Pitanje: {questionTitle}</h3>
                    
                    <h4>Najbolji odgovor (autor: {bestAnswerAuthor}):</h4>
                    <div style='border-left: 3px solid #007bff; padding-left: 15px; margin: 10px 0;'>
                        <p>{bestAnswerContent}</p>
                    </div>
                    
                    <p>Hvala vam na učešću u našoj zajednici!</p>
                    
                    <p>Srdačan pozdrav,<br/>StackOverflow tim</p>
                </body>
                </html>";
        }
    }
}
