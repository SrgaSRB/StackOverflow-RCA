using Microsoft.AspNetCore.Mvc;
using StackOverflow.Models;
using StackOverflow.Services;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            if (comment == null || string.IsNullOrEmpty(comment.Text) || string.IsNullOrEmpty(comment.UserId) || string.IsNullOrEmpty(comment.QuestionId))
            {
                return BadRequest("Invalid comment data.");
            }

            comment.RowKey = Guid.NewGuid().ToString();
            comment.Timestamp = DateTimeOffset.UtcNow;

            await _commentService.AddCommentAsync(comment);

            return CreatedAtAction(nameof(CreateComment), new { id = comment.RowKey }, comment);
        }

        [HttpGet("question/{questionId}")]
        public async Task<IActionResult> GetCommentsForQuestion(string questionId)
        {
            IEnumerable<object> comments = await _commentService.GetCommentsForQuestionAsync(questionId);
            return Ok(comments);
        }
    }

    public class CommentRequest
    {
        public required string Text { get; set; }
        public required string UserId { get; set; }
        public required string QuestionId { get; set; }
    }
}
