using Eyewa_new_api.Models;
using Eyewa_new_api.Services;
using Eyewa_new_api.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly IDbLoggerService _dbLogger;

        public SalesController(ISalesService salesService, IDbLoggerService dbLogger)
        {
            _salesService = salesService;
            _dbLogger = dbLogger;
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

        [Route("SaveSalesDetails")]
        [HttpPost]
        public async Task<IActionResult> SaveSalesDetails([FromBody] SaveSalesDetails save)
        {
            var result = await _salesService.SaveSalesDetails(save);
            if (result.Status == "200")
                return Ok(result);
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
            var result = await _salesService.GetSalesDetailsGrid(SalesId);
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
    }
}
