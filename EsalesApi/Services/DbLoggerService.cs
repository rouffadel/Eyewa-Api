using Eyewa_new_api.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Eyewa_new_api.Services
{
    public class DbLoggerService : IDbLoggerService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbLoggerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LogInfoAsync(string message)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var log = new DbLog
                    {
                        Message = message,
                        Level = "Info",
                        Timestamp = DateTime.UtcNow
                    };
                    context.DbLogs.Add(log);
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                // Fallback to avoid breaking application execution flow if logging fails
            }
        }

        public async Task LogErrorAsync(string message, string? exception = null)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var log = new DbLog
                    {
                        Message = message,
                        Level = "Error",
                        Timestamp = DateTime.UtcNow,
                        Exception = exception
                    };
                    context.DbLogs.Add(log);
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                // Fallback
            }
        }

        public void LogInfo(string message)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var log = new DbLog
                    {
                        Message = message,
                        Level = "Info",
                        Timestamp = DateTime.UtcNow
                    };
                    context.DbLogs.Add(log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }

        public void LogError(string message, string? exception = null)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var log = new DbLog
                    {
                        Message = message,
                        Level = "Error",
                        Timestamp = DateTime.UtcNow,
                        Exception = exception
                    };
                    context.DbLogs.Add(log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }
    }
}
