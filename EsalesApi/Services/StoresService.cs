using Eyewa_new_api.Models;
using Eyewa_new_api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public class StoresService : IStoresService
    {
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;

        public StoresService(
            IDbLoggerService dbLogger,
            IDbExecutorService dbExecutor)
        {
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
        }

        public async Task<TransactResult> FillStore(int loginId, int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo($"FillStore Method was started, LoginId={loginId} StoreId={storeId}");
                var whereCondition = string.Empty;
                var str = string.Empty;
                var transaction = string.Empty;

                if (loginId == 1)
                    transaction = "ddlStore";
                else if (loginId != 1 && storeId != 0)
                {
                    transaction = "ddlStoreForUser";
                    whereCondition = " and L.LoginID =" + loginId;
                }
                else
                {
                    transaction = "ddlStoreForOrgUser";
                    str = " and L.LoginID =" + loginId;
                }

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@whereCondition2", str },
                    { "@Transaction", transaction }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataStoreDeliveryNoteNew", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("FillStore Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }
    }
}
