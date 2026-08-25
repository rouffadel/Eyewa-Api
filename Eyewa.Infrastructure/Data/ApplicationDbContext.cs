using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eyewa.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<DbLog> DbLogs { get; set; }
        public DbSet<TenantFeatureAccess> TenantFeatureAccesses { get; set; }
        public DbSet<Tax> Taxes { get; set; }
    }
}




