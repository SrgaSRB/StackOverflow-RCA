namespace StackOverflow.Models
{
    public class User : Entity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; } = false;

        public string FullName => $"{FirstName} {LastName}";
    }
}