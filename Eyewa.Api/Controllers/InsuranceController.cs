using Eyewa.Application.DTOs;
using Eyewa.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/insurance")]
    [Authorize]
    public class InsuranceController : ControllerBase
    {
        private readonly IInsuranceService _insuranceService;

        public InsuranceController(IInsuranceService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        [HttpPost]
        [Route("SaveInsuranceCompany")]
        public async Task<IActionResult> SaveInsuranceCompany([FromBody] InsuranceCompanyDto obj)
        {
            var result = await _insuranceService.SaveInsuranceCompany(obj);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost]
        [Route("SaveSalesInsurance")]
        public async Task<IActionResult> SaveSalesInsurance([FromBody] SalesInsuranceDto obj)
        {
            var result = await _insuranceService.SaveSalesInsurance(obj);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet]
        [Route("GetInsuranceBySalesId")]
        public async Task<IActionResult> GetInsuranceBySalesId(int salesId)
        {
            var result = await _insuranceService.GetInsuranceBySalesId(salesId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet]
        [Route("GetAllInsuranceCompanies")]
        public async Task<IActionResult> GetAllInsuranceCompanies()
        {
            var result = await _insuranceService.GetAllInsuranceCompanies();
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}
