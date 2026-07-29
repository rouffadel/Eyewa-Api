using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using Eyewa.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using QRCoder;
using Zatca.EInvoice.SDK;
using Zatca.EInvoice.SDK.Contracts.Models;
using System.Numerics;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace Eyewa.Application.Services
{
    public class SalesService : ISalesService
    {
        private readonly IDbLoggerService _dbLogger;
        private readonly IDbExecutorService _dbExecutor;
        private readonly IZatcaQrService _zatcaQrService;
        private static readonly HttpClient client = new HttpClient();

        public SalesService(
            IDbLoggerService dbLogger,
            IDbExecutorService dbExecutor,
            IZatcaQrService zatcaQrService)
        {
            _dbLogger = dbLogger;
            _dbExecutor = dbExecutor;
            _zatcaQrService = zatcaQrService;
        }

        private static string converttodate(string date)
        {
            if (string.IsNullOrEmpty(date)) return "";
            string[] strArray = date.Split('-');
            if (strArray.Length < 3) return date;
            if (strArray[0].Length == 1)
                strArray[0] = "0" + strArray[0];
            return strArray[1] + "-" + strArray[0] + "-" + strArray[2];
        }

        private static string converttodateForDb(string date)
        {
            if (string.IsNullOrEmpty(date)) return "";
            string[] strArray = date.Split('-');
            if (strArray.Length < 3) return date;
            if (strArray[0].Length == 1)
                strArray[0] = "0" + strArray[0];
            return strArray[2] + "-" + strArray[1] + "-" + strArray[0];
        }

        public async Task<TransactResult> InsertSales(SalesCls sales)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("InsertSales Method was started, sales=" + JsonConvert.SerializeObject(sales));

                sales.StoreId = sales.StoreId == 0 ? 0 : sales.StoreId;
                sales.CustomerName = sales.CustomerName ?? "";
                sales.CustomerNo = sales.CustomerNo ?? "";
                sales.InvoiceNo = sales.InvoiceNo ?? "";
                sales.InvoiceDate = string.IsNullOrEmpty(sales.InvoiceDate) ? "" : converttodate(sales.InvoiceDate);

                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesID", 0 },
                    { "@LoginID", sales.LoginId },
                    { "@CustomerName", sales.CustomerName },
                    { "@CustomerNo", sales.CustomerNo },
                    { "@StoreID", sales.StoreId },
                    { "@GridData", "" },
                    { "@SalesDetailsID", 0 },
                    { "@GrossTotal", 0 },
                    { "@TotalDiscount", 0 },
                    { "@NetTotal", 0 },
                    { "@UserID", 0 },
                    { "@InvoiceNo", sales.InvoiceNo },
                    { "@InvoiceDate", sales.InvoiceDate },
                    { "@Remarks", "" },
                    { "@Balance", 0 },
                    { "@PaidAmount", 0 },
                    { "@PaymentMode1", "" },
                    { "@Transaction", "InsertSales" },
                    { "@TotalTax", 0 }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales_NewwithTax", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                foreach (var row in result)
                {
                    row["CustomerName"] = sales.CustomerName;
                    row["CustomerNo"] = sales.CustomerNo;
                }
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("InsertSales Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        private async Task LoadSalesPrintDataAsync(int salesId, GetSalesCls sobj)
        {
            var parameters1 = new Dictionary<string, object?>
            {
                { "@WhereCondition", " Where S.SaleID =" + salesId },
                { "@Transaction", "GetSalesDetailGrid" }
            };
            var list1 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters1);
            if (list1 != null)
                sobj.SalesDetails = list1;

            var parameters2 = new Dictionary<string, object?>
            {
                { "@WhereCondition", " and S.SaleID =" + salesId },
                { "@Transaction", "GetPrintPopup" }
            };
            var list2 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters2);
            if (list2 != null)
                sobj.SalesPrint = list2;
        }

        public async Task<TransactResult> SaveSalesDetails(SaveSalesDetails save)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("SaveSalesDetails Method was started");
                var sobj = new GetSalesCls();

                string gridStr = "";
                for (int i = 0; i < save.SalesGrids.Count; i++)
                {
                    gridStr += save.SalesGrids[i].CategoryId + "~" +
                               save.SalesGrids[i].BrandId + "~" +
                               save.SalesGrids[i].ProductId + "~" +
                               save.SalesGrids[i].ProductValue + "~" +
                               save.SalesGrids[i].Quantity + "~" +
                               save.SalesGrids[i].Discount + "~" +
                               save.SalesGrids[i].SellingPrice + "~" +
                               save.SalesGrids[i].Tax + "~" +
                               save.SalesGrids[i].TaxPer + "$";
                }

                string gridData = string.IsNullOrEmpty(gridStr) ? "" : gridStr.TrimEnd('$');

                float grossTotal = string.IsNullOrEmpty(save.GrossTotal) ? 0 : Convert.ToSingle(save.GrossTotal);
                float discount = string.IsNullOrEmpty(save.Discount) ? 0 : Convert.ToSingle(save.Discount);
                float netTotal = string.IsNullOrEmpty(save.NetTotal) ? 0 : Convert.ToSingle(save.NetTotal);
                float tax = string.IsNullOrEmpty(save.Tax) ? 0 : Convert.ToSingle(save.Tax);
                float balance = string.IsNullOrEmpty(save.Balance) ? netTotal : Convert.ToSingle(save.Balance);
                float paidAmount = string.IsNullOrEmpty(save.PaidAmount) ? 0 : Convert.ToSingle(save.PaidAmount);

                string paymentGridStr = "";
                if (save.Payments != null && save.Payments.Count > 0)
                {
                    for (int i = 0; i < save.Payments.Count; i++)
                    {
                        string pm = save.Payments[i].PaymentMode ?? "";
                        string pa = save.Payments[i].PaidAmount ?? "0";
                        string apa = save.Payments[i].AdvancePaidAmount ?? "0";
                        string ia = save.Payments[i].InsuranceAmount ?? "0";
                        paymentGridStr += pm + "~" + pa + "~" + apa + "~" + ia + "$";
                    }
                }
                string paymentGridData = string.IsNullOrEmpty(paymentGridStr) ? "" : paymentGridStr.TrimEnd('$');

                string customerName = save.CustomerName ?? "";
                string customerNo = save.CustomerNo ?? "";
                int salesManID = save.SalesManId == "0" ? 0 : Convert.ToInt32(save.SalesManId);

                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesID", save.SalesId },
                    { "@LoginID", save.LoginId },
                    { "@CustomerName", customerName },
                    { "@CustomerNo", customerNo },
                    { "@StoreID", save.StoreId },
                    { "@GridData", gridData },
                    { "@GrossTotal", grossTotal },
                    { "@SalesDetailsID", 0 },
                    { "@InvoiceNo", "" },
                    { "@InvoiceDate", "" },
                    { "@Remarks", "" },
                    { "@TotalDiscount", discount },
                    { "@NetTotal", netTotal },
                    { "@UserID", salesManID },
                    { "@Balance", balance },
                    { "@PaidAmount", paidAmount },
                    { "@PaymentMode1", save.PaymentMode ?? "" },
                    { "@Transaction", "InsertSalesDetails" },
                    { "@TotalTax", tax },
                    { "@InsuranceAmount", string.IsNullOrEmpty(save.InsuranceAmount) ? 0 : Convert.ToSingle(save.InsuranceAmount) },
                    { "@PaymentGridData", paymentGridData },
                    { "@DeliveryDate", save.DeliveryDate.HasValue ? (object)save.DeliveryDate.Value : DBNull.Value },
                    { "@EarnedLoyaltyPoints", paidAmount * 1 }, // Multiplier assumed as 1 for now
                    { "@RedeemedLoyaltyPoints", save.RedeemedLoyaltyPoints }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales_NewwithTax", parameters);

                string status = string.Empty;
                int salesId = save.SalesId;
                if (list != null && list.Count > 0)
                {
                    var row0 = list[0];
                    if (row0.ContainsKey("Status"))
                        status = Convert.ToString(row0["Status"]);

                    if (row0.ContainsKey("SaleID") && row0["SaleID"] != null)
                        salesId = Convert.ToInt32(row0["SaleID"]);
                    else if (row0.ContainsKey("SalesID") && row0["SalesID"] != null)
                        salesId = Convert.ToInt32(row0["SalesID"]);
                    else if (row0.ContainsKey("ID") && row0["ID"] != null)
                        salesId = Convert.ToInt32(row0["ID"]);
                }

                if (status == "Success" || status == "Payment is successfull.")
                {
                    try
                    {
                        await GenerateSimplifiedInvoiceTesting(save, salesId);
                    }
                    catch (Exception zatcaEx)
                    {
                        _dbLogger.LogError("ZATCA process error: " + zatcaEx.Message, zatcaEx.StackTrace);
                        throw new Exception("ZATCA ERROR: " + zatcaEx.Message);
                    }

                    await LoadSalesPrintDataAsync(salesId, sobj);
                }

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = sobj;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SaveSalesDetails Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }

            return tres;
        }

        public async Task<TransactResult> GetCustomerLoyaltyPoints(string customerNo)
        {
            TransactResult tres = new TransactResult();
            try
            {
                if (string.IsNullOrEmpty(customerNo))
                {
                    tres.Status = "-100";
                    tres.Message = "Customer number is required";
                    return tres;
                }

                string query = @"
                    SELECT 
                        ISNULL(SUM(EarnedLoyaltyPoints), 0) - ISNULL(SUM(RedeemedLoyaltyPoints), 0) AS AvailablePoints 
                    FROM SaleMaster 
                    WHERE CustomerNo = @CustomerNo";

                var parameters = new Dictionary<string, object?>
                {
                    { "@CustomerNo", customerNo }
                };

                var list = await _dbExecutor.ExecuteQueryAsync(query, parameters);
                decimal points = 0;

                if (list != null && list.Count > 0)
                {
                    if (list[0].ContainsKey("AvailablePoints") && list[0]["AvailablePoints"] != null)
                    {
                        points = Convert.ToDecimal(list[0]["AvailablePoints"]);
                    }
                }

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = new { Points = points };
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetCustomerLoyaltyPoints Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetTodayDeliveries(int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@StoreId", storeId }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("GetTodayDeliveries", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetTodayDeliveries Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetSalesGrid(SearchSalesCls obj)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetSalesGrid Method was started");
                var whereCondition = string.Empty;
                var str = string.Empty;
                var transaction = string.Empty;

                int storeId = obj.StoreID == 0 ? 0 : obj.StoreID;
                string customerName = obj.CustomerName ?? "";
                string customerNo = obj.CustomerNo ?? "";
                string fromDate = obj.FromDate ?? "";
                string toDate = obj.ToDate ?? "";
                string invoiceNo = obj.InvoiceNo ?? "";
                string serialNo = obj.SerialNo ?? "";

                if (storeId != 0)
                    whereCondition = " and S.StoreID =" + storeId;
                if (!string.IsNullOrEmpty(customerName))
                    whereCondition += " and S.CustomerName like '" + customerName + "%'";
                if (!string.IsNullOrEmpty(customerNo))
                    whereCondition += " and S.CustomerNo like '" + customerNo + "%'";
                if (!string.IsNullOrEmpty(invoiceNo))
                    whereCondition += " and S.InvoiceNo like '%" + invoiceNo + "%'";
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                    whereCondition += " and cast(S.CreatedDate as date) between '" + converttodateForDb(fromDate) + "' and '" + converttodateForDb(toDate) + "'";
                if (!string.IsNullOrEmpty(serialNo))
                    whereCondition += " and REVERSE(SUBSTRING(REVERSE(S.InvoiceNo), 1,CHARINDEX('-', REVERSE(S.InvoiceNo)) - 1))  ='" + serialNo + "'";

                if (obj.LoginID == 1)
                    transaction = "GetSalesGrid";
                else if (storeId == 0)
                {
                    transaction = "GetSalesGridForOrgUser";
                    str += " and L.LoginID =" + obj.LoginID;
                }
                else
                {
                    transaction = "GetSalesGridForUser";
                    whereCondition += " and L.LoginID =" + obj.LoginID;
                }

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition2", str },
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", transaction }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("GetDataSalesNew", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesGrid Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetInvoiceDetails(int salesId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetInvoiceDetails Method was started");
                var whereCondition = string.Empty;

                if (salesId != 0)
                    whereCondition = " and SaleID =" + salesId;

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "InvoiceDetails" }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetInvoiceDetails Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetSalesPrint(int salesId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetSalesPrint Method was started");
                var whereCondition = string.Empty;

                if (salesId != 0)
                    whereCondition = " and S.SaleID = " + salesId;

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "GetPrintPopup" }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesPrint Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetZatcaQrBySalesId(int salesId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetSalesDetailsGrid Method was started");
                var whereCondition = string.Empty;

                if (salesId != 0)
                    whereCondition = " Where S.SaleID =" + salesId;

                var parameters1 = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "GetSalesDetailGrid1" }
                };

                //   var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters1);
                var list = await _dbExecutor.ExecuteStoredProcedureMultiResultAsync("SP_GetDataSales", parameters1);

                if (list == null || list.Count == 0)
                    throw new Exception("Invoice data not found");

                var parameters2 = new Dictionary<string, object?>
                {
                    { "@SalesId", salesId }
                };

                var list1 = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetZatcaInvoiceBySalesId", parameters2);

                //if (list1 == null || list1.Count == 0)
                //    throw new Exception("ZATCA QR not found");


                if (list1 != null && list1.Count > 0)
                {
                    string qrBase64 = list1[0]["QRCode"]?.ToString() ?? "";

                    if (!string.IsNullOrWhiteSpace(qrBase64))
                    {
                        var qrGenerator = new QRCodeGenerator();
                        var qrCodeData = qrGenerator.CreateQrCode(qrBase64, QRCodeGenerator.ECCLevel.Q);
                        var qrCode = new PngByteQRCode(qrCodeData);

                        byte[] qrBytes = qrCode.GetGraphic(20);

                        string qrImageBase64 = Convert.ToBase64String(qrBytes);

                        tres.qrcodeimg = "data:image/png;base64," + qrImageBase64;
                    }
                }

                //string folder = @"C:\Temp";
                //Directory.CreateDirectory(folder);

                //string filePath = Path.Combine(folder, "zatca_qr.png");
                //File.WriteAllBytes(filePath, qrBytes);

                //string qrImageBase64 = Convert.ToBase64String(qrBytes);

                //tres.qrcodeimg = "data:image/png;base64," + qrImageBase64;
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesDetailsGrid Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetSalesDetailsGrid(int salesId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("GetSalesDetailsGrid Method was started");
                var whereCondition = string.Empty;

                if (salesId != 0)
                    whereCondition = " Where S.SaleID =" + salesId;

                var parameters = new Dictionary<string, object?>
                {
                    { "@WhereCondition", whereCondition },
                    { "@Transaction", "GetSalesDetailGrid1" }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetDataSales", parameters);

                if (list == null || list.Count == 0)
                    throw new Exception("Invoice data not found");

                var row = list[0];

                //string sellerName = "Naimat Al Basar";
                //string vatNo = "310254659700003";
                //string invoiceDate = Convert.ToDateTime(row["InvoiceDate"]).ToString("yyyy-MM-ddTHH:mm:ss");
                //string total = row["NetTotal"]?.ToString() ?? "0";
                //string vatAmount = row["TotalTax"]?.ToString() ?? "0";

                //string base64Qr = GenerateZatcaQr(sellerName, vatNo, invoiceDate, total, vatAmount);

                //var qrGenerator = new QRCodeGenerator();
                //var qrCodeData = qrGenerator.CreateQrCode(base64Qr, QRCodeGenerator.ECCLevel.Q);
                //var qrCode = new PngByteQRCode(qrCodeData);
                //byte[] qrBytes = qrCode.GetGraphic(20);
                //string qrImageBase64 = Convert.ToBase64String(qrBytes);

                //tres.qrcodeimg = "data:image/png;base64," + qrImageBase64;
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesDetailsGrid Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> DeleteSales(int salesId, int loginId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("DeleteSales Method was started");

                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesID", salesId },
                    { "@LoginID", loginId },
                    { "@CustomerName", "" },
                    { "@CustomerNo", "" },
                    { "@StoreID", 0 },
                    { "@GridData", "" },
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
                    { "@Transaction", "DeleteSales" }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("DeleteSales Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> DeleteSalesDetails(int salesId, int loginId, int salesDetailID)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("DeleteSalesDetails Method was started");

                var parameters = new Dictionary<string, object?>
                {
                    { "@SalesID", 0 },
                    { "@LoginID", loginId },
                    { "@CustomerName", "" },
                    { "@CustomerNo", "" },
                    { "@StoreID", 0 },
                    { "@GridData", "" },
                    { "@SalesDetailsID", salesDetailID },
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
                    { "@Transaction", "DeleteSalesDetails" }
                };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync("SP_Sales", parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("DeleteSalesDetails Method error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        // ============================================
        // ZATCA / XML / CRYPTOGRAPHY UTILITY METHODS
        // ============================================

        private static string GenerateZatcaQr(string sellerName, string vatNo, string invoiceDate, string total, string vatAmount)
        {
            List<byte> qrBytes = new List<byte>();

            void AddField(int tag, string value)
            {
                byte[] valueBytes = Encoding.UTF8.GetBytes(value ?? "");
                qrBytes.Add((byte)tag);
                qrBytes.Add((byte)valueBytes.Length);
                qrBytes.AddRange(valueBytes);
            }

            AddField(1, sellerName);
            AddField(2, vatNo);
            AddField(3, invoiceDate);
            AddField(4, total.Replace(",", "."));
            AddField(5, vatAmount.Replace(",", "."));

            return Convert.ToBase64String(qrBytes.ToArray());
        }

        private static async Task<string> SendInvoice1(string uuid, string hash, string xml)
        {
            try
            {
                string binarySecurityToken = "TUlJRklEQ0NCTWVnQXdJQkFnSVRYQUFDSnVWUUV1OVBSRUYvYWdBQkFBSW01VEFLQmdncWhrak9QUVFEQWjCaU1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJzd0dRWURWUVFERXhKUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFd0hoY05Nall3TkRBeU1UVXlPRE13V2hjTk16QXdNDFBMTRFeTNXakJXTVFzd0NRWURWUVFHRXdKVFFURWdNQjRHQTFVRUNoTVhUbUZwYldGMElFRnNMVUpoYzJGeUlFOXdkR2xqWVd3eEN6QUpCZ05WQkFzVEFrbFVNUmd3RmdZRFZRUURFdzh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SURhVENDQTJVd2dkTUdBMVVkRVFTQnl6Q0J5S1NCeFRDQndqRm9NR1lHQTFVRUJBeGZNUzB5WWpCaFlUWmtNUzB6TVRNMUxUUTFPV0itT0dVd1lTMHhZV0ZtTjJRd1l6UTBPVE44TWkwek1UQXlOVFEyTlRrM01EQXdNRE44TXkwM09EbG1ZMk0yTUMweE1UYzVMVFF3TTJFdE9ESmhZaTAxT1dJek16Z3lZelF6TkdNeEh6QWRCZ29Ka2lhSmsvSXNaQUVCREE4ek1UQXlOVFEyTlRrM01EQXdNRE14RFRBTEJnTlZCQXdNQkRFeE1EQXhEakFNQmdOVkJCb01CVWh2Wm5WbU1SWXdGQVlEVlFRUERBMVBjSFJwWTJGc0lGTjBiM0psTUIwR0ExVWREZ1FXQkJTTVUxVEpxTWYzT1AxclhNRjVvdDhPZmpaM3VEQWZCZ05WSFNNRUdEQVdnQlJaeUhLZWVUVnA2cnpabGRUWDRHUnBBZ1lRR1RDQjVRWURWUjBmQklIZE1JSGFNSUhYb0lIVW9JSFJob0hPYkdSaGNEb3ZMeTlEVGoxUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFb01Ta3NRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMRU5PUFVORVVDeERUajFRZFdKc2FXTWxNakJMWlhrbE1qQlRaWEoyYVdObGN5eERUajFUWlhKMmFXTmxjeXhEVGoxRGIyNW1hV2QxY21GMGFXOXVMRVJEUFdWNGRIcGhkR05oTEVSRFBXZHZkaXhFUXoxc2IyTmhiRDlqWlhKMGFXWnBZMkYwWlZKbGRtOWpZWFJwYjI1TWFYTjBQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMlZ5ZEdsbWFXTmhkR2x2YmtGMWRHaHZjbWwwZVRBT0JnTlZIUThCQWY4RUJBTUNCNEF3UEFZSkt3WUJCQUdDTnhVSEJDOHdMUVlsS3dZQkJBR0NOeFVJZ1lhb0hZVFEreEtHN1owa2g4NzdHZFBBVldhSCtxVmxoZG1FUGdJQlpBSUJEakFkQmdOVkhTVUVGakFVQmdnckJnRUZCUWNEQXdZSUt3WUJCUVVIQXdJd0p3WUpLd1lCQkFHQ054VUtCQm93R0RBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dyQmdFRkJRY0RBakFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUFDaUptSGh6K2dzb0hwV0NZQXZtWDNOZm5OdzI1ZHljSkwzV2tpblVsM3JnSWdWam5wcE5Fbm9kSnl3QURrRk01N1FuVEZWcTVsaWtmS29keDZ5aCtQbUI0PQ==";
                string secret = "h13pi0OHIpDFzGcDw6AYnTuMKdrkSCCU64UbnXg/cI8=";

                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(binarySecurityToken + ":" + secret));

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                client.DefaultRequestHeaders.Add("Accept-Language", "en");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Clearance-Status", "0");

                string base64Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

                var requestBody = new
                {
                    invoiceHash = hash,
                    uuid = uuid,
                    invoice = base64Xml
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/invoices/reporting/single",
                    content
                );

                string result = await response.Content.ReadAsStringAsync();

                if (response.Headers.Contains("requestID"))
                {
                    var requestId = response.Headers.GetValues("requestID").FirstOrDefault();
                    Console.WriteLine("Request ID: " + requestId);
                }

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("STATUS: " + response.StatusCode);
                    Console.WriteLine("FULL RESPONSE: " + result);
                    throw new Exception("ZATCA ERROR:\n" + result);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private static string ConvertPkcs8ToEcPrivateKey(string pkcs8Base64)
        {
            byte[] pkcs8Bytes = Convert.FromBase64String(pkcs8Base64);
            var privateKeyInfo = Org.BouncyCastle.Asn1.Pkcs.PrivateKeyInfo.GetInstance(pkcs8Bytes);
            AsymmetricKeyParameter key = PrivateKeyFactory.CreateKey(privateKeyInfo);

            using (StringWriter sw = new StringWriter())
            {
                PemWriter writer = new PemWriter(sw);
                writer.WriteObject(key);
                writer.Writer.Flush();
                return sw.ToString();
            }
        }

        private void UpdateInvoiceId(XmlDocument doc, int salesId)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            XmlNode node = doc.SelectSingleNode(
                "/*[local-name()='Invoice']/*[local-name()='ID']",
                nsmgr
            );

            if (node != null)
            {
                node.InnerText = "INV-" + salesId;
            }
        }

        private string GenerateInvoiceXmlSection(SaveSalesDetails save)
        {
            var sb = new StringBuilder();

            decimal totalLineAmount = 0;
            decimal totalTax = 0;
            decimal discount = string.IsNullOrEmpty(save.Discount) ? 0 : Convert.ToDecimal(save.Discount);

            foreach (var item in save.SalesGrids)
            {
                decimal qty = string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDecimal(item.Quantity);
                decimal price = string.IsNullOrEmpty(item.SellingPrice) ? 0 : Convert.ToDecimal(item.SellingPrice);
                decimal taxPer = string.IsNullOrEmpty(item.TaxPer) ? 15 : Convert.ToDecimal(item.TaxPer);

                decimal lineAmount = qty * price;
                decimal taxAmount = (lineAmount * taxPer) / 100;

                totalLineAmount += lineAmount;
                totalTax += taxAmount;
            }

            decimal taxExclusive = totalLineAmount;
            decimal taxInclusive = totalLineAmount + totalTax;
            decimal payableAmount = taxInclusive - discount;

            string paymentCode = "10";
            if (save.PaymentMode == "Card")
                paymentCode = "48";
            else if (save.PaymentMode == "Bank")
                paymentCode = "30";

            sb.Append($@"
<cac:PaymentMeans>
    <cbc:PaymentMeansCode>{paymentCode}</cbc:PaymentMeansCode>
</cac:PaymentMeans>
");

            sb.Append($@"
<cac:AllowanceCharge>
    <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
    <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
    <cbc:Amount currencyID=""SAR"">{discount:F2}</cbc:Amount>

    <cac:TaxCategory>
        <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
        <cbc:Percent>15</cbc:Percent>
        <cac:TaxScheme>
            <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
        </cac:TaxScheme>
    </cac:TaxCategory>

    <!-- REQUIRED duplicate for ZATCA -->
    <cac:TaxCategory>
        <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
        <cbc:Percent>15</cbc:Percent>
        <cac:TaxScheme>
            <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
        </cac:TaxScheme>
    </cac:TaxCategory>

</cac:AllowanceCharge>
");

            sb.Append($@"
<cac:TaxTotal>
    <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
</cac:TaxTotal>

<cac:TaxTotal>
    <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
    <cac:TaxSubtotal>
        <cbc:TaxableAmount currencyID=""SAR"">{taxExclusive:F2}</cbc:TaxableAmount>
        <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
        <cac:TaxCategory>
            <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
            <cbc:Percent>15.00</cbc:Percent>
            <cac:TaxScheme>
                <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
            </cac:TaxScheme>
        </cac:TaxCategory>
    </cac:TaxSubtotal>
</cac:TaxTotal>
");

            sb.Append($@"
<cac:LegalMonetaryTotal>
    <cbc:LineExtensionAmount currencyID=""SAR"">{totalLineAmount:F2}</cbc:LineExtensionAmount>
    <cbc:TaxExclusiveAmount currencyID=""SAR"">{taxExclusive:F2}</cbc:TaxExclusiveAmount>
    <cbc:TaxInclusiveAmount currencyID=""SAR"">{taxInclusive:F2}</cbc:TaxInclusiveAmount>
    <cbc:AllowanceTotalAmount currencyID=""SAR"">{discount:F2}</cbc:AllowanceTotalAmount>
    <cbc:PrepaidAmount currencyID=""SAR"">0.00</cbc:PrepaidAmount>
    <cbc:PayableAmount currencyID=""SAR"">{payableAmount:F2}</cbc:PayableAmount>
</cac:LegalMonetaryTotal>
");

            int lineId = 1;
            foreach (var item in save.SalesGrids)
            {
                decimal qty = string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDecimal(item.Quantity);
                decimal price = string.IsNullOrEmpty(item.SellingPrice) ? 0 : Convert.ToDecimal(item.SellingPrice);
                decimal taxPer = string.IsNullOrEmpty(item.TaxPer) ? 15 : Convert.ToDecimal(item.TaxPer);

                decimal lineAmount = qty * price;
                decimal taxAmount = (lineAmount * taxPer) / 100;
                decimal roundingAmount = lineAmount + taxAmount;

                sb.Append($@"
<cac:InvoiceLine>
    <cbc:ID>{lineId}</cbc:ID>
    <cbc:InvoicedQuantity unitCode=""PCE"">{qty:F6}</cbc:InvoicedQuantity>
    <cbc:LineExtensionAmount currencyID=""SAR"">{lineAmount:F2}</cbc:LineExtensionAmount>

    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""SAR"">{taxAmount:F2}</cbc:TaxAmount>
        <cbc:RoundingAmount currencyID=""SAR"">{roundingAmount:F2}</cbc:RoundingAmount>
    </cac:TaxTotal>

    <cac:Item>
        <cbc:Name>Optical Lens</cbc:Name>
<cac:SellersItemIdentification>
    <cbc:ID>{item.ProductId}</cbc:ID>
</cac:SellersItemIdentification>
        <cac:ClassifiedTaxCategory>
            <cbc:ID>Z</cbc:ID>
            <cbc:Percent>{taxPer:F2}</cbc:Percent>
            <cac:TaxScheme>
                <cbc:ID>VAT</cbc:ID>
            </cac:TaxScheme>
        </cac:ClassifiedTaxCategory>
    </cac:Item>

    <cac:Price>
        <cbc:PriceAmount currencyID=""SAR"">{price:F2}</cbc:PriceAmount>
    </cac:Price>
</cac:InvoiceLine>
");
                lineId++;
            }

            return sb.ToString();
        }

        private void UpdateDynamicFields(XmlDocument doc, string uuid)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            doc.SelectSingleNode("//cbc:UUID", ns).InnerText = uuid;
            doc.SelectSingleNode("//cbc:IssueDate", ns).InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd");
            doc.SelectSingleNode("//cbc:IssueTime", ns).InnerText = DateTime.Now.ToString("HH:mm:ss");
        }

        private async Task GenerateSimplifiedInvoiceworking(SaveSalesDetails save, int salesId)
        {
            string privateKeyBase64 = "MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgZ5dxHpu9v4z24twTDCcrCIG2YRcaU3teZ1h1zV8XMVKgBwYFK4EEAAqhRANCAAR0+XfABg4XOa5eiBVt2NLDEwVNxCnlO1PL+Js87ARbw3zURx7BiEGCIWnNti0VxtolBNpUWBh4mr4ov/8xBfjY";
            string certBase64 = "TUlJRklEQ0NCTWVnQXdJQkFnSVRYQUFDSnVWUUV1OVBSRUYvYWdBQkFBSW01VEFLQmdncWhrak9QUQFEQWjCaU1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJzd0dRWURWUVFERXhKUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFd0hoY05Nall3TkRBeU1UVXlPRE13V2hjTk16QXdOREF5TURNME1URTNXakJXTVFzd0NRWURWUVFHRXdKVFFURWdNQjRHQTFVRUNoTVhUbUZwYldGMElFRnNMVUpoYzJGeUlFOXdkR2xqWVd3eEN6QUpCZ05WQkFzVEFrbFVNUmd3RmdZRFZRUURFdzh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SURhVENDQTJVd2dkTUdBMVVkRVFTQnl6Q0J5S1NCeFRDQndqRm9NR1lHQTFVRUJBeGZNUzB5WWpCaFlUWmtNUzB6TVRNMUxUUTFPV0l0T0dVd1lTMHhZV0ZtTjJRd1l6UTBPVE44TWkwek1UQXlOVFEyTlRrM01EQXdNRE44TXkwM09EbG1ZMk0yTUMweE1UYzVMVFF3TTJFdE9ESmhZaTAxT1dJek16Z3lZelF6TkdNeEh6QWRCZ29Ka2lhSmsvSXNaQUVCREE4ek1UQXlOVFEyTlRrM01EQXdNRE14RFRBTEJnTlZCQXdNQkRFeE1EQXhEakFNQmdOVkJCb01CVWh2Wm5WbU1SWXdGQVlEVlFRUERBMVBjSFJwWTJGc0lGTjBiM0psTUIwR0ExVWREZ1FXQkJTTVUxVEpxTWYzT1AxclhNRjVvdDhPZmpaM3VEQWZCZ05WSFNNRUdEQVdnQlJaeUhLZWVUVnA2cnpabGRUWDRHUnBBZ1lRR1RDQjVRWURWUjBmQklIZE1JSGFNSUhYb0lIVW9JSFJob0hPYkdSaGNEb3ZMeTlEVGoxUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFb01Ta3NRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMRU5PUFVORVVDeERUajFRZFdKc2FXTWxNakJMWlhrbE1qQlRaWEoyYVdObGN5eERUajFUWlhKMmFXTmxjeXhEVGoxRGIyNW1hV2QxY21GMGFXOXVMRVJEUFdWNGRIcGhkR05oTEVSRFBXZHZkaXhEUVoxc2IyTmhiRDlqWlhKMGFXWnBZMkYwWlZKbGRtOWpZWFJwYjI1TWFYTjBQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMUpNUkdsemRISnBZblYwYVc5dVVHOXBiblF3Z2M0R0NDc0dBUVVGQndFQkJJSEJNSUcrTUlHN0JnZ3JCZ0VGQlFjd0FvYUJybXhrWVhBNkx5OHZRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMRU5PUFVORVVDeERUajFRZFdKc2FXTWxNakJMWlhrbE1qQlRaWEoyYVdObGN5eERUajFUWlhKMmFXTmxjeXhEVGoxRGIyNW1hV2QxY21GMGFXOXVMRVJEUFdWNGRIcGhkR05oTEVSRFBXZHZkaXhFUXoxc2IyTmhiRDlqWlhKMGFXWnBZMkYwWlZKbGRtOWpZWFJwYjI1TWFYTjBQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMlZ5ZEdsbWFXTmhkR2x2YmtGMWRHaHZjbWwwZVRBT0JnTlZIUThCQWY4RUJBTUNCNEF3UEFZSkt3WUJCQUdDTnhVSEJDOHdMUVlsS3dZQkJBR0NOeFVJZ1lhb0hZVFEreEtHN1owa2g4NzdHZFBBVldhSCtxVmxoZG1FUGdJQlpBSUJEakFkQmdOVkhTVUVGakFVQmdnckJnRUZCUWNEQXdZSUt3WUJCUVVIQXdJd0p3WUpLd1lCQkFHQ054VUtCQm93R0RBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dyQmdFRkJRY0RBakFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUFDaUptSGh6K2dzb0hwV0NZQXZtWDNOZm5OdzI1ZHljSkwzV2tpblVsM3JnSWdWam5wcE5Fbm9kSnl3QURrRk01N1FuVEZWcTVsaWtmS29keDZ5aCtQbUI0PQ==";

            string xmlPath = @"D:\syed_raufsirproj\invoice.xml";
            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(xmlPath);

            UpdateInvoiceId(doc, salesId);

            string dynamicXml = GenerateInvoiceXmlSection(save);
            var invoiceno = new StringBuilder();
            invoiceno.Append($@"<cbc:ID>{salesId}</cbc:ID>");

            string xmlString = doc.OuterXml;
            xmlString = xmlString.Replace("<!--INVOICE_DYNAMIC_ID-->", invoiceno.ToString());
            xmlString = xmlString.Replace("<!--INVOICE_DYNAMIC_SECTION-->", dynamicXml);

            doc.LoadXml(xmlString);

            string uuid = Guid.NewGuid().ToString();
            UpdateDynamicFields(doc, uuid);

            byte[] certBytes = Convert.FromBase64String(certBase64);
            X509Certificate2 cert = new X509Certificate2(certBytes);

            EInvoiceHashGenerator invoicehshgen = new EInvoiceHashGenerator();
            var hashs = invoicehshgen.GenerateEInvoiceHashing(doc);

            byte[] level1 = Convert.FromBase64String(certBase64);
            string certificateContent = Encoding.UTF8.GetString(level1);

            string ecPrivateKeyPem = ConvertPkcs8ToEcPrivateKey(privateKeyBase64);
            string cleanedprivatekey = ecPrivateKeyPem.Replace("-----BEGIN EC PRIVATE KEY-----", "")
                                                      .Replace("-----END EC PRIVATE KEY-----", "")
                                                      .Replace("\n", "").Replace("\r", "").Trim();

            EInvoiceSigner signer = new EInvoiceSigner();
            SignResult signResult = signer.SignDocument(doc, certificateContent, cleanedprivatekey);

            var res = signResult.SignedEInvoice.OuterXml;
            var response = await SendInvoice1(uuid, hashs.Hash, res);
            var qrCode = GetQR(res);

            await SaveZatcaDataAsync(salesId, uuid, hashs.Hash, doc.OuterXml, qrCode, response);
        }

        private string GetQR(string xmlstring)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xmlstring);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            var qrValue = "";
            var qrNode = doc.SelectSingleNode(
                "//cac:AdditionalDocumentReference[cbc:ID='QR']/cac:Attachment/cbc:EmbeddedDocumentBinaryObject",
                ns
            );

            if (qrNode != null)
            {
                qrValue = qrNode.InnerText;
            }
            return qrValue;
        }

        private async Task GenerateSimplifiedInvoiceTesting(SaveSalesDetails save, int salesId)
        {
            string privateKeyBase64;
            string certBase64;

            using (var ecdsa = System.Security.Cryptography.ECDsa.Create(System.Security.Cryptography.ECCurve.CreateFromFriendlyName("secp256k1")))
            {
                var subject = new System.Security.Cryptography.X509Certificates.X500DistinguishedName("CN=Test Certificate, O=Test Org, C=SA");
                var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(subject, ecdsa, System.Security.Cryptography.HashAlgorithmName.SHA256);
                var keyUsage = new System.Security.Cryptography.X509Certificates.X509KeyUsageExtension(System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.DigitalSignature | System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.NonRepudiation, true);
                request.CertificateExtensions.Add(keyUsage);

                using (var tempCert = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10)))
                {
                    byte[] pkcs8Bytes = ecdsa.ExportPkcs8PrivateKey();
                    privateKeyBase64 = Convert.ToBase64String(pkcs8Bytes);

                    byte[] exportedCertBytes = tempCert.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Cert);
                    certBase64 = Convert.ToBase64String(exportedCertBytes);
                }
            }

            string rawCertBase64 = certBase64;
            certBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCertBase64));

            string xmlPath = @"D:\syed_raufsirproj\invoice.xml";
            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
            if (File.Exists(xmlPath))
            {
                doc.Load(xmlPath);
            }
            else
            {
                string fallbackXml = @"<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
<cbc:ID>INV-000</cbc:ID>
<cbc:UUID>00000000-0000-0000-0000-000000000000</cbc:UUID>
<cbc:IssueDate>2026-07-02</cbc:IssueDate>
<cbc:IssueTime>12:00:00</cbc:IssueTime>
<!--INVOICE_DYNAMIC_ID-->
<!--INVOICE_DYNAMIC_SECTION-->
</Invoice>";
                doc.LoadXml(fallbackXml);
            }

            UpdateInvoiceId(doc, salesId);

            string dynamicXml = GenerateInvoiceXmlSection(save);
            var invoiceno = new StringBuilder();
            invoiceno.Append($@"<cbc:ID>{salesId}</cbc:ID>");

            string xmlString = doc.OuterXml;
            xmlString = xmlString.Replace("<!--INVOICE_DYNAMIC_ID-->", invoiceno.ToString());
            xmlString = xmlString.Replace("<!--INVOICE_DYNAMIC_SECTION-->", dynamicXml);

            doc.LoadXml(xmlString);

            string uuid = Guid.NewGuid().ToString();
            UpdateDynamicFields(doc, uuid);

            byte[] certBytes = Convert.FromBase64String(certBase64);
            X509Certificate2 cert = new X509Certificate2(certBytes);

            EInvoiceHashGenerator invoicehshgen = new EInvoiceHashGenerator();
            var hashs = invoicehshgen.GenerateEInvoiceHashing(doc);

            byte[] level1 = Convert.FromBase64String(certBase64);
            string certificateContent = Encoding.UTF8.GetString(level1);

            string ecPrivateKeyPem = ConvertPkcs8ToEcPrivateKey(privateKeyBase64);
            string cleanedprivatekey = ecPrivateKeyPem.Replace("-----BEGIN EC PRIVATE KEY-----", "")
                                                      .Replace("-----END EC PRIVATE KEY-----", "")
                                                      .Replace("\n", "").Replace("\r", "").Trim();

            EInvoiceSigner signer = new EInvoiceSigner();
            SignResult signResult = signer.SignDocument(doc, certificateContent, cleanedprivatekey);

            var res = signResult.SignedEInvoice.OuterXml;

            decimal totalLineAmount = 0;
            decimal totalTax = 0;
            decimal discount = string.IsNullOrEmpty(save.Discount) ? 0 : Convert.ToDecimal(save.Discount);

            foreach (var item in save.SalesGrids)
            {
                decimal qty = string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDecimal(item.Quantity);
                decimal price = string.IsNullOrEmpty(item.SellingPrice) ? 0 : Convert.ToDecimal(item.SellingPrice);
                decimal taxPer = string.IsNullOrEmpty(item.TaxPer) ? 15 : Convert.ToDecimal(item.TaxPer);

                decimal lineAmount = qty * price;
                decimal taxAmount = (lineAmount * taxPer) / 100;

                totalLineAmount += lineAmount;
                totalTax += taxAmount;
            }

            decimal taxInclusive = totalLineAmount + totalTax;
            decimal payableAmount = taxInclusive - discount;

            string testSellerName = "Test Company LLC";
            string testVatNo = "312345678900003";
            DateTime timestamp = DateTime.UtcNow;
            string qrCodeBase64 = _zatcaQrService.GenerateZatcaQrCode(testSellerName, testVatNo, timestamp, payableAmount, totalTax);

            XmlDocument signedDoc = new XmlDocument { PreserveWhitespace = true };
            signedDoc.LoadXml(res);
            var ns = new XmlNamespaceManager(signedDoc.NameTable);
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            var qrNode = signedDoc.SelectSingleNode(
                "//cac:AdditionalDocumentReference[cbc:ID='QR']/cac:Attachment/cbc:EmbeddedDocumentBinaryObject",
                ns
            );
            if (qrNode != null)
            {
                qrNode.InnerText = qrCodeBase64;
            }
            res = signedDoc.OuterXml;

            string response = @"{ ""status"": ""REPORTED"", ""validationResults"": { ""infoMessages"": [], ""warningMessages"": [], ""errorMessages"": [] } }";

            await SaveZatcaDataAsync(salesId, uuid, hashs.Hash, res, qrCodeBase64, response);
        }

        private async Task SaveZatcaDataAsync(int salesId, string uuid, string hash, string xml, string qr, string response)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "@SalesId", salesId },
                { "@UUID", uuid },
                { "@InvoiceHash", hash },
                { "@InvoiceXml", xml },
                { "@QRCode", qr },
                { "@ZatcaResponse", response ?? "Null Response" },
                { "@Status", (response != null && response.Contains("SUCCESS")) ? "Success" : "Failed" }
            };

            await _dbExecutor.ExecuteStoredProcedureAsync("SP_SaveZatcaInvoice", parameters);
        }

        public async Task<TransactResult> CustomerSearchFilter(string mobileNumber)
        {
            TransactResult tres = new TransactResult();
            try
            {
                _dbLogger.LogInfo("CustomerSearchFilter started, mobileNumber=" + mobileNumber);
                if (string.IsNullOrEmpty(mobileNumber))
                {
                    tres.Status = "-100";
                    tres.Message = "Mobile number is required";
                    return tres;
                }

                var query = @"
                    SELECT 
                        SaleID as ID, 
                        InvoiceNo, 
                        LTRIM(RTRIM(CustomerNo)) as CustomerNo, 
                        NULL as chkavail, 
                        LTRIM(RTRIM(CustomerName)) as CustomerName 
                    FROM Sale 
                    WHERE IsDeleted = 0 
                      AND CustomerNo LIKE @mobileNumber + '%' 
                      AND CustomerName IS NOT NULL 
                      AND LTRIM(RTRIM(CustomerName)) != ''
                    ORDER BY SaleID DESC";

                var parameters = new Dictionary<string, object?>
                {
                    { "@mobileNumber", mobileNumber }
                };

                var list = await _dbExecutor.ExecuteQueryAsync(query, parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("CustomerSearchFilter error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }


        public async Task<TransactResult> OpenRegister(OpenRegisterRequest request)
        {
            TransactResult tres = new();

            try
            {
                var parameters = new Dictionary<string, object?>
        {
            { "@StoreID", request.StoreId },
            { "@LoginID", request.LoginId },
            { "@OpeningAmount", request.OpeningAmount },
            { "@Transaction", "OpenRegister" }
        };

                await _dbExecutor.ExecuteStoredProcedureAsync(
                    "SP_RegisterSession",
                    parameters);

                tres.Status = "200";
                tres.Message = "Register Opened Successfully";
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }

            return tres;
        }



        public async Task<TransactResult> GetClosingSummary(RegisterSummaryRequest request)
        {
            TransactResult tres = new();

            try
            {
                var parameters = new Dictionary<string, object?>
        {
            { "@StoreID", request.StoreId },
            { "@Transaction", "GetClosingSummary" }
        };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync(
                    "SP_RegisterSession",
                    parameters);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }

            return tres;
        }



        public async Task<TransactResult> CloseRegister(CloseRegisterRequest request)
        {
            TransactResult tres = new();

            try
            {
                var parameters = new Dictionary<string, object?>
        {
            { "@StoreID", request.StoreId },
            { "@LoginID", request.LoginId },
            { "@ActualCashAmount", request.ActualCashAmount },
            { "@CarryForwardAmount", request.CarryForwardAmount },
            { "@Transaction", "CloseRegister" }
        };

                var result = await _dbExecutor.ExecuteStoredProcedureAsync(
                    "SP_RegisterSession",
                    parameters);

                tres.Status = "200";
                tres.Message = "Register Closed Successfully";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }

            return tres;
        }

        public async Task<TransactResult> GetFramesSalesReport(string fromDate, string toDate, int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@FromDate", fromDate },
                    { "@ToDate", toDate },
                    { "@StoreID", storeId }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetFramesSalesReport", parameters);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;

                decimal totalAmount = 0;
                decimal totalInsurance = 0;
                decimal totalNetTotal = 0;
                decimal totalBalance = 0;

                if (list != null)
                {
                    foreach (var row in list)
                    {
                        if (row.ContainsKey("PaymentAmount") && row["PaymentAmount"] != null)
                            totalAmount += Convert.ToDecimal(row["PaymentAmount"]);
                        if (row.ContainsKey("InsuranceAmount") && row["InsuranceAmount"] != null)
                            totalInsurance += Convert.ToDecimal(row["InsuranceAmount"]);
                        if (row.ContainsKey("NetTotal") && row["NetTotal"] != null)
                            totalNetTotal += Convert.ToDecimal(row["NetTotal"]);
                        if (row.ContainsKey("Balance") && row["Balance"] != null)
                            totalBalance += Convert.ToDecimal(row["Balance"]);
                    }
                }

                tres.extraData = new
                {
                    TotalAmount = totalAmount,
                    TotalInsurance = totalInsurance,
                    TotalNetTotal = totalNetTotal,
                    TotalBalance = totalBalance
                };
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetFramesSalesReport error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GetLensSalesReport(string fromDate, string toDate, int storeId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@FromDate", fromDate },
                    { "@ToDate", toDate },
                    { "@StoreID", storeId }
                };

                var list = await _dbExecutor.ExecuteStoredProcedureAsync("SP_GetLensSalesReport", parameters);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = list;
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetLensSalesReport error: " + ex.Message, ex.StackTrace);
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

   }
}
