using Microsoft.AspNetCore.Mvc;
using StackOverflow.Models;
using StackOverflow.Services;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // POST /api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Provera da li već postoji korisnik sa istim emailom
            var existing = (await _userService.GetAllUsersAsync())
                .FirstOrDefault(u => u.Email == user.Email);
            if (existing != null)
                return BadRequest("Email already exists.");

            user.PartitionKey = "USER";
            user.RowKey = Guid.NewGuid().ToString();
            var created = await _userService.CreateUserAsync(user);
            return Ok(created);
        }

        // POST /api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var users = await _userService.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Ovde možeš vratiti token ili podatke o korisniku
            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}