using Common.Models;
using StackOverflowServiceWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace StackOverflowServiceWeb.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly UserService _userService;
        private readonly CommentService _commentService;

        public UsersController(UserService userService, CommentService commentService)
        {
            _userService = userService;
            _commentService = commentService;
        }

        // POST api/users/register
        [HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register([FromBody] User user)
        {
            try
            {
                var existing = (await _userService.GetAllUsersAsync())
                    .FirstOrDefault(u => u.Email == user.Email);
                if (existing != null)
                    return BadRequest("Email already exists.");

                user.PartitionKey = "USER";
                user.RowKey = Guid.NewGuid().ToString();
                user.CreatedDate = DateTime.UtcNow;
                user.IsAdmin = false;
                user.QuestionsCount = 0;
                user.AnswersCount = 0;

                var created = await _userService.CreateUserAsync(user);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest("Registration failed: " + ex.Message);
            }
        }

        // POST api/users/login
        [HttpPost, Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
                if (user == null)
                    return Unauthorized();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest("Login failed: " + ex.Message);
            }
        }

        // GET api/users/{id}
        [HttpGet, Route("{id}")]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to get user: " + ex.Message);
            }
        }

        // GET api/users/{id}/stats
        [HttpGet, Route("{id}/stats")]
        public async Task<IHttpActionResult> GetUserStats(string id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                var answersCount = await _commentService.GetUserAnswersCountAsync(id);

                var userWithStats = new
                {
                    user.RowKey,
                    user.PartitionKey,
                    user.FirstName,
                    user.LastName,
                    user.Username,
                    user.Email,
                    user.Country,
                    user.City,
                    user.StreetAddress,
                    user.Gender,
                    user.ProfilePictureUrl,
                    user.CreatedDate,
                    user.IsAdmin,
                    user.QuestionsCount,
                    AnswersCount = answersCount
                };

                return Ok(userWithStats);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to get user stats: " + ex.Message);
            }
        }

        // PUT api/users/{id}
        [HttpPut, Route("{id}")]
        public async Task<IHttpActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                // Sačuvaj originalne vrednosti
                var originalRowKey = user.RowKey;
                var originalPartitionKey = user.PartitionKey;
                var originalETag = user.ETag;
                var originalCreatedDate = user.CreatedDate;

                // Update polja
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Username = updatedUser.Username;
                user.Email = updatedUser.Email;
                user.Country = updatedUser.Country;
                user.City = updatedUser.City;
                user.StreetAddress = updatedUser.StreetAddress;
                user.Gender = updatedUser.Gender;

                if (!string.IsNullOrEmpty(updatedUser.ProfilePictureUrl))
                {
                    user.ProfilePictureUrl = updatedUser.ProfilePictureUrl;
                }

                if (updatedUser.IsAdmin.HasValue)
                {
                    user.IsAdmin = updatedUser.IsAdmin;
                }

                // Vrati originalne ključne vrednosti
                user.RowKey = originalRowKey;
                user.PartitionKey = originalPartitionKey;
                user.ETag = originalETag;
                user.CreatedDate = originalCreatedDate;

                await _userService.UpdateUserAsync(user);

                var refreshedUser = await _userService.GetUserAsync(id);
                return Ok(refreshedUser ?? user);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to update user: " + ex.Message);
            }
        }

        // POST api/users/{id}/profile-picture
        [HttpPost, Route("{id}/profile-picture")]
        public async Task<IHttpActionResult> UploadProfilePicture(string id)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count == 0)
                    return BadRequest("No file uploaded");

                var user = await _userService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                var file = httpRequest.Files[0];
                using (var stream = file.InputStream)
                {
                    var imageUrl = await _userService.UploadProfilePictureAsync(id, stream, file.FileName);
                    user.ProfilePictureUrl = imageUrl;
                    await _userService.UpdateUserAsync(user);

                    return Ok(new { imageUrl });
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to upload image: " + ex.Message);
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}