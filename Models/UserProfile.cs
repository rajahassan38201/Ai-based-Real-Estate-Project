namespace PakProperties.Models
{
    public class UserProfile
    {
        public string Id { get; set; } // IdentityUser's Id
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        // Add other profile properties as needed
    }
}