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
    [Route("api/zatca")]
    [Authorize]
    public class ZatcaController : ControllerBase
    {
        private readonly IZatcaService _zatcaService;
        private readonly IDbLoggerService _dbLogger;

        public ZatcaController(IZatcaService zatcaService, IDbLoggerService dbLogger)
        {
            _zatcaService = zatcaService;
            _dbLogger = dbLogger;
        }

        [HttpPost("generate-csid")]
        public async Task<IActionResult> GenerateCsid(string otp)
        {
            var result = await _zatcaService.GenerateCsid(otp);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("Generateuuid")]
        public async Task<IActionResult> Generateuuid(string vat)
        {
            var result = await _zatcaService.Generateuuid(vat);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("GenerateProdCsid")]
        public async Task<IActionResult> GenerateProdCsid(string otp)
        {
            var result = await _zatcaService.GenerateProdCsid(otp);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}


