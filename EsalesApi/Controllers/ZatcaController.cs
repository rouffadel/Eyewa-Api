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
