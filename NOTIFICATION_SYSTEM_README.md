# StackOverflow Notification Service

This notification service automatically sends emails to users when a question they answered gets a best answer selected.

## Architecture

The system consists of:

1. **StackOverflow Service** - Main API that handles question and answer operations
2. **NotificationService** - Worker role that processes email notifications
3. **Azure Storage Queue** - Message queue for decoupling notification processing
4. **Azure Table Storage** - Stores notification logs for tracking

## Components

### 1. StackOverflow Service
- When a user marks a best answer, a notification message is sent to the "notifications" queue
- The message contains: answerId, questionId, and timestamp

### 2. NotificationService Worker Role
- Continuously polls the "notifications" queue for new messages
- For each message:
  - Retrieves the question and best answer details
  - Finds all users who answered the question (excluding the best answer author)
  - Sends email notifications to each user
  - Logs the notification details in the NotificationLogs table

### 3. Email Service
- Uses MailKit/SMTP to send HTML emails
- Configurable SMTP settings in appsettings.json
- Email contains question title, best answer author, and best answer content

## Database Schema

### NotificationLogs Table
| Column | Type | Description |
|--------|------|-------------|
| datum-vreme (Timestamp) | DateTime | When the notification was processed |
| id-odgovora (AnswerId) | String | ID of the best answer |
| broj-poslatih-mejlova (EmailsSent) | Int | Number of emails successfully sent |
| questionId | String | ID of the question |
| questionTitle | String | Title of the question |
| bestAnswerAuthor | String | Username of the best answer author |
| bestAnswerContent | String | Content of the best answer |

## Setup Instructions

### 1. Prerequisites
- .NET 8.0 SDK
- Azure Storage Emulator (Azurite) or Azure Storage Account
- SMTP server access (Gmail, SendGrid, etc.)

### 2. Configuration

#### StackOverflow Service (appsettings.json)
```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  }
}
```

#### NotificationService (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "UseDevelopmentStorage=true"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@stackoverflow.com",
    "FromName": "StackOverflow"
  }
}
```

### 3. Running the Services

#### Start Azurite (if using local storage)
```bash
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

#### Run StackOverflow Service
```bash
cd StackOverflow/StackOverflowService
dotnet run
```

#### Run NotificationService (3 instances for scalability)
```bash
# Terminal 1
cd StackOverflow/NotificationService
dotnet run

# Terminal 2
cd StackOverflow/NotificationService
dotnet run

# Terminal 3
cd StackOverflow/NotificationService
dotnet run
```

#### Run React Frontend
```bash
cd my-app
npm start
```

## Email Configuration

### Gmail SMTP Setup
1. Enable 2-factor authentication on your Gmail account
2. Generate an App Password:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Create a new app password for "Mail"
3. Use the app password in the NotificationService configuration

### SendGrid Setup (Alternative)
1. Sign up for SendGrid
2. Create an API key
3. Update NotificationService configuration:
```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "SmtpUsername": "apikey",
    "SmtpPassword": "your-sendgrid-api-key",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "StackOverflow"
  }
}
```

## Testing the System

1. Create multiple user accounts
2. Create a question with one user
3. Answer the question with other users
4. Log in as the question author
5. Mark one answer as the best answer
6. Check the notification logs in Azure Storage Explorer
7. Verify that emails were sent to all answering users (except the best answer author)

## Monitoring

- Check NotificationService logs for processing status
- Monitor the "notifications" queue in Azure Storage Explorer
- Review NotificationLogs table for delivery statistics
- Check email delivery status in your SMTP provider's dashboard

## Scalability

The system is designed for scalability:
- **Multiple NotificationService instances**: Run 3+ instances to handle high load
- **Queue-based processing**: Ensures reliable message delivery
- **Async processing**: Email sending doesn't block the main application
- **Retry mechanism**: Failed messages become visible again for retry

## Error Handling

- Failed email sends are logged but don't stop processing other emails
- Queue messages have automatic retry capabilities
- Notification processing errors are logged with details
- System continues processing even if individual notifications fail
