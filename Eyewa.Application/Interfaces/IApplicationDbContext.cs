using static Eyewa.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using Eyewa.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Eyewa.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        DbSet<NotificationSettings> NotificationSettings { get; set; }
        DbSet<DbLog> DbLogs { get; set; }
        DbSet<TenantFeatureAccess> TenantFeatureAccesses { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}



