using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface ISettingsService
    {
        Task<TransactResult> GetNotificationSettings();
        Task<TransactResult> SaveNotificationSettings(NotificationSettings settings);
    }
}
