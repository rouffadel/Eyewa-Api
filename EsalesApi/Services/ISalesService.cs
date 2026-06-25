using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface ISalesService
    {
        Task<TransactResult> InsertSales(SalesCls sales);
        Task<TransactResult> SaveSalesDetails(SaveSalesDetails save);
        Task<TransactResult> GetSalesGrid(SearchSalesCls obj);
        Task<TransactResult> GetInvoiceDetails(int salesId);
        Task<TransactResult> GetSalesPrint(int salesId);
        Task<TransactResult> GetZatcaQrBySalesId(int salesId);
        Task<TransactResult> GetSalesDetailsGrid(int salesId);
        Task<TransactResult> DeleteSales(int salesId, int loginId);
        Task<TransactResult> DeleteSalesDetails(int salesId, int loginId, int salesDetailId);
        Task<TransactResult> CustomerSearchFilter(string mobileNumber);
    }
}
