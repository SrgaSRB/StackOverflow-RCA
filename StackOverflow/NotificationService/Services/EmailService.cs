using MailKit.Net.Smtp;
using MimeKit;

namespace NotificationService.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = configuration["Email:FromEmail"] ?? "noreply@stackoverflow.com";
            _fromName = configuration["Email:FromName"] ?? "StackOverflow";
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                // Check if SMTP credentials are configured
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    // Demo mode - just log the email instead of sending it
                    Console.WriteLine("=== EMAIL DEMO MODE ===");
                    Console.WriteLine($"TO: {toEmail} ({toName})");
                    Console.WriteLine($"SUBJECT: {subject}");
                    Console.WriteLine($"FROM: {_fromEmail}");
                    Console.WriteLine("BODY:");
                    Console.WriteLine(htmlBody);
                    Console.WriteLine("=== END EMAIL ===");
                    Console.WriteLine();
                    
                    // Simulate email sending delay
                    await Task.Delay(500);
                    return true;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"Email successfully sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public string CreateQuestionClosedEmailBody(string recipientName, string questionTitle, 
            string bestAnswerAuthor, string bestAnswerContent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Question Closed - {questionTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ padding: 20px; border: 1px solid #dee2e6; border-radius: 5px; }}
        .best-answer {{ background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ margin-top: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>🎉 Question Successfully Closed</h2>
        </div>
        
        <div class=""content"">
            <p>Hello {recipientName},</p>
            
            <p>We're writing to inform you that a question you answered has been successfully closed with a best answer selected.</p>
            
            <p><strong>Question:</strong> {questionTitle}</p>
            
            <div class=""best-answer"">
                <h4>✅ Best Answer (by @{bestAnswerAuthor}):</h4>
                <p>{bestAnswerContent}</p>
            </div>
            
            <p>Thank you for your contribution to the community! Your participation helps make our platform a valuable resource for everyone.</p>
        </div>
        
        <div class=""footer"">
            <p>This is an automated message from StackOverflow. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string CreateBestAnswerSelectedEmailBody(string recipientName, string questionTitle, string bestAnswerContent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Congratulations! Your Answer Was Selected - {questionTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #d4edda; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ padding: 20px; border: 1px solid #dee2e6; border-radius: 5px; }}
        .your-answer {{ background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #bee5eb; }}
        .footer {{ margin-top: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px; font-size: 12px; color: #6c757d; }}
        .congratulations {{ color: #155724; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2 class=""congratulations"">🏆 Congratulations! Your Answer Was Selected as Best!</h2>
        </div>
        
        <div class=""content"">
            <p>Hello {recipientName},</p>
            
            <p><strong>Great news!</strong> Your answer has been selected as the best answer for the following question:</p>
            
            <p><strong>Question:</strong> {questionTitle}</p>
            
            <div class=""your-answer"">
                <h4>🌟 Your Best Answer:</h4>
                <p>{bestAnswerContent}</p>
            </div>
            
            <p>This recognition shows the value of your contribution to the community. Thank you for sharing your knowledge and helping others!</p>
            
            <p><strong>What this means:</strong></p>
            <ul>
                <li>Your answer has been marked as the most helpful solution</li>
                <li>The question is now considered resolved</li>
                <li>Your expertise is helping other developers learn</li>
            </ul>
        </div>
        
        <div class=""footer"">
            <p>This is an automated message from StackOverflow. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
