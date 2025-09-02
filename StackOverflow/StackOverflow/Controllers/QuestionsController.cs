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
        private readonly UserService _userService;

        public QuestionsController(QuestionService questionService, UserService userService)
        {
            _questionService = questionService;
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
                    // Log the exception
                    return StatusCode(500, "An error occurred while uploading the picture.");
                }
            }

            var createdQuestion = await _questionService.CreateQuestionAsync(question);

            // Update user's questions count
            try
            {
                await _userService.IncrementUserQuestionsCountAsync(model.UserId);
            }
            catch (Exception)
            {
                // Log the exception, but don't fail the operation
                // The question was created successfully
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
            return Ok(questionDetails);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] QuestionUpdateModel model)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            // Check if the user owns this question
            if (question.UserId != model.UserId)
            {
                return Forbid("You can only edit your own questions.");
            }

            // Update question properties
            question.Title = model.Title;
            question.Description = model.Description;

            // Handle picture update
            if (model.Picture != null)
            {
                try
                {
                    // Delete old picture if exists
                    if (!string.IsNullOrEmpty(question.PictureUrl))
                    {
                        await _questionService.DeletePictureAsync(question.PictureUrl);
                    }

                    // Upload new picture
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
                // Remove existing picture
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

            // Check if the user owns this question
            if (question.UserId != userId)
            {
                return Forbid("You can only delete your own questions.");
            }

            try
            {
                await _questionService.DeleteQuestionAsync(id);
                
                // Update user's questions count
                try
                {
                    await _userService.DecrementUserQuestionsCountAsync(userId);
                }
                catch (Exception)
                {
                    // Log the exception, but don't fail the operation
                    // The question was deleted successfully
                }
                
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the question.");
            }
        }
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
}
