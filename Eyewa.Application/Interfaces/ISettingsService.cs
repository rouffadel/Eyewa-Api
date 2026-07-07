using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;


namespace Eyewa.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<TransactResult> GetNotificationSettings();
        Task<TransactResult> SaveNotificationSettings(NotificationSettings settings);
    }
}





