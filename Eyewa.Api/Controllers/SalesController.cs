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

        public SalesController(ISalesService salesService, IDbLoggerService dbLogger, INotificationService notificationService)
        {
            _salesService = salesService;
            _dbLogger = dbLogger;
            _notificationService = notificationService;
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
                        byte[] receiptImageBytes = Eyewa.Application.Helpers.ReceiptGenerator.GenerateImageReceipt(save, qrImgBase64);
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
                    string emailBody = Eyewa.Application.Helpers.ReceiptGenerator.GenerateHtmlReceipt(save, qrImgBase64);

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
    }
}
