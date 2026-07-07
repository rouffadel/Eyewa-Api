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
    [Route("api/prescriptions")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionsService _prescriptionsService;
        private readonly IDbLoggerService _dbLogger;

        public PrescriptionsController(IPrescriptionsService prescriptionsService, IDbLoggerService dbLogger)
        {
            _prescriptionsService = prescriptionsService;
            _dbLogger = dbLogger;
        }

        [Route("GetOrderLense")]
        [HttpGet]
        public async Task<IActionResult> GetOrderLense(int SalesId)
        {
            var result = await _prescriptionsService.GetOrderLense(SalesId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetPrescriptionDropDowns")]
        [HttpGet]
        public async Task<IActionResult> GetPrescriptionDropDowns()
        {
            var result = await _prescriptionsService.GetPrescriptionDropDowns();
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("SaveOrderLense")]
        [HttpPost]
        public async Task<IActionResult> SaveOrderLense([FromBody] OrderLenseItemsCls order)
        {
            var result = await _prescriptionsService.SaveOrderLense(order);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}


