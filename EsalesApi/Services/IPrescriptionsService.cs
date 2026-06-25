using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface IPrescriptionsService
    {
        Task<TransactResult1> GetOrderLense(int salesId);
        Task<TransactResult> GetPrescriptionDropDowns();
        Task<TransactResult> SaveOrderLense(OrderLenseItemsCls order);
    }
}
