using Eyewa.Application.DTOs;
using Eyewa.Application.DTOs;
using Eyewa.Domain.Entities;
using Eyewa.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using static Eyewa.Application.DTOs.Common;


namespace Eyewa.Application.Interfaces
{
    public interface IStoresService
    {
        Task<TransactResult> FillStore(int loginId, int storeId);
        Task<TransactResult> GetUserStores(ClaimsPrincipal user);
    }
}





