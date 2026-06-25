using Eyewa_new_api.Models;
using Eyewa_new_api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;

        public ProductsService(
            IDbLoggerService dbLogger,
            IDbExecutorService dbExecutor)
        {
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
        }

        public async Task<TransactResult> FillCategory()
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("FillCategory Method was started");

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", "" },
                    { "@Transaction", "DDLCategory" }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataProducts", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("FillCategory Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> FillBrand()
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("FillBrand Method was started");

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", "" },
                    { "@Transaction", "ddlBrand" }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataProducts", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("FillBrand Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetBrand(string brandName)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetBrand Method was started");
                var whereCondition = string.Empty;

                if (!string.IsNullOrEmpty(brandName))
                    whereCondition = " and BrandName like '" + brandName + "%'";

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "GetBrandID" }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetBrand Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetProduct(int categoryId, int brandId, int storeId, string productName)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetProduct Method was started");
                var whereCondition = string.Empty;
                var transaction = string.Empty;

                if (categoryId == 0 || brandId == 0)
                {
                    transaction = "GetProductCategoryBrandIDValue";
                    if (!string.IsNullOrEmpty(productName))
                        whereCondition = " and P.ProductName like '" + productName + "%'";
                    if (storeId != 0)
                        whereCondition += " and ST.StoreID =" + storeId;
                }
                else
                {
                    transaction = "GetProductIDValue1";
                    if (!string.IsNullOrEmpty(productName))
                        whereCondition = " and P.ProductName like '" + productName + "%'";
                    if (categoryId != 0)
                        whereCondition += " and P.CategoryID=" + categoryId;
                    if (brandId != 0)
                        whereCondition += " and P.BrandID= " + brandId;
                    if (storeId != 0)
                        whereCondition += " and ST.StoreID =" + storeId;
                }

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", transaction }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetProduct Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetQuantity(int productId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetQuantity Method was started");
                var whereCondition = " and ProductId =" + productId;

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "GetQuantity1" }
                };
                
                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);
                
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetQuantity Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }
    }
}
