using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;


namespace Eyewa.Application.Interfaces
{
    public interface IPrescriptionsService
    {
        Task<TransactResult1> GetOrderLense(int salesId);
        Task<TransactResult> GetPrescriptionDropDowns();
        Task<TransactResult> SaveOrderLense(OrderLenseItemsCls order);
    }
}





