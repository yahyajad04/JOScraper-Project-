using Microsoft.AspNetCore.Identity;

namespace JoSale.Models
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
