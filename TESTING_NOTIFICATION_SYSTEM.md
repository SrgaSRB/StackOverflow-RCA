# Testing the Notification System

## Prerequisites
1. Make sure Azurite is running
2. All services are running (use `start-all-services.bat`)
3. Configure email settings in NotificationService's appsettings.json

## Test Scenario

### Step 1: Create Test Users
1. Register at least 3 users in the system:
   - `testuser1@example.com` (will ask the question)
   - `testuser2@example.com` (will answer the question)
   - `testuser3@example.com` (will answer the question and get the best answer)

### Step 2: Create a Question
1. Login as `testuser1`
2. Create a question with title: "How to implement async/await in C#?"
3. Add description: "I'm confused about async/await. Can someone explain?"

### Step 3: Add Answers
1. Logout and login as `testuser2`
2. Answer the question: "You can use async/await to handle asynchronous operations without blocking the thread."

3. Logout and login as `testuser3`
4. Answer the question: "Async/await is syntactic sugar over Tasks. Use async for methods that return Task and await for calling async methods."

### Step 4: Mark Best Answer
1. Logout and login as `testuser1` (question author)
2. Navigate to the question
3. Click "Mark as Best" on `testuser3`'s answer
4. Confirm the action

### Step 5: Verify Notifications
1. Check NotificationService console logs - should show:
   ```
   Processing notification for answer: [answerId], question: [questionId]
   Email sent to user: testuser2@example.com
   Notification processing completed. Emails sent: 1
   ```

2. Check Azure Storage Explorer:
   - `notifications` queue should be empty (message processed)
   - `NotificationLogs` table should have a new entry with:
     - EmailsSent: 1
     - QuestionId: [question ID]
     - AnswerId: [best answer ID]
     - ProcessedAt: [timestamp]

3. Check email inbox:
   - `testuser2@example.com` should receive an email
   - `testuser3@example.com` should NOT receive an email (they authored the best answer)
   - Email should contain question title and best answer details

## Expected Email Content

**Subject:** Question Closed: How to implement async/await in C#?

**Body:**
```
🎉 Question Successfully Closed

Hello [User Name],

We're writing to inform you that a question you answered has been successfully closed with a best answer selected.

Question: How to implement async/await in C#?

✅ Best Answer (by @testuser3):
Async/await is syntactic sugar over Tasks. Use async for methods that return Task and await for calling async methods.

Thank you for your contribution to the community! Your participation helps make our platform a valuable resource for everyone.
```

## Troubleshooting

### No emails being sent
1. Check NotificationService logs for SMTP errors
2. Verify email configuration in appsettings.json
3. Ensure SMTP credentials are correct
4. Check if Gmail App Password is being used (not regular password)

### Notifications not processing
1. Check if Azurite is running
2. Verify queue connection string
3. Check NotificationService logs for errors
4. Ensure the queue "notifications" exists

### Messages stuck in queue
1. Check Azure Storage Explorer
2. Look for messages with high dequeue count
3. Check NotificationService error logs
4. Verify table storage connections

## Performance Testing

### Multiple Notifications
1. Create a question with many answers (5-10 users)
2. Mark a best answer
3. Verify all users (except best answer author) receive emails
4. Check processing time and success rate

### Load Testing
1. Mark multiple best answers quickly
2. Verify all notifications are processed
3. Check queue depth and processing speed
4. Monitor NotificationService instances

## Monitoring Queries

### Check notification logs
```sql
-- In Azure Storage Explorer, query NotificationLogs table:
PartitionKey eq 'NOTIFICATION_LOG'
```

### Check recent notifications
```sql
-- Filter by recent date:
PartitionKey eq 'NOTIFICATION_LOG' and ProcessedAt ge datetime'2024-12-18T00:00:00Z'
```

### Check notification statistics
Look for:
- Total emails sent today
- Failed notifications
- Processing time patterns
- Most active questions
