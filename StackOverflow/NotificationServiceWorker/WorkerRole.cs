using Common.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private DataService _dataService;
        private EmailService _emailService;

        public override void Run()
        {
            Trace.TraceInformation("NotificationServiceWorker is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            _dataService = new DataService();
            _emailService = new EmailService();

            bool result = base.OnStart();

            Trace.TraceInformation("NotificationServiceWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NotificationServiceWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NotificationServiceWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {

                try
                {
                    var notification = await _dataService.ReceiveNotificationAsync();

                    if (notification != null)
                    {
                        Trace.TraceInformation($"Processing notification for answer {notification.AnswerId}");
                        await ProcessNotificationAsync(notification);
                    }
                    else
                    {
                        Trace.TraceInformation("Trenutno ne postoji poruka u redu.");
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error processing notifications: {ex.Message}");
                }

                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        private async Task ProcessNotificationAsync(NotificationMessage notification)
        {
            try
            {
                Trace.TraceInformation($"Started processing notification for answer {notification.AnswerId}");

                var bestAnswer = await _dataService.GetCommentAsync(notification.AnswerId);
                if (bestAnswer == null)
                {
                    Trace.TraceWarning($"Best answer {notification.AnswerId} not found");
                    return;
                }

                var question = await _dataService.GetQuestionAsync(notification.QuestionId);
                if (question == null)
                {
                    Trace.TraceWarning($"Question {notification.QuestionId} not found");
                    return;
                }

                var bestAnswerAuthor = await _dataService.GetUserAsync(bestAnswer.UserId);
                if (bestAnswerAuthor == null)
                {
                    Trace.TraceWarning($"Best answer author {bestAnswer.UserId} not found");
                    return;
                }

                var allComments = await _dataService.GetCommentsForQuestionAsync(notification.QuestionId);

                var userIds = allComments
                    .Where(c => c.UserId != question.UserId && c.UserId != bestAnswer.UserId)
                    .Select(c => c.UserId)
                    .Distinct()
                    .ToList();

                Trace.TraceInformation($"Found {userIds.Count} users to notify");

                var users = await _dataService.GetUsersAsync(userIds);

                int emailsSent = 0;
                var emailBody = _emailService.CreateNotificationEmailBody(
                    question.Title,
                    bestAnswerAuthor.Username,
                    bestAnswer.Text
                );

                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var success = await _emailService.SendEmailAsync(
                            user.Email,
                            user.Username,
                            $"Pitanje zatvoreno: {question.Title}",
                            emailBody
                        );

                        if (success)
                        {
                            emailsSent++;
                        }
                    }
                }

                var notificationLog = new NotificationLog
                {
                    AnswerId = notification.AnswerId,
                    EmailsSent = emailsSent,
                    ProcessedAt = DateTime.UtcNow,
                    QuestionTitle = question.Title,
                    BestAnswerAuthor = bestAnswerAuthor.Username,
                    BestAnswerContent = bestAnswer.Text
                };

                await _dataService.SaveNotificationLogAsync(notificationLog);

                Trace.TraceInformation($"Notification processed successfully. Emails sent: {emailsSent}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error processing notification for answer {notification.AnswerId}: {ex.Message}");
            }
        }
    }
}

