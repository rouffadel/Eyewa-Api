using Microsoft.AspNetCore.Identity;

namespace Eyewa_new_api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int RoleId { get; set; } = 2; // Default to standard user role
    }
}
