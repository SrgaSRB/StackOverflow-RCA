using Common.Models;
using StackOverflowServiceWeb.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace StackOverflowServiceWeb.Controllers
{
    [RoutePrefix("api/questions")]
    public class QuestionsController : ApiController
    {
        private readonly QuestionService _questionService;
        private readonly CommentService _commentService;
        private readonly UserService _userService;
        private readonly VoteService _voteService;
        private readonly NotificationQueueService _notificationQueueService;

        public QuestionsController(
            QuestionService questionService,
            CommentService commentService,
            UserService userService,
            VoteService voteService,
            NotificationQueueService notificationQueueService)
        {
            _questionService = questionService;
            _commentService = commentService;
            _userService = userService;
            _voteService = voteService;
            _notificationQueueService = notificationQueueService;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var questions = await _questionService.GetAllQuestionsWithUserDetailsAsync();
            return Ok(questions);
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Create()
        {
            // čitanje form-data (Web API nema IFormFile)
            var httpRequest = System.Web.HttpContext.Current.Request;

            var title = httpRequest.Form["Title"];
            var description = httpRequest.Form["Description"];
            var userId = httpRequest.Form["UserId"];

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("Missing required fields.");

            var question = new Question
            {
                PartitionKey = "QUESTION",
                RowKey = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            if (httpRequest.Files.Count > 0)
            {
                try
                {
                    var file = httpRequest.Files[0];
                    using (var stream = file.InputStream)
                    {
                        var pictureUrl = await _questionService.UploadQuestionPictureAsync(stream, file.FileName);
                        question.PictureUrl = pictureUrl;
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("Error uploading picture: " + ex.Message));
                }
            }

            var created = await _questionService.CreateQuestionAsync(question);

            try
            {
                await _userService.IncrementUserQuestionsCountAsync(userId);
            }
            catch { }

            var location = new Uri(Request.RequestUri, created.RowKey);
            return Created(location, created);
        }

        [HttpGet, Route("user/{userId}")]
        public async Task<IHttpActionResult> GetByUserId(string userId)
        {
            var questions = await _questionService.GetQuestionsByUserIdWithDetailsAsync(userId);
            return Ok(questions);
        }

        [HttpGet, Route("{id}")]
        public async Task<IHttpActionResult> GetById(string id)
        {
            var questionDetails = await _questionService.GetQuestionWithDetailsAsync(id);
            if (questionDetails == null)
                return NotFound();

            var question = await _questionService.GetQuestionByIdAsync(id);
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

            return Ok(new
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
            });
        }

        [HttpPut, Route("{id}")]
        public async Task<IHttpActionResult> Update(string id)
        {
            Question question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            var httpRequest = System.Web.HttpContext.Current.Request;
            var userId = httpRequest.Form["userId"] ?? httpRequest.Form["UserId"];
            if (question.UserId != userId)
                return ResponseMessage(Request.CreateErrorResponse(System.Net.HttpStatusCode.Forbidden, "You can only edit your own questions."));

            // Accept both camelCase and PascalCase for form fields
            question.Title = httpRequest.Form["title"] ?? httpRequest.Form["Title"];
            question.Description = httpRequest.Form["description"] ?? httpRequest.Form["Description"];

            // Handle image upload/removal
            if (httpRequest.Files.Count > 0)
            {
                if (!string.IsNullOrEmpty(question.PictureUrl))
                    await _questionService.DeletePictureAsync(question.PictureUrl);

                var file = httpRequest.Files[0];
                using (var stream = file.InputStream)
                {
                    var pictureUrl = await _questionService.UploadQuestionPictureAsync(stream, file.FileName);
                    question.PictureUrl = pictureUrl;
                }
            }
            else if ((!string.IsNullOrEmpty(httpRequest.Form["removePicture"]) || !string.IsNullOrEmpty(httpRequest.Form["RemovePicture"])) && question.PictureUrl != null)
            {
                await _questionService.DeletePictureAsync(question.PictureUrl);
                question.PictureUrl = null;
            }

            await _questionService.UpdateQuestionAsync(question);
            return Ok(question);
        }

        [HttpDelete, Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id, [FromUri] string userId)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound();
            if (question.UserId != userId) return ResponseMessage(Request.CreateErrorResponse(System.Net.HttpStatusCode.Forbidden, "You can only delete your own questions."));

            await _questionService.DeleteQuestionAsync(id);
            try { await _userService.DecrementUserQuestionsCountAsync(userId); } catch { }

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        [HttpPost, Route("{questionId}/answers")]
        public async Task<IHttpActionResult> AddAnswer(string questionId, [FromBody] CreateCommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content) || string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest("Invalid answer data.");

            var comment = new Comment
            {
                PartitionKey = "COMMENT",
                RowKey = Guid.NewGuid().ToString(),
                QuestionId = questionId,
                UserId = dto.UserId,
                Text = dto.Content
            };

            await _commentService.AddCommentAsync(comment);
            var user = await _userService.GetUserAsync(dto.UserId);

            return Ok(new
            {
                AnswerId = comment.RowKey,
                Content = comment.Text,
                CreatedAt = comment.Timestamp,
                Upvotes = 0,
                Downvotes = 0,
                TotalVotes = 0,
                User = new { Username = user?.Username, ProfilePictureUrl = user?.ProfilePictureUrl }
            });
        }

        [HttpPost, Route("{questionId}/upvote")]
        public async Task<IHttpActionResult> UpvoteQuestion(string questionId, [FromBody] VoteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            var (up, down, tot) = await _questionService.UpvoteQuestionAsync(questionId, request.UserId);
            var userVote = await _questionService.GetUserVoteAsync(request.UserId, questionId);

            return Ok(new { upvotes = up, downvotes = down, totalVotes = tot, userVote });
        }

        [HttpPost, Route("{questionId}/downvote")]
        public async Task<IHttpActionResult> DownvoteQuestion(string questionId, [FromBody] VoteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            var (up, down, tot) = await _questionService.DownvoteQuestionAsync(questionId, request.UserId);
            var userVote = await _questionService.GetUserVoteAsync(request.UserId, questionId);

            return Ok(new { upvotes = up, downvotes = down, totalVotes = tot, userVote });
        }

        [HttpGet, Route("{questionId}/vote/{userId}")]
        public async Task<IHttpActionResult> GetUserVote(string questionId, string userId)
        {
            var userVote = await _questionService.GetUserVoteAsync(userId, questionId);
            return Ok(new { userVote });
        }

        [HttpPost, Route("migrate-vote-counts")]
        public async Task<IHttpActionResult> MigrateVoteCounts()
        {
            await _voteService.MigrateVoteCountsAsync();
            return Ok(new { message = "Vote counts migration completed successfully." });
        }

        [HttpGet, Route("popular")]
        public async Task<IHttpActionResult> GetPopularQuestions([FromUri] int limit = 5)
        {
            var questions = await _questionService.GetPopularQuestionsAsync(limit);
            return Ok(questions);
        }

        [HttpPost, Route("{questionId}/best-answer/{answerId}")]
        public async Task<IHttpActionResult> MarkBestAnswer(string questionId, string answerId, [FromBody] MarkBestAnswerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            var question = await _questionService.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound();
            if (question.UserId != request.UserId) return ResponseMessage(Request.CreateErrorResponse(System.Net.HttpStatusCode.Forbidden, "Only the question author can mark the best answer"));

            var answer = await _commentService.GetCommentByIdAsync(answerId);
            if (answer == null || answer.QuestionId != questionId)
                return NotFound();

            await _questionService.MarkBestAnswerAsync(questionId, answerId);

            try
            {
                var notification = new NotificationMessage
                {
                    AnswerId = answerId,
                    QuestionId = questionId,
                    Timestamp = DateTime.UtcNow
                };
                //await _notificationQueueService.SendNotificationAsync(notification);
            }
            catch { }

            return Ok(new { message = "Best answer marked successfully" });
        }

        [HttpDelete, Route("{questionId}/best-answer")]
        public async Task<IHttpActionResult> UnmarkBestAnswer(string questionId, [FromBody] MarkBestAnswerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            var question = await _questionService.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound();
            if (question.UserId != request.UserId) return ResponseMessage(Request.CreateErrorResponse(System.Net.HttpStatusCode.Forbidden, "Only the question author can unmark the best answer"));

            await _questionService.UnmarkBestAnswerAsync(questionId);
            return Ok(new { message = "Best answer unmarked successfully" });
        }
    }

    public class VoteRequest { public string UserId { get; set; } }
    public class CreateCommentDto { public string Content { get; set; } public string UserId { get; set; } }
    public class MarkBestAnswerRequest { public string UserId { get; set; } }
}