using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eyewa.Application.Interfaces;
using Eyewa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/TenantAccess")]
    [AllowAnonymous]
    public class TenantAccessController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public TenantAccessController(IApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpGet("default")]
        //public IActionResult GetDefaultTenantAccess()
        //{
        //    var config = new
        //    {
        //        hasProductsAccess = true,
        //        hasInsuranceAccess = true,
        //        hasRedmeePointsAccess = true
        //    };
        //    return Ok(config);
        //}

        [HttpGet("{tenantId}")]
        public async Task<ActionResult<TenantFeatureAccess>> GetTenantAccess(string tenantId)
        {
            var access = await _context.TenantFeatureAccesses
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            if (access == null)
            {
                // Return default config if not found
                return new TenantFeatureAccess
                {
                    TenantId = tenantId,
                    HasInsuranceAccess = false,
                    HasRedmeePointsAccess = false,
                    HasProductsAccess = true // default to true
                };
            }

            return access;
        }
    }
}
