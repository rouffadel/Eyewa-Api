using Microsoft.AspNetCore.Identity;
using System;

namespace Eyewa.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int RoleId { get; set; } = 2; // Default to standard user role
        public string TenantId { get; set; } = Guid.NewGuid().ToString();
        public int LegacyLoginId { get; set; }
    }
}
