using Microsoft.AspNetCore.Mvc;
using StackOverflow.Models;
using StackOverflow.Services;
using System;
using System.Threading.Tasks;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly CommentService _commentService;

        public UsersController(UserService userService, CommentService commentService)
        {
            _userService = userService;
            _commentService = commentService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
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
                
                Console.WriteLine($"User registered with RowKey: {created.RowKey}");
                return Ok(created);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return BadRequest($"Registration failed: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                Console.WriteLine($"Login attempt for: {req.Email}");
                var users = await _userService.GetAllUsersAsync();
                Console.WriteLine($"Found {users.Count} users");
                var user = users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
                
                if (user == null)
                    return Unauthorized("Invalid email or password.");

                Console.WriteLine($"User found - RowKey: {user.RowKey}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Login failed: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
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
                Console.WriteLine($"Get user error: {ex.Message}");
                return BadRequest($"Failed to get user: {ex.Message}");
            }
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetUserStats(string id)
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
                Console.WriteLine($"Get user stats error: {ex.Message}");
                return BadRequest($"Failed to get user stats: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            try
            {
                Console.WriteLine($"Attempting to update user with ID: {id}");
                var user = await _userService.GetUserAsync(id);
                if (user == null)
                {
                    Console.WriteLine($"User not found with ID: {id}");
                    return NotFound();
                }

                // Očuvaj originalne vrednosti koje ne smeju biti promenjene
                var originalRowKey = user.RowKey;
                var originalPartitionKey = user.PartitionKey;
                var originalETag = user.ETag;
                var originalCreatedDate = user.CreatedDate;

                // Update all fields
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Username = updatedUser.Username;
                user.Email = updatedUser.Email;
                user.Country = updatedUser.Country;
                user.City = updatedUser.City;
                user.StreetAddress = updatedUser.StreetAddress;
                user.Gender = updatedUser.Gender;
                
                // Samo ažuriraj ProfilePictureUrl ako je nova vrednost postavljena
                if (!string.IsNullOrEmpty(updatedUser.ProfilePictureUrl))
                {
                    user.ProfilePictureUrl = updatedUser.ProfilePictureUrl;
                }
                
                // Očuvaj admin status osim ako nije eksplicitno postavljen
                if (updatedUser.IsAdmin.HasValue)
                {
                    user.IsAdmin = updatedUser.IsAdmin;
                }

                // Očuvaj ključne vrednosti
                user.RowKey = originalRowKey;
                user.PartitionKey = originalPartitionKey;
                user.ETag = originalETag;
                user.CreatedDate = originalCreatedDate;

                Console.WriteLine($"Updating user: {user.FirstName} {user.LastName}");
                await _userService.UpdateUserAsync(user);
                
                // Ponovo učitaj korisnika iz baze da dobiješ najnovije podatke
                var refreshedUser = await _userService.GetUserAsync(id);
                Console.WriteLine($"User updated successfully: {refreshedUser?.FirstName} {refreshedUser?.LastName}");
                
                return Ok(refreshedUser ?? user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update user error: {ex.Message}");
                return BadRequest($"Failed to update user: {ex.Message}");
            }
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                var user = await _userService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                var imageUrl = await _userService.UploadProfilePictureAsync(id, file);
                user.ProfilePictureUrl = imageUrl;
                await _userService.UpdateUserAsync(user);

                Console.WriteLine($"Profile picture updated for user {id}: {imageUrl}");
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile picture upload error: {ex.Message}");
                return BadRequest($"Failed to upload image: {ex.Message}");
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}