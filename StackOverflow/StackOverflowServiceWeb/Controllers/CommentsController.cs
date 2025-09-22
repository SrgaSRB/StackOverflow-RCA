using Azure.Core;
using Common.Models;
using StackOverflowServiceWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace StackOverflowServiceWeb.Controllers
{
    [RoutePrefix("api/comments")]
    public class CommentsController : ApiController
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // POST api/comments
        [HttpPost, Route("")]
        public async Task<IHttpActionResult> CreateComment([FromBody] CommentRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Text) ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.QuestionId))
            {
                return BadRequest("Invalid comment data.");
            }

            var comment = new Comment
            {
                PartitionKey = "COMMENT",
                RowKey = Guid.NewGuid().ToString(),
                QuestionId = request.QuestionId,
                UserId = request.UserId,
                Text = request.Text,
                // Upvotes/Downvotes/TotalVotes ostaju 0 (TableEntity.Timestamp popunjava storage)
            };

            await _commentService.AddCommentAsync(comment);

            // Created (201) + Location header
            var location = new Uri(Request.RequestUri, comment.RowKey);
            return Created(location, comment);
        }

        // GET api/comments/question/{questionId}
        [HttpGet, Route("question/{questionId}")]
        public async Task<IHttpActionResult> GetCommentsForQuestion(string questionId)
        {
            if (string.IsNullOrWhiteSpace(questionId))
                return BadRequest("questionId is required.");

            var comments = await _commentService.GetCommentsForQuestionAsync(questionId);
            return Ok(comments);
        }

        // POST api/comments/{commentId}/upvote
        [HttpPost, Route("{commentId}/upvote")]
        public async Task<IHttpActionResult> UpvoteComment(string commentId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(commentId))
                return BadRequest("commentId is required.");
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required.");

            try
            {
                var (upvotes, downvotes, totalVotes) = await _commentService.UpvoteCommentAsync(commentId, request.UserId);
                var userVote = await _commentService.GetUserVoteAsync(request.UserId, commentId);
                return Ok(new
                {
                    upvotes,
                    downvotes,
                    totalVotes,
                    userVote
                });
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(
                    System.Net.HttpStatusCode.InternalServerError,
                    "An error occurred while upvoting the comment: " + ex.Message));
            }
        }

        // POST api/comments/{commentId}/downvote
        [HttpPost, Route("{commentId}/downvote")]
        public async Task<IHttpActionResult> DownvoteComment(string commentId, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(commentId))
                return BadRequest("commentId is required.");
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required.");

            try
            {
                var (upvotes, downvotes, totalVotes) = await _commentService.DownvoteCommentAsync(commentId, request.UserId);
                var userVote = await _commentService.GetUserVoteAsync(request.UserId, commentId);
                return Ok(new
                {
                    upvotes,
                    downvotes,
                    totalVotes,
                    userVote
                });
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(
                    System.Net.HttpStatusCode.InternalServerError,
                    "An error occurred while downvoting the comment: " + ex.Message));
            }
        }

        // GET api/comments/{commentId}/vote/{userId}
        [HttpGet, Route("{commentId}/vote/{userId}")]
        public async Task<IHttpActionResult> GetUserVote(string commentId, string userId)
        {
            if (string.IsNullOrWhiteSpace(commentId) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("commentId and userId are required.");

            try
            {
                var userVote = await _commentService.GetUserVoteAsync(userId, commentId);
                return Ok(new { userVote });
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(
                    System.Net.HttpStatusCode.InternalServerError,
                    "An error occurred while getting user vote: " + ex.Message));
            }
        }
    }

    public class CommentRequest
    {
        public string Text { get; set; }
        public string UserId { get; set; }
        public string QuestionId { get; set; }
    }
}