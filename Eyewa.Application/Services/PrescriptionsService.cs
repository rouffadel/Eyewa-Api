using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using Eyewa.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Eyewa.Application.Services
{
    public class PrescriptionsService : IPrescriptionsService
    {
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;

        public PrescriptionsService(
            IDbLoggerService dbLogger,
            IDbExecutorService dbExecutor)
        {
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
        }

        public async Task<TransactResult1> GetOrderLense(int salesId)
        {
            TransactResult1 tres = new TransactResult1();
            try
            {
                _dbLogger.LogInfo("GetOrderLense Method was started");
                
                // Get Order Lenses Grid
                var whereCondition1 = " and SalesID =" + salesId;
                var parameters1 = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition1 },
                    { "@Transaction", "GetOrderLenseGrid" }
                };
                var list1 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters1);

                // Get Prescription Details
                var whereCondition2 = string.Empty;
                if (salesId != 0)
                    whereCondition2 = " where SaleID =" + salesId;
                var parameters2 = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition2 },
                    { "@Transaction", "GetPrescriptionDetails" }
                };
                var list2 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters2);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult1 = list1;
                tres.objresult2 = list2;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetOrderLense Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetPrescriptionDropDowns()
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetPrescriptionDropDowns Method was started");

                var listSph = new List<Dictionary<string, object>>();
                float num1 = 16f;
                for (int index = 0; index < 129; ++index)
                {
                    var row = new Dictionary<string, object>();
                    string valStr = num1 > 0.0 ? "+" + string.Format("{0:f2}", num1) : string.Format("{0:f2}", num1);
                    row["txt"] = valStr;
                    row["val"] = valStr;
                    num1 -= 0.25f;
                    listSph.Add(row);
                }

                var listCyl = new List<Dictionary<string, object>>();
                float num2 = 6f;
                for (int index = 0; index < 49; ++index)
                {
                    var row = new Dictionary<string, object>();
                    string valStr = num2 > 0.0 ? "+" + string.Format("{0:f2}", num2) : string.Format("{0:f2}", num2);
                    row["txt"] = valStr;
                    row["val"] = valStr;
                    num2 -= 0.25f;
                    listCyl.Add(row);
                }

                var listAxis = new List<Dictionary<string, object>>();
                double num3 = 180.0;
                for (int index = 0; index <= 180; ++index)
                {
                    var row = new Dictionary<string, object>();
                    row["txt"] = num3;
                    row["val"] = num3;
                    --num3;
                    listAxis.Add(row);
                }

                var listAdd = new List<Dictionary<string, object>>();
                double num4 = 3.5;
                while (num4 >= 0.0)
                {
                    var row = new Dictionary<string, object>();
                    string valStr = string.Format("{0:f2}", num4);
                    row["txt"] = valStr;
                    row["val"] = valStr;
                    num4 -= 0.25;
                    listAdd.Add(row);
                }

                var dropdownsDict = new Dictionary<string, object>
                {
                    { "SPH", listSph },
                    { "CYL", listCyl },
                    { "AXIS", listAxis },
                    { "ADD", listAdd }
                };

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = dropdownsDict;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetPrescriptionDropDowns Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> SaveOrderLense(OrderLenseItemsCls order)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("SaveOrderLense Method was started");
                string gridStr = string.Empty;
                if (order.OrderLenses != null && order.OrderLenses.Count > 0)
                {
                    for (int i = 0; i < order.OrderLenses.Count; i++)
                    {
                        gridStr += order.OrderLenses[i].CategoryId + "~" + order.OrderLenses[i].Brand + "~" + order.OrderLenses[i].Price + "~" + order.OrderLenses[i].Quantity + "~" + order.OrderLenses[i].Total + "$";
                    }
                    if (gridStr != "")
                        gridStr = gridStr.Substring(0, gridStr.Length - 1);
                }

                var parameters1 = new Dictionary<string, object?>
                {
                    { "@SalesID", order.SalesId },
                    { "@LoginID", 0 },
                    { "@CustomerName", "" },
                    { "@CustomerNo", "" },
                    { "@StoreID", "" },
                    { "@GridData", gridStr },
                    { "@SalesDetailsID", 0 },
                    { "@GrossTotal", 0 },
                    { "@Discount", 0 },
                    { "@NetTotal", 0 },
                    { "@UserID", 0 },
                    { "@InvoiceNo", "" },
                    { "@InvoiceDate", "" },
                    { "@Balance", 0 },
                    { "@Remarks", "" },
                    { "@PaidAmount", 0 },
                    { "@PaymentMode1", "" },
                    { "@Transaction", "SaveOrderLense" }
                };
                
                var list1 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales", parameters1);
                
                if (list1 != null && list1.Count > 0)
                {
                    string status = Convert.ToString(list1[0]["Status"]);
                    string prescStr = string.Empty;

                    if (order.PrescriptionDetails != null)
                    {
                        for (int index = 0; index < order.PrescriptionDetails.Count; ++index)
                        {
                            string s = string.IsNullOrEmpty(order.PrescriptionDetails[index].sph) ? "~" : order.PrescriptionDetails[index].sph + "~";
                            string c = string.IsNullOrEmpty(order.PrescriptionDetails[index].cyl) ? s + "~" : s + order.PrescriptionDetails[index].cyl + "~";
                            string a = string.IsNullOrEmpty(order.PrescriptionDetails[index].axis) ? c + "~" : c + order.PrescriptionDetails[index].axis + "~";
                            prescStr = string.IsNullOrEmpty(order.PrescriptionDetails[index].add) ? a + "~" : a + order.PrescriptionDetails[index].add + "~";
                        }
                    }

                    if (order.PrescriptionIpd != null)
                    {
                        string s = string.IsNullOrEmpty(order.PrescriptionIpd.sphtext) ? prescStr + "~" : prescStr + order.PrescriptionIpd.sphtext + "~";
                        string c = string.IsNullOrEmpty(order.PrescriptionIpd.cyltext) ? s + "~" : s + order.PrescriptionIpd.cyltext + "~";
                        string a = string.IsNullOrEmpty(order.PrescriptionIpd.axistext) ? c + "~" : c + order.PrescriptionIpd.axistext + "~";
                        prescStr = string.IsNullOrEmpty(order.PrescriptionIpd.addtext) ? a + "~" : a + order.PrescriptionIpd.addtext + "~";
                    }

                    if (prescStr != "")
                        prescStr = prescStr.Substring(0, prescStr.Length - 1);

                    var parameters2 = new Dictionary<string, object?>
                    {
                        { "@SalesID", order.SalesId },
                        { "@LoginID", 0 },
                        { "@CustomerName", "" },
                        { "@CustomerNo", "" },
                        { "@StoreID", "" },
                        { "@GridData", prescStr },
                        { "@SalesDetailsID", 0 },
                        { "@GrossTotal", 0 },
                        { "@Discount", 0 },
                        { "@NetTotal", 0 },
                        { "@UserID", 0 },
                        { "@InvoiceNo", "" },
                        { "@InvoiceDate", "" },
                        { "@Remarks", "" },
                        { "@Balance", 0 },
                        { "@PaidAmount", 0 },
                        { "@PaymentMode1", "" },
                        { "@Transaction", "SavePrescription" }
                    };
                    
                    await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales", parameters2);
                }

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list1;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SaveOrderLense Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }
    }
}





