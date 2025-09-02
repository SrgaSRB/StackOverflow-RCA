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

        public QuestionsController(QuestionService questionService, CommentService commentService, UserService userService)
        {
            _questionService = questionService;
            _commentService = commentService;
            _userService = userService;
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
            var questions = await _questionService.GetQuestionsByUserIdAsync(userId);
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

            // Get answers for this question
            var comments = await _commentService.GetCommentsForQuestionAsync(id);
            var answers = comments.Select(c => new
            {
                AnswerId = ((dynamic)c).AnswerId,
                Content = ((dynamic)c).Content,
                CreatedAt = ((dynamic)c).CreatedAt,
                TotalVotes = ((dynamic)c).TotalVotes,
                User = ((dynamic)c).User
            }).ToList();

            // Add answers to question details
            var result = new
            {
                questionDetails.QuestionId,
                questionDetails.Title,
                questionDetails.Description,
                questionDetails.PictureUrl,
                questionDetails.TotalVotes,
                questionDetails.CreatedAt,
                questionDetails.User,
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
                TotalVotes = comment.TotalVotes,
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

        public class CreateCommentDto
        {
            public required string Content { get; set; }
            public required string UserId { get; set; }
        }
    }
}
