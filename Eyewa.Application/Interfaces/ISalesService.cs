using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;


namespace Eyewa.Application.Interfaces
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





