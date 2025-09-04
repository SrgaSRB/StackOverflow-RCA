using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using NotificationService.Models;
using NotificationService.Services;
using System.Text.Json;

namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly QueueClient? _queueClient;
    private readonly TableClient? _questionsTableClient;
    private readonly TableClient? _commentsTableClient;
    private readonly TableClient? _usersTableClient;
    private readonly TableClient? _notificationLogTableClient;
    private readonly EmailService _emailService;
    private readonly string _connectionString;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, EmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                           "UseDevelopmentStorage=true";

        try
        {
            // Initialize queue client
            _queueClient = new QueueClient(_connectionString, "notifications");
            _queueClient.CreateIfNotExists();

            // Initialize table clients
            _questionsTableClient = new TableClient(_connectionString, "Questions");
            _questionsTableClient.CreateIfNotExists();
            
            _commentsTableClient = new TableClient(_connectionString, "Comments");
            _commentsTableClient.CreateIfNotExists();
            
            _usersTableClient = new TableClient(_connectionString, "Users");
            _usersTableClient.CreateIfNotExists();
            
            _notificationLogTableClient = new TableClient(_connectionString, "NotificationLogs");
            _notificationLogTableClient.CreateIfNotExists();
            
            _logger.LogInformation("Successfully initialized Azure Storage clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure Storage clients. Service will continue but notification processing may not work.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_queueClient == null)
        {
            _logger.LogError("Queue client is not initialized. Notification processing will not start.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check for messages in the queue
                var messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 10, 
                    visibilityTimeout: TimeSpan.FromMinutes(5), stoppingToken);

                foreach (var message in messages.Value)
                {
                    try
                    {
                        await ProcessNotificationMessage(message);
                        
                        // Delete the message after successful processing
                        await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        
                        _logger.LogInformation("Successfully processed notification message: {MessageId}", 
                            message.MessageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process notification message: {MessageId}", 
                            message.MessageId);
                        
                        // Let the message become visible again for retry
                        // Azure Queue Service will automatically handle retries based on visibility timeout
                    }
                }

                // Wait 30 seconds before checking for new messages
                await Task.Delay(30000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification processing loop");
                
                // Check if it's a cancellation token exception
                if (ex is TaskCanceledException || ex is OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Notification worker is stopping due to cancellation request");
                        break;
                    }
                }
                
                try
                {
                    await Task.Delay(60000, stoppingToken); // Wait 1 minute on error
                }
                catch (TaskCanceledException)
                {
                    // Service is shutting down
                    break;
                }
            }
        }
    }

    private async Task ProcessNotificationMessage(QueueMessage message)
    {
        try
        {
            // Decode and deserialize the message
            var decodedMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
            var notificationMessage = JsonSerializer.Deserialize<NotificationMessage>(decodedMessage);
            
            if (notificationMessage == null)
            {
                _logger.LogWarning("Failed to deserialize notification message");
                return;
            }

            _logger.LogInformation("Processing notification for answer: {AnswerId}, question: {QuestionId}", 
                notificationMessage.AnswerId, notificationMessage.QuestionId);

            // Get the best answer
            var bestAnswer = await GetCommentAsync(notificationMessage.AnswerId);
            if (bestAnswer == null)
            {
                _logger.LogWarning("Best answer not found: {AnswerId}", notificationMessage.AnswerId);
                return;
            }

            // Get the question
            var question = await GetQuestionAsync(notificationMessage.QuestionId);
            if (question == null)
            {
                _logger.LogWarning("Question not found: {QuestionId}", notificationMessage.QuestionId);
                return;
            }

            // Get the best answer author
            var bestAnswerAuthor = await GetUserAsync(bestAnswer.UserId);
            if (bestAnswerAuthor == null)
            {
                _logger.LogWarning("Best answer author not found: {UserId}", bestAnswer.UserId);
                return;
            }

            // Get all users who answered this question
            var allAnswers = await GetAllAnswersForQuestionAsync(notificationMessage.QuestionId);
            var uniqueUserIds = allAnswers
                .Select(a => a.UserId)
                .Distinct()
                .ToList();

            var emailsSent = 0;

            // Send emails to all users who answered the question
            foreach (var userId in uniqueUserIds)
            {
                var user = await GetUserAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string emailBody;
                    string subject;
                    
                    // Check if this user is the author of the best answer
                    if (userId == bestAnswer.UserId)
                    {
                        // Email for the best answer author
                        emailBody = _emailService.CreateBestAnswerSelectedEmailBody(
                            $"{user.FirstName} {user.LastName}".Trim(),
                            question.Title,
                            bestAnswer.Text
                        );
                        subject = $"Congratulations! Your Answer Was Selected: {question.Title}";
                    }
                    else
                    {
                        // Email for other users who answered
                        emailBody = _emailService.CreateQuestionClosedEmailBody(
                            $"{user.FirstName} {user.LastName}".Trim(),
                            question.Title,
                            bestAnswerAuthor.Username,
                            bestAnswer.Text
                        );
                        subject = $"Question Closed: {question.Title}";
                    }

                    var emailSent = await _emailService.SendEmailAsync(
                        user.Email,
                        $"{user.FirstName} {user.LastName}".Trim(),
                        subject,
                        emailBody
                    );

                    if (emailSent)
                    {
                        emailsSent++;
                        _logger.LogInformation("Email sent to user: {Email} (Type: {EmailType})", 
                            user.Email, userId == bestAnswer.UserId ? "Best Answer Author" : "Other Answerer");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send email to user: {Email}", user.Email);
                    }

                    // Add a small delay between emails to avoid overwhelming the SMTP server
                    await Task.Delay(1000);
                }
            }

            // Log the notification processing
            await LogNotificationAsync(notificationMessage.AnswerId, notificationMessage.QuestionId, 
                emailsSent, question.Title, bestAnswerAuthor.Username, bestAnswer.Text);

            _logger.LogInformation("Notification processing completed. Emails sent: {EmailsSent}", emailsSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification message");
            throw;
        }
    }

    private async Task<Comment?> GetCommentAsync(string commentId)
    {
        try
        {
            if (_commentsTableClient == null)
            {
                _logger.LogError("Comments table client is not initialized");
                return null;
            }
            
            var response = await _commentsTableClient.GetEntityAsync<Comment>("COMMENT", commentId);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment: {CommentId}", commentId);
            return null;
        }
    }

    private async Task<Question?> GetQuestionAsync(string questionId)
    {
        try
        {
            if (_questionsTableClient == null)
            {
                _logger.LogError("Questions table client is not initialized");
                return null;
            }
            
            var response = await _questionsTableClient.GetEntityAsync<Question>("QUESTION", questionId);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting question: {QuestionId}", questionId);
            return null;
        }
    }

    private async Task<User?> GetUserAsync(string userId)
    {
        try
        {
            if (_usersTableClient == null)
            {
                _logger.LogError("Users table client is not initialized");
                return null;
            }
            
            var response = await _usersTableClient.GetEntityAsync<User>("USER", userId);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return null;
        }
    }

    private async Task<List<Comment>> GetAllAnswersForQuestionAsync(string questionId)
    {
        var comments = new List<Comment>();
        try
        {
            if (_commentsTableClient == null)
            {
                _logger.LogError("Comments table client is not initialized");
                return comments;
            }
            
            await foreach (var comment in _commentsTableClient.QueryAsync<Comment>(
                c => c.QuestionId == questionId))
            {
                comments.Add(comment);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting answers for question: {QuestionId}", questionId);
        }
        return comments;
    }

    private async Task LogNotificationAsync(string answerId, string questionId, int emailsSent, 
        string questionTitle, string bestAnswerAuthor, string bestAnswerContent)
    {
        try
        {
            if (_notificationLogTableClient == null)
            {
                _logger.LogError("Notification log table client is not initialized");
                return;
            }
            
            var log = new NotificationLog
            {
                RowKey = Guid.NewGuid().ToString(),
                AnswerId = answerId,
                QuestionId = questionId,
                EmailsSent = emailsSent,
                ProcessedAt = DateTime.UtcNow,
                QuestionTitle = questionTitle,
                BestAnswerAuthor = bestAnswerAuthor,
                BestAnswerContent = bestAnswerContent,
                Timestamp = DateTimeOffset.UtcNow
            };

            await _notificationLogTableClient.AddEntityAsync(log);
            _logger.LogInformation("Notification log saved: {LogId}", log.RowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving notification log");
        }
    }
}
