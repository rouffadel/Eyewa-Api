using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Eyewa.Infrastructure.Data;
using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Infrastructure.Data;
using Eyewa.Application.DTOs;
using Eyewa.Application.Interfaces;
using Eyewa.Application.Services;
using Eyewa.Infrastructure.Services;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly IDbLoggerService _dbLogger;
        private readonly INotificationService _notificationService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IDbExecutorService _dbExecutor;

        public SalesController(
            ISalesService salesService, 
            IDbLoggerService dbLogger, 
            INotificationService notificationService,
            IWhatsAppService whatsAppService,
            IDbExecutorService dbExecutor)
        {
            _salesService = salesService;
            _dbLogger = dbLogger;
            _notificationService = notificationService;
            _whatsAppService = whatsAppService;
            _dbExecutor = dbExecutor;
        }

        [Route("InsertSales")]
        [HttpPost]
        public async Task<IActionResult> InsertSales([FromBody] SalesCls sales)
        {
            var result = await _salesService.InsertSales(sales);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetCustomerLoyaltyPoints")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerLoyaltyPoints(string customerNo)
        {
            var result = await _salesService.GetCustomerLoyaltyPoints(customerNo);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetTodayDeliveries")]
        [HttpGet]
        public async Task<IActionResult> GetTodayDeliveries(int storeId)
        {
            var result = await _salesService.GetTodayDeliveries(storeId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("SaveSalesDetails")]
        [HttpPost]
        public async Task<IActionResult> SaveSalesDetails([FromBody] SaveSalesDetails save)
        {
            var result = await _salesService.SaveSalesDetails(save);

            if (result.Status == "200")
            {
                // Fetch QR code
                var qrResult = await _salesService.GetZatcaQrBySalesId(save.SalesId);
                string qrImgBase64 = qrResult?.qrcodeimg ?? "";

                // WhatsApp Notification
                if (!string.IsNullOrEmpty(save.CustomerNo))
                {
                    try
                    {
                        string whatsappMsg = "Please find your receipt attached below.";

                        // Generate the Image receipt!
                        byte[] receiptImageBytes = Eyewa.Application.Helpers.ReceiptGenerator.GenerateImageReceipt(save, qrImgBase64, qrResult?.objresult);
                        string receiptImageBase64 = "data:image/png;base64," + Convert.ToBase64String(receiptImageBytes);

                        var (success, errMsg) = await _notificationService.SendWhatsAppMessageAsync(
                            save.CustomerNo,
                            whatsappMsg,
                            receiptImageBase64);

                        if (success)
                        {
                            _dbLogger.LogInfo($"WhatsApp receipt sent successfully to {save.CustomerNo}");
                        }
                        else
                        {
                            _dbLogger.LogError($"WhatsApp receipt sending failed for {save.CustomerNo}. Error: {errMsg}", "");
                            result.Message += $" | WhatsApp Error: {errMsg}";
                        }
                    }
                    catch (Exception ex)
                    {
                        _dbLogger.LogError(
                            $"WhatsApp receipt sending failed for {save.CustomerNo}. Error: {ex.Message}",
                            ex.StackTrace);
                        result.Message += $" | WhatsApp Error: {ex.Message}";
                    }
                }

                // Email Notification
                try
                {
                    string targetEmail = "ahmed.khan@fadelsoft.com";

                    string emailSubject = "Eyewa - Invoice Receipt";

                    // Generate the HTML receipt!
                    string emailBody = Eyewa.Application.Helpers.ReceiptGenerator.GenerateHtmlReceipt(save, qrImgBase64, qrResult?.objresult);

                    if (!string.IsNullOrEmpty(qrImgBase64))
                    {
                        result.qrcodeimg = qrImgBase64;
                    }

                    var (success, errMsg) = await _notificationService.SendEmailAsync(
                        targetEmail,
                        emailSubject,
                        emailBody,
                        qrImgBase64);

                    if (success)
                    {
                        _dbLogger.LogInfo($"Email sent successfully to {targetEmail}");
                    }
                    else
                    {
                        _dbLogger.LogError($"Email sending failed. Error: {errMsg}", "");
                        result.Message += $" | Email Error: {errMsg}";
                    }
                }
                catch (Exception ex)
                {
                    _dbLogger.LogError(
                        $"Email sending failed. Error: {ex.Message}",
                        ex.StackTrace);
                    result.Message += $" | Email Error: {ex.Message}";
                }

                return Ok(result);
            }

            return BadRequest(result);
        }

        [Route("GetSalesGrid")]
        [HttpPost]
        public async Task<IActionResult> GetSalesGrid([FromBody] SearchSalesCls obj)
        {
            var result = await _salesService.GetSalesGrid(obj);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetInvoiceDetails")]
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(int SalesId)
        {
            var result = await _salesService.GetInvoiceDetails(SalesId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetSalesPrint")]
        [HttpGet]
        public async Task<IActionResult> GetSalesPrint(int SalesId)
        {
            var result = await _salesService.GetSalesPrint(SalesId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetSalesDetailsGrid")]
        [HttpGet]
        public async Task<IActionResult> GetSalesDetailsGrid(int SalesId)
        {
            var result = await _salesService.GetZatcaQrBySalesId(SalesId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("DeleteSales")]
        [HttpGet]
        public async Task<IActionResult> DeleteSales(int SaleId, int LoginId)
        {
            var result = await _salesService.DeleteSales(SaleId, LoginId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("DeleteSalesDetails")]
        [HttpGet]
        public async Task<IActionResult> DeleteSalesDetails(int SaleId, int LoginId, int SalesDetailID)
        {
            var result = await _salesService.DeleteSalesDetails(SaleId, LoginId, SalesDetailID);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("customersearchfilter")]
        [HttpGet]
        public async Task<IActionResult> CustomerSearchFilter(string mobileNumber)
        {
            var result = await _salesService.CustomerSearchFilter(mobileNumber);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("OpenRegister")]
        [HttpPost]
        public async Task<IActionResult> OpenRegister([FromBody] OpenRegisterRequest request)
        {
            var result = await _salesService.OpenRegister(request);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }

        [Route("GetClosingSummary")]
        [HttpPost]
        public async Task<IActionResult> GetClosingSummary([FromBody] RegisterSummaryRequest request)
        {
            var result = await _salesService.GetClosingSummary(request);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }

        [Route("CloseRegister")]
        [HttpPost]
        public async Task<IActionResult> CloseRegister([FromBody] CloseRegisterRequest request)
        {
            var result = await _salesService.CloseRegister(request);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("GetFramesSalesReport")]
        public async Task<IActionResult> GetFramesSalesReport(string fromDate = "", string toDate = "", int storeId = 0)
        {
            var result = await _salesService.GetFramesSalesReport(fromDate, toDate, storeId);
            if (result.Status == "200")
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("GetLensSalesReport")]
        public async Task<IActionResult> GetLensSalesReport(string fromDate = "", string toDate = "", int storeId = 0)
        {
            var result = await _salesService.GetLensSalesReport(fromDate, toDate, storeId);
            if (result.Status == "200")
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [Route("statuses")]
        [HttpGet]
        public async Task<IActionResult> GetStatuses()
        {
            string sql = "SELECT Id, StatusName, Message, SendNotification, PdfTemplatePath, IsActive FROM OrderStatuses WHERE IsActive = 1";
            var statuses = await _dbExecutor.ExecuteQueryAsync(sql, null);
            return Ok(statuses);
        }

        [Route("sales-status-summary")]
        [HttpGet]
        public async Task<IActionResult> GetSalesStatusSummary([FromQuery] int? storeId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ISNULL(NetTotal, 0) AS NetTotal,
                        ISNULL(Balance, 0) AS Balance
                    FROM Sale
                    WHERE (IsDeleted IS NULL OR IsDeleted = 0)
                      AND (IsActive IS NULL OR IsActive = 1)
                      AND InvoiceDate >= CAST(GETDATE() AS DATE)
                      AND InvoiceDate < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))";

                var paramDict = new Dictionary<string, object?>();

                if (storeId.HasValue && storeId.Value > 0)
                {
                    sql += " AND StoreID = @StoreID";
                    paramDict["StoreID"] = storeId.Value;
                }

                var rows = await _dbExecutor.ExecuteQueryAsync(sql, paramDict.Count > 0 ? paramDict : null);

                int completed = 0;
                int pending = 0;
                int incomplete = 0;

                decimal completedAmount = 0m;
                decimal pendingAmount = 0m;
                decimal incompleteAmount = 0m;

                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        decimal net = 0m;
                        decimal bal = 0m;

                        if (row.ContainsKey("NetTotal") && row["NetTotal"] != null && row["NetTotal"] != DBNull.Value)
                            net = Convert.ToDecimal(row["NetTotal"]);

                        if (row.ContainsKey("Balance") && row["Balance"] != null && row["Balance"] != DBNull.Value)
                            bal = Convert.ToDecimal(row["Balance"]);

                        if (bal == 0m && net > 0m)
                        {
                            completed++;
                            completedAmount += net;
                        }
                        else if (bal > 0m && net > 0m)
                        {
                            pending++;
                            pendingAmount += net;
                        }
                        else
                        {
                            incomplete++;
                            incompleteAmount += net;
                        }
                    }
                }

                var result = new
                {
                    totalInvoices = (rows?.Count ?? 0),
                    completed = completed,
                    pending = pending,
                    incomplete = incomplete,
                    completedAmount = completedAmount,
                    pendingAmount = pendingAmount,
                    incompleteAmount = incompleteAmount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetSalesStatusSummary Error", ex.Message);
                return BadRequest(new { Status = "500", Message = ex.Message });
            }
        }

        [Route("order-status-list")]
        [HttpGet]
        public async Task<IActionResult> GetOrderStatusList(
            [FromQuery] int? storeId,
            [FromQuery] int? take,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                int limit = (take.HasValue && take.Value > 0) ? take.Value : 100;

                string sql = $@"
                    SELECT TOP ({limit})
                        s.SaleID AS SalesId,
                        ISNULL(s.InvoiceNo, '#' + CAST(s.SaleID AS VARCHAR)) AS InvoiceNo,
                        ISNULL(s.CustomerName, 'Walk-in Customer') AS CustomerName,
                        ISNULL(s.CustomerNo, '') AS CustomerNo,
                        ISNULL(s.GrossTotal, 0) AS GrossTotal,
                        ISNULL(s.NetTotal - s.Balance, 0) AS PaidAmount,
                        ISNULL(s.Balance, 0) AS Balance,
                        ISNULL(s.Discount, 0) AS DiscountAmount,
                        0 AS InsuranceAmount,
                        ot.StatusId AS OrderStatusId,
                        ISNULL(st.StatusName, 'Pending') AS StatusName,
                        ISNULL(s.InvoiceDate, s.CreatedDate) AS CreatedDate
                    FROM Sale s
                    LEFT JOIN (
                        SELECT OrderId, StatusId, ROW_NUMBER() OVER (PARTITION BY OrderId ORDER BY Id DESC) AS rn
                        FROM OrderTracking
                        WHERE IsActive = 1
                    ) ot ON s.SaleID = ot.OrderId AND ot.rn = 1
                    LEFT JOIN OrderStatuses st ON ot.StatusId = st.Id
                    WHERE (s.IsDeleted IS NULL OR s.IsDeleted = 0)
                      AND (s.IsActive IS NULL OR s.IsActive = 1)";

                var paramDict = new Dictionary<string, object?>();

                if (storeId.HasValue && storeId.Value > 0)
                {
                    sql += " AND s.StoreID = @StoreID";
                    paramDict["StoreID"] = storeId.Value;
                }

                if (fromDate.HasValue)
                {
                    sql += " AND s.InvoiceDate >= @FromDate";
                    paramDict["FromDate"] = fromDate.Value.Date;
                }

                if (toDate.HasValue)
                {
                    sql += " AND s.InvoiceDate < @ToDate";
                    paramDict["ToDate"] = toDate.Value.Date.AddDays(1);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    sql += " AND (LOWER(s.CustomerName) LIKE @Search OR LOWER(s.InvoiceNo) LIKE @Search OR LOWER(s.CustomerNo) LIKE @Search OR CAST(s.SaleID AS VARCHAR) LIKE @Search)";
                    paramDict["Search"] = "%" + search.Trim().ToLower() + "%";
                }

                sql += " ORDER BY s.InvoiceDate DESC, s.SaleID DESC";

                var list = await _dbExecutor.ExecuteQueryAsync(sql, paramDict.Count > 0 ? paramDict : null) ?? new List<Dictionary<string, object>>();
                var enrichedList = await EnrichOrderStatusList(list);
                return Ok(enrichedList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private async Task<List<Dictionary<string, object>>> EnrichOrderStatusList(List<Dictionary<string, object>> list)
        {
            if (list == null || list.Count == 0) return list;

            var salesIds = new List<int>();
            foreach (var row in list)
            {
                int salesId = 0;
                if (row.ContainsKey("SalesId") && row["SalesId"] != null)
                    salesId = Convert.ToInt32(row["SalesId"]);
                else if (row.ContainsKey("SaleID") && row["SaleID"] != null)
                    salesId = Convert.ToInt32(row["SaleID"]);

                if (salesId > 0 && !salesIds.Contains(salesId))
                {
                    salesIds.Add(salesId);
                }
            }

            if (salesIds.Count == 0) return list;

            string idListStr = string.Join(",", salesIds);
            string batchSql = $@"
                SELECT 
                    v.SalesId,
                    SUM(v.ItemsSum) AS ItemsSum,
                    SUM(v.LensesSum) AS LensesSum,
                    SUM(v.PaidSum) AS PaidSum
                FROM (
                    SELECT SalesID AS SalesId, ISNULL(SUM(Quantity * SellingPrice), 0) AS ItemsSum, 0 AS LensesSum, 0 AS PaidSum
                    FROM SalesDetails WHERE SalesID IN ({idListStr}) AND IsActive = 1 GROUP BY SalesID
                    UNION ALL
                    SELECT SalesID AS SalesId, 0 AS ItemsSum, ISNULL(SUM(Quantity * Price), 0) AS LensesSum, 0 AS PaidSum
                    FROM OrderLense WHERE SalesID IN ({idListStr}) AND IsActive = 1 GROUP BY SalesID
                    UNION ALL
                    SELECT SaleID AS SalesId, 0 AS ItemsSum, 0 AS LensesSum, ISNULL(SUM(PaymentAmount), 0) AS PaidSum
                    FROM InvoicePayment WHERE SaleID IN ({idListStr}) AND IsActive = 1 GROUP BY SaleID
                ) v
                GROUP BY v.SalesId";

            var summaryRows = await _dbExecutor.ExecuteQueryAsync(batchSql);
            var summaryMap = new Dictionary<int, (double itemsSum, double lensesSum, double paidSum)>();

            if (summaryRows != null)
            {
                foreach (var sRow in summaryRows)
                {
                    if (sRow.ContainsKey("SalesId") && sRow["SalesId"] != null)
                    {
                        int sId = Convert.ToInt32(sRow["SalesId"]);
                        double iSum = sRow.ContainsKey("ItemsSum") && sRow["ItemsSum"] != null ? Convert.ToDouble(sRow["ItemsSum"]) : 0;
                        double lSum = sRow.ContainsKey("LensesSum") && sRow["LensesSum"] != null ? Convert.ToDouble(sRow["LensesSum"]) : 0;
                        double pSum = sRow.ContainsKey("PaidSum") && sRow["PaidSum"] != null ? Convert.ToDouble(sRow["PaidSum"]) : 0;
                        summaryMap[sId] = (iSum, lSum, pSum);
                    }
                }
            }

            foreach (var row in list)
            {
                int salesId = 0;
                if (row.ContainsKey("SalesId") && row["SalesId"] != null)
                    salesId = Convert.ToInt32(row["SalesId"]);
                else if (row.ContainsKey("SaleID") && row["SaleID"] != null)
                    salesId = Convert.ToInt32(row["SaleID"]);

                double grossTotal = 0;
                if (row.ContainsKey("GrossTotal") && row["GrossTotal"] != null)
                    grossTotal = Convert.ToDouble(row["GrossTotal"]);

                summaryMap.TryGetValue(salesId, out var totals);

                if (grossTotal == 0)
                {
                    grossTotal = totals.itemsSum + totals.lensesSum;
                    if (grossTotal > 0)
                    {
                        grossTotal = Math.Round(grossTotal * 1.15, 2);
                    }
                    row["GrossTotal"] = grossTotal;
                }

                double paidAmount = totals.paidSum;
                if (paidAmount == 0 && row.ContainsKey("PaidAmount") && row["PaidAmount"] != null && row["PaidAmount"] != DBNull.Value)
                {
                    paidAmount = Convert.ToDouble(row["PaidAmount"]);
                }
                row["PaidAmount"] = paidAmount;

                double balance = grossTotal - paidAmount;
                if (balance < 0) balance = 0;
                if (totals.paidSum == 0 && row.ContainsKey("Balance") && row["Balance"] != null && row["Balance"] != DBNull.Value)
                {
                    balance = Convert.ToDouble(row["Balance"]);
                }
                row["Balance"] = balance;
            }

            return list;
        }


        [Route("{orderId}/status")]
        [HttpPut]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] int statusId)
        {
            try
            {
                // Note: Update Order status logic goes here
                // We'd ideally have an OrderStatuses table in the same DB or synchronize it
                
                // 1. Fetch Sales/Order details
                string sqlSale = "SELECT SaleID AS SalesId, StoreID, TenantId, CreatedBy, CustomerNo, CustomerName FROM Sale WHERE SaleID = @SalesId";
                var salesData = await _dbExecutor.ExecuteQueryAsync(sqlSale, new Dictionary<string, object?> { { "SalesId", orderId } });
                var sale = salesData.FirstOrDefault();

                if (sale == null) return NotFound(new { Status = "404", Message = "Order not found" });

                // 2. Fetch Status details from OrderStatuses
                string sqlStatus = "SELECT StatusName, PdfTemplatePath FROM OrderStatuses WHERE Id = @Id";
                var statusData = await _dbExecutor.ExecuteQueryAsync(sqlStatus, new Dictionary<string, object?> { { "Id", statusId } });
                var status = statusData.FirstOrDefault();

                if (status == null) return NotFound(new { Status = "404", Message = "Status not found" });

                var storeId = sale.ContainsKey("StoreID") && sale["StoreID"] != null ? Convert.ToInt32(sale["StoreID"]) : 0;
                var createdBy = sale.ContainsKey("CreatedBy") && sale["CreatedBy"] != null ? Convert.ToInt32(sale["CreatedBy"]) : 1;
                var tenantId = 1; // OrderTracking.TenantId is an int, but Sale.TenantId is a Guid string. Default to 1.

                // 3. Update the Sales table status
                // We'll insert into OrderTracking instead since Sale doesn't have OrderStatusId
                string sqlTracking = @"
                    IF NOT EXISTS (SELECT 1 FROM OrderTracking WHERE OrderId = @SalesId)
                        INSERT INTO OrderTracking (OrderId, StoreId, StatusId, Remarks, TenantId, CreatedBy, CreatedAt, IsActive) 
                        VALUES (@SalesId, @StoreId, @StatusId, 'Status updated', @TenantId, @CreatedBy, GETDATE(), 1)
                    ELSE
                        UPDATE OrderTracking SET StatusId = @StatusId, ModifiedAt = GETDATE() WHERE OrderId = @SalesId";
                await _dbExecutor.ExecuteQueryAsync(sqlTracking, new Dictionary<string, object?> { 
                    { "StatusId", statusId }, 
                    { "SalesId", orderId }, 
                    { "StoreId", storeId },
                    { "TenantId", tenantId },
                    { "CreatedBy", createdBy }
                });

                var customerNo = sale.ContainsKey("CustomerNo") ? sale["CustomerNo"]?.ToString() : null;
                var customerName = sale.ContainsKey("CustomerName") ? sale["CustomerName"]?.ToString() : null;
                var pdfTemplatePath = status.ContainsKey("PdfTemplatePath") ? status["PdfTemplatePath"]?.ToString() : null;
                var statusName = status.ContainsKey("StatusName") ? status["StatusName"]?.ToString() : null;

                // 4. Send WhatsApp Notification
                if (!string.IsNullOrEmpty(customerNo) && !string.IsNullOrEmpty(pdfTemplatePath))
                {
                    string message = _whatsAppService.FormatMessage(
                        pdfTemplatePath, 
                        customerName ?? "Customer", 
                        orderId.ToString(), 
                        statusName);
                        
                    await _notificationService.SendWhatsAppMessageAsync(customerNo, message);
                }

                return Ok(new { Status = "200", Message = "Status updated and notification sent" });
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("UpdateOrderStatus Error", ex.Message);
                return BadRequest(new { Status = "500", Message = ex.Message });
            }
        }

        [Route("record-payment")]
        [HttpPost]
        public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentRequest request)
        {
            try
            {
                // Insert into InvoicePayment
                string sqlInsert = @"
                    INSERT INTO InvoicePayment (SaleID, PaymentAmount, PaymentMode, IsActive, CreatedDate) 
                    VALUES (@SaleID, @PaymentAmount, @PaymentMode, 1, GETDATE())";
                await _dbExecutor.ExecuteQueryAsync(sqlInsert, new Dictionary<string, object?> {
                    { "@SaleID", request.SalesId },
                    { "@PaymentAmount", request.PaymentAmount },
                    { "@PaymentMode", request.PaymentMode }
                });

                // Update the Balance in Sale table
                string sqlUpdate = @"
                    UPDATE Sale 
                    SET Balance = CASE WHEN (Balance - @PaymentAmount) < 0 THEN 0 ELSE (Balance - @PaymentAmount) END 
                    WHERE SaleID = @SalesId";
                await _dbExecutor.ExecuteQueryAsync(sqlUpdate, new Dictionary<string, object?> {
                    { "@SalesId", request.SalesId },
                    { "@PaymentAmount", request.PaymentAmount }
                });

                return Ok(new { Status = "200", Message = "Payment recorded successfully" });
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("RecordPayment Error", ex.Message);
                return BadRequest(new { Status = "500", Message = ex.Message });
            }
        }
    }

    public class RecordPaymentRequest
    {
        public int SalesId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
    }
}
