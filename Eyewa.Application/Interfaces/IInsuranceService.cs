using System.Threading.Tasks;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Interfaces
{
    public interface IInsuranceService
    {
        Task<TransactResult> SaveInsuranceCompany(InsuranceCompanyDto obj);
        Task<TransactResult> SaveSalesInsurance(SalesInsuranceDto obj);
        Task<TransactResult> GetInsuranceBySalesId(int salesId);
        Task<TransactResult> GetAllInsuranceCompanies();
    }
}
