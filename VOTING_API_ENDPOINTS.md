# Advanced Voting System

This document describes the advanced voting functionality implemented for questions and answers in the StackOverflow application.

## Key Features

1. **One Vote Per User**: Each user can give only one vote (upvote OR downvote) per question/answer
2. **Vote Toggle**: Clicking the same vote type again removes the vote
3. **Vote Switch**: Users can change from upvote to downvote and vice versa
4. **Total Votes = Total Participants**: Shows how many people voted, not the difference
5. **Visual Feedback**: Shows which vote type the user has selected

## Database Schema

### Vote Entity
```csharp
public class Vote : Entity
{
    public string UserId { get; set; }          // Who voted
    public string TargetId { get; set; }       // Question or Comment ID
    public string TargetType { get; set; }     // "QUESTION" or "COMMENT"
    public bool IsUpvote { get; set; }          // true = upvote, false = downvote
}
```

### Updated Models
- `Question` and `Comment` models no longer have Upvote/Downvote fields
- Vote counts are calculated dynamically from the Vote table

## API Endpoints

### Question Voting
- **POST** `/api/questions/{questionId}/upvote`
- **POST** `/api/questions/{questionId}/downvote`
- **GET** `/api/questions/{questionId}/vote/{userId}`

### Answer Voting
- **POST** `/api/comments/{commentId}/upvote`
- **POST** `/api/comments/{commentId}/downvote`
- **GET** `/api/comments/{commentId}/vote/{userId}`

### Request Format
```json
{
  "UserId": "user-id-here"
}
```

### Response Format
```json
{
  "upvotes": 5,
  "downvotes": 2,
  "totalVotes": 7,
  "userVote": "upvote" // or "downvote" or null
}
```

## Frontend Implementation

### State Management
- `questionVoteState`: Tracks question vote status
- `answerVoteStates`: Tracks vote status for each answer
- Visual indicators for user's current vote

### Voting Logic
1. User must be logged in to vote
2. API call includes user ID
3. Server handles vote logic (add/remove/change)
4. UI updates with new vote counts and user's vote status
5. Visual feedback shows active vote type

### CSS Classes
- `.voted-up`: Blue background for upvoted items
- `.voted-down`: Red background for downvoted items

## Vote Calculation Logic

```csharp
// In VoteService.VoteAsync()
var existingVote = await GetUserVoteAsync(userId, targetId, targetType);

if (existingVote != null) {
    if (existingVote.IsUpvote == isUpvote) {
        // Same vote type - remove vote (toggle off)
        await DeleteVote(existingVote);
    } else {
        // Different vote type - update vote
        existingVote.IsUpvote = isUpvote;
        await UpdateVote(existingVote);
    }
} else {
    // No existing vote - create new vote
    await CreateVote(userId, targetId, targetType, isUpvote);
}
```

## Example Usage

```javascript
// User clicks upvote on question
const response = await fetch(`/api/questions/123/upvote`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ UserId: 'user456' })
});

const result = await response.json();
// { "upvotes": 3, "downvotes": 1, "totalVotes": 4, "userVote": "upvote" }
```

This system ensures democratic voting where each user's voice counts equally, and the total vote count represents community engagement rather than score differential.
