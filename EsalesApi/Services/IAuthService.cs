using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface IAuthService
    {
        Task<TransactResult> Register(string username, string password, int roleId);
        Task<TransactResult> LoginWithIdentity(string username, string password, string ipAddress);
        Task<TransactResult> RefreshToken(string token, string ipAddress);
        Task<TransactResult> RevokeToken(string token, string ipAddress);
        Task<TransactResult> VerifyUserLogin(string loginName, string password, string ipAddress);
        Task<TransactResult> GetUsers(int loginId, int storeId);
        Task<TransactResult> GetSalesMan(int loginId, int storeId);
        Task<List<Dictionary<string, object>>> Echeckpermissions(int roleId);
        Task SyncLegacyUsersAsync();
    }
}
