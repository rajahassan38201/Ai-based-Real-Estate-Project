using Microsoft.AspNetCore.Identity;

namespace PakProperties.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }
        public ICollection<Sell>? Properties { get; set; }
        public ICollection<Rent>? RentProperties { get; set; }
        public ICollection<Video>? Videos { get; set; }
    }
}
