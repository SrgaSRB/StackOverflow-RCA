using Microsoft.AspNetCore.Mvc;
using StackOverflow.Models;
using StackOverflow.Services;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuestionService _questionService;

        public QuestionsController(QuestionService questionService)
        {
            _questionService = questionService;
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
                catch (Exception ex)
                {
                    // Log the exception
                    return StatusCode(500, "An error occurred while uploading the picture.");
                }
            }

            var createdQuestion = await _questionService.CreateQuestionAsync(question);

            return CreatedAtAction(nameof(Create), new { id = createdQuestion.RowKey }, createdQuestion);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var questions = await _questionService.GetQuestionsByUserIdAsync(userId);
            return Ok(questions);
        }
    }

    public class QuestionCreateModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string UserId { get; set; }
        public IFormFile? Picture { get; set; }
    }
}
