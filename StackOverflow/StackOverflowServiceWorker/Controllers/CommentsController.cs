using Microsoft.AspNetCore.Mvc;
using StackOverflow.Models;
using StackOverflow.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost("{commentId}/upvote")]
        public async Task<IActionResult> UpvoteComment(string commentId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var (upvotes, downvotes, totalVotes) = await _commentService.UpvoteCommentAsync(commentId, request.UserId);
                var userVote = await _commentService.GetUserVoteAsync(request.UserId, commentId);
                return Ok(new { 
                    upvotes = upvotes,
                    downvotes = downvotes,
                    totalVotes = totalVotes,
                    userVote = userVote
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while upvoting the comment.");
            }
        }

        [HttpPost("{commentId}/downvote")]
        public async Task<IActionResult> DownvoteComment(string commentId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            try
            {
                var (upvotes, downvotes, totalVotes) = await _commentService.DownvoteCommentAsync(commentId, request.UserId);
                var userVote = await _commentService.GetUserVoteAsync(request.UserId, commentId);
                return Ok(new { 
                    upvotes = upvotes,
                    downvotes = downvotes,
                    totalVotes = totalVotes,
                    userVote = userVote
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while downvoting the comment.");
            }
        }

        [HttpGet("{commentId}/vote/{userId}")]
        public async Task<IActionResult> GetUserVote(string commentId, string userId)
        {
            try
            {
                var userVote = await _commentService.GetUserVoteAsync(userId, commentId);
                return Ok(new { userVote = userVote });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while getting user vote.");
            }
        }
    }

    public class VoteRequest
    {
        public required string UserId { get; set; }
    }

    public class CommentRequest
    {
        public required string Text { get; set; }
        public required string UserId { get; set; }
        public required string QuestionId { get; set; }
    }
}
