using System.Threading.Tasks;

namespace Eyewa_new_api.Services
{
    public interface IDbLoggerService
    {
        Task LogInfoAsync(string message);
        Task LogErrorAsync(string message, string? exception = null);
        void LogInfo(string message);
        void LogError(string message, string? exception = null);
    }
}
