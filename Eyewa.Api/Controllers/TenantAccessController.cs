using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eyewa.Application.Interfaces;
using Eyewa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
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

        [HttpGet("{tenantId}")]
        public async Task<ActionResult<TenantFeatureAccess>> GetTenantAccess(string tenantId)
        {
            try
            {
                var access = await _context.TenantFeatureAccesses
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (access != null)
                {
                    return access;
                }
            }
            catch (Exception)
            {
                // Fallback raw SQL reading HasTaxAccess dynamically if column exists or default 1 if missing
                try
                {
                    var sqlAccess = await _context.TenantFeatureAccesses
                        .FromSqlRaw("SELECT Id, TenantId, HasInsuranceAccess, HasRedmeePointsAccess, HasProductsAccess, HasOffersAccess, " +
                                    "CASE WHEN COL_LENGTH('TenantFeatureAccesses', 'HasTaxAccess') IS NOT NULL THEN HasTaxAccess ELSE CAST(1 AS BIT) END AS HasTaxAccess " +
                                    "FROM TenantFeatureAccesses WHERE TenantId = {0}", tenantId)
                        .FirstOrDefaultAsync();

                    if (sqlAccess != null)
                    {
                        return sqlAccess;
                    }
                }
                catch (Exception) { }
            }

            return new TenantFeatureAccess
            {
                TenantId = tenantId,
                HasInsuranceAccess = false,
                HasRedmeePointsAccess = false,
                HasProductsAccess = true,
                HasOffersAccess = true,
                HasTaxAccess = true
            };
        }

        [HttpPost]
        public async Task<ActionResult<TenantFeatureAccess>> SaveTenantAccess([FromBody] TenantFeatureAccess config)
        {
            if (config == null) return BadRequest("Invalid configuration payload");

            try
            {
                var existing = await _context.TenantFeatureAccesses
                    .FirstOrDefaultAsync(t => t.TenantId == config.TenantId);

                if (existing == null)
                {
                    _context.TenantFeatureAccesses.Add(config);
                }
                else
                {
                    existing.HasInsuranceAccess = config.HasInsuranceAccess;
                    existing.HasRedmeePointsAccess = config.HasRedmeePointsAccess;
                    existing.HasProductsAccess = config.HasProductsAccess;
                    existing.HasOffersAccess = config.HasOffersAccess;
                    try { existing.HasTaxAccess = config.HasTaxAccess; } catch { }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception) { }

            return Ok(config);
        }
    }
}
