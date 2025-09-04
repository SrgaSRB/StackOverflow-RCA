using Microsoft.AspNetCore.Mvc;
using StackOverflow.DTOs;
using StackOverflow.Models;
using StackOverflow.Services;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuestionService _questionService;
        private readonly CommentService _commentService;
        private readonly UserService _userService;
        private readonly VoteService _voteService;
        private readonly NotificationQueueService _notificationQueueService;

        public QuestionsController(QuestionService questionService, CommentService commentService, UserService userService, VoteService voteService, NotificationQueueService notificationQueueService)
        {
            _questionService = questionService;
            _commentService = commentService;
            _userService = userService;
            _voteService = voteService;
            _notificationQueueService = notificationQueueService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questions = await _questionService.GetAllQuestionsWithUserDetailsAsync();
            return Ok(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] QuestionCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var question = new Question
            {
                RowKey = Guid.NewGuid().ToString(),
                Title = model.Title,
                Description = model.Description,
                UserId = model.UserId,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (model.Picture != null)
            {
                try
                {
                    var pictureUrl = await _questionService.UploadQuestionPictureAsync(model.Picture);
                    question.PictureUrl = pictureUrl;
                }
                catch (Exception)
                {
                    return StatusCode(500, "An error occurred while uploading the picture.");
                }
            }

            var createdQuestion = await _questionService.CreateQuestionAsync(question);

            try
            {
                await _userService.IncrementUserQuestionsCountAsync(model.UserId);
            }
            catch (Exception)
            {
                // Log the exception, but don't fail the operation
            }

            return CreatedAtAction(nameof(Create), new { id = createdQuestion.RowKey }, createdQuestion);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var questions = await _questionService.GetQuestionsByUserIdWithDetailsAsync(userId);
            return Ok(questions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var questionDetails = await _questionService.GetQuestionWithDetailsAsync(id);
            if (questionDetails == null)
            {
                return NotFound();
            }

            // Get the question entity to access BestCommentId
            var question = await _questionService.GetQuestionByIdAsync(id);

            // Get answers for this question
            var comments = await _commentService.GetCommentsForQuestionAsync(id);
            var answers = comments.Select(c => new
            {
                AnswerId = ((dynamic)c).AnswerId,
                Content = ((dynamic)c).Content,
                CreatedAt = ((dynamic)c).CreatedAt,
                Upvotes = ((dynamic)c).Upvotes,
                Downvotes = ((dynamic)c).Downvotes,
                TotalVotes = ((dynamic)c).TotalVotes,
                User = ((dynamic)c).User,
                IsBestAnswer = question?.BestCommentId == ((dynamic)c).AnswerId
            }).ToList();

            // Add answers to question details
            var result = new
            {
                questionDetails.QuestionId,
                questionDetails.Title,
                questionDetails.Description,
                questionDetails.PictureUrl,
                questionDetails.Upvotes,
                questionDetails.Downvotes,
                questionDetails.TotalVotes,
                questionDetails.CreatedAt,
                questionDetails.User,
                BestCommentId = question?.BestCommentId,
                Answers = answers
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] QuestionUpdateModel model)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            if (question.UserId != model.UserId)
            {
                return Forbid("You can only edit your own questions.");
            }

            question.Title = model.Title;
            question.Description = model.Description;

            if (model.Picture != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(question.PictureUrl))
                    {
                        await _questionService.DeletePictureAsync(question.PictureUrl);
                    }

                    var pictureUrl = await _questionService.UploadQuestionPictureAsync(model.Picture);
                    question.PictureUrl = pictureUrl;
                }
                catch (Exception)
                {
                    return StatusCode(500, "An error occurred while uploading the picture.");
                }
            }
            else if (model.RemovePicture && !string.IsNullOrEmpty(question.PictureUrl))
            {
                await _questionService.DeletePictureAsync(question.PictureUrl);
                question.PictureUrl = null;
            }

            try
            {
                await _questionService.UpdateQuestionAsync(question);
                return Ok(question);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the question.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string userId)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            if (question.UserId != userId)
            {
                return Forbid("You can only delete your own questions.");
            }

            try
            {
                await _questionService.DeleteQuestionAsync(id);
                
                try
                {
                    await _userService.DecrementUserQuestionsCountAsync(userId);
                }
                catch (Exception)
                {
                    // Log the exception, but don't fail the operation
                }
                
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the question.");
            }
        }

        [HttpPost("{questionId}/answers")]
        public async Task<IActionResult> AddAnswer(string questionId, [FromBody] CreateCommentDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Content) || string.IsNullOrEmpty(dto.UserId))
            {
                return BadRequest("Invalid answer data.");
            }

            var comment = new Comment
            {
                RowKey = Guid.NewGuid().ToString(),
                Text = dto.Content,
                UserId = dto.UserId,
                QuestionId = questionId,
                Timestamp = DateTimeOffset.UtcNow
            };
            
            await _commentService.AddCommentAsync(comment);

            var user = await _userService.GetUserAsync(dto.UserId);

            var result = new
            {
                AnswerId = comment.RowKey,
                Content = comment.Text,
                CreatedAt = comment.Timestamp,
                Upvotes = 0,
                Downvotes = 0,
                TotalVotes = 0, // New comment has no votes yet
                User = new
                {
                    Username = user?.Username,
                    ProfilePictureUrl = user?.ProfilePictureUrl,
                }
            };

            return Ok(result);
        }

        public class QuestionCreateModel
        {
            public required string Title { get; set; }
            public required string Description { get; set; }
            public required string UserId { get; set; }
            public IFormFile? Picture { get; set; }
        }

        public class QuestionUpdateModel
        {
            public required string Title { get; set; }
            public required string Description { get; set; }
            public required string UserId { get; set; }
            public IFormFile? Picture { get; set; }
            public bool RemovePicture { get; set; } = false;
        }

        [HttpPost("{questionId}/upvote")]
        public async Task<IActionResult> UpvoteQuestion(string questionId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var (upvotes, downvotes, totalVotes) = await _questionService.UpvoteQuestionAsync(questionId, request.UserId);
                var userVote = await _questionService.GetUserVoteAsync(request.UserId, questionId);
                return Ok(new { 
                    upvotes = upvotes,
                    downvotes = downvotes,
                    totalVotes = totalVotes,
                    userVote = userVote
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while upvoting the question.");
            }
        }

        [HttpPost("{questionId}/downvote")]
        public async Task<IActionResult> DownvoteQuestion(string questionId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var (upvotes, downvotes, totalVotes) = await _questionService.DownvoteQuestionAsync(questionId, request.UserId);
                var userVote = await _questionService.GetUserVoteAsync(request.UserId, questionId);
                return Ok(new { 
                    upvotes = upvotes,
                    downvotes = downvotes,
                    totalVotes = totalVotes,
                    userVote = userVote
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while downvoting the question.");
            }
        }

        [HttpGet("{questionId}/vote/{userId}")]
        public async Task<IActionResult> GetUserVote(string questionId, string userId)
        {
            try
            {
                var userVote = await _questionService.GetUserVoteAsync(userId, questionId);
                return Ok(new { userVote = userVote });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while getting user vote.");
            }
        }

        public class VoteRequest
        {
            public required string UserId { get; set; }
        }

        public class CreateCommentDto
        {
            public required string Content { get; set; }
            public required string UserId { get; set; }
        }

        [HttpPost("migrate-vote-counts")]
        public async Task<IActionResult> MigrateVoteCounts()
        {
            try
            {
                await _voteService.MigrateVoteCountsAsync();
                return Ok(new { message = "Vote counts migration completed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred during migration: {ex.Message}");
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularQuestions([FromQuery] int limit = 5)
        {
            var questions = await _questionService.GetPopularQuestionsAsync(limit);
            return Ok(questions);
        }

        [HttpPost("{questionId}/best-answer/{answerId}")]
        public async Task<IActionResult> MarkBestAnswer(string questionId, string answerId, [FromBody] MarkBestAnswerRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var question = await _questionService.GetQuestionByIdAsync(questionId);
                if (question == null)
                {
                    return NotFound("Question not found");
                }

                // Check if the user is the question author
                if (question.UserId != request.UserId)
                {
                    return Forbid("Only the question author can mark the best answer");
                }

                // Verify the answer exists and belongs to this question
                var answer = await _commentService.GetCommentByIdAsync(answerId);
                if (answer == null || answer.QuestionId != questionId)
                {
                    return NotFound("Answer not found or doesn't belong to this question");
                }

                await _questionService.MarkBestAnswerAsync(questionId, answerId);
                
                // Send notification to queue
                try
                {
                    var notificationMessage = new Models.NotificationMessage
                    {
                        AnswerId = answerId,
                        QuestionId = questionId,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    await _notificationQueueService.SendNotificationAsync(notificationMessage);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the operation
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
                
                return Ok(new { message = "Best answer marked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{questionId}/best-answer")]
        public async Task<IActionResult> UnmarkBestAnswer(string questionId, [FromBody] MarkBestAnswerRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var question = await _questionService.GetQuestionByIdAsync(questionId);
                if (question == null)
                {
                    return NotFound("Question not found");
                }

                // Check if the user is the question author
                if (question.UserId != request.UserId)
                {
                    return Forbid("Only the question author can unmark the best answer");
                }

                await _questionService.UnmarkBestAnswerAsync(questionId);
                return Ok(new { message = "Best answer unmarked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public class MarkBestAnswerRequest
        {
            public required string UserId { get; set; }
        }
    }
}
