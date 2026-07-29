using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eyewa.Application.Interfaces;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Services
{
    public class InsuranceService : IInsuranceService
    {
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;

        public InsuranceService(IDbLoggerService dbLogger, IDbExecutorService dbExecutor)
        {
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
        }

        public async Task<TransactResult> SaveInsuranceCompany(InsuranceCompanyDto obj)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@InsuranceCompanyName", obj.InsuranceCompanyName },
                    { "@TaxRegistrationNumber", obj.TaxRegistrationNumber },
                    { "@ContactEmail", obj.ContactEmail },
                    { "@ContactPhone", obj.ContactPhone }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_SaveInsuranceCompany", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SaveInsuranceCompany error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> SaveSalesInsurance(SalesInsuranceDto obj)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesId", obj.SalesId },
                    { "@InsuranceCompanyId", obj.InsuranceCompanyId },
                    { "@PolicyNumber", obj.PolicyNumber },
                    { "@Compensation", obj.Compensation },
                    { "@CompensationType", obj.CompensationType },
                    { "@ValidityStartDate", obj.ValidityStartDate },
                    { "@ValidityEndDate", obj.ValidityEndDate }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_SaveSalesInsurance", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SaveSalesInsurance error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetInsuranceBySalesId(int salesId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesId", salesId }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetInsuranceBySalesId", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetInsuranceBySalesId error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetAllInsuranceCompanies()
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>(); // No parameters needed
                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetAllInsuranceCompanies", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetAllInsuranceCompanies error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }
    }
}
