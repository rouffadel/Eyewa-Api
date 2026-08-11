using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/TenantAccess")]
    [AllowAnonymous]
    public class TenantAccessController : ControllerBase
    {
        [HttpGet("default")]
        public IActionResult GetDefaultTenantAccess()
        {
            var config = new
            {
                hasProductsAccess = true,
                hasInsuranceAccess = true,
                hasRedmeePointsAccess = true
            };
            return Ok(config);
        }
    }
}
