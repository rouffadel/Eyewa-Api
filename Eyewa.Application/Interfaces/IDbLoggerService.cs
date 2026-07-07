using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;

namespace Eyewa.Application.Interfaces
{
    public interface IDbLoggerService
    {
        Task LogInfoAsync(string message);
        Task LogErrorAsync(string message, string? exception = null);
        void LogInfo(string message);
        void LogError(string message, string? exception = null);
    }
}





