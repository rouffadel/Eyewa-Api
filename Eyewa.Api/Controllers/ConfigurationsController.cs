using Eyewa.Application.Interfaces;
using Eyewa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IDbLoggerService _dbLogger;

        public ConfigurationsController(IApplicationDbContext context, IDbLoggerService dbLogger)
        {
            _context = context;
            _dbLogger = dbLogger;
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetCompanyConfiguration()
        {
            try
            {
                var config = await _context.CompanyConfigurations
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (config == null)
                {
                    config = new CompanyConfiguration
                    {
                        CompanyName = "Eyewa",
                        AppTitle = "Eyewa Admin Portal",
                        CompanyLogoUrl = "/assets/logos/eyewa-logo.png"
                    };
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                _dbLogger?.LogError("GetCompanyConfiguration error: " + ex.Message, ex.StackTrace);
                return Ok(new CompanyConfiguration
                {
                    CompanyName = "Eyewa",
                    AppTitle = "Eyewa Admin Portal",
                    CompanyLogoUrl = "/assets/logos/eyewa-logo.png"
                });
            }
        }

        [HttpPost("company")]
        public async Task<IActionResult> SaveCompanyConfiguration([FromBody] CompanyConfiguration dto)
        {
            if (dto == null) return BadRequest("Invalid configuration payload");

            try
            {
                var existing = await _context.CompanyConfigurations
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    existing = new CompanyConfiguration();
                    _context.CompanyConfigurations.Add(existing);
                }

                existing.CompanyName = string.IsNullOrWhiteSpace(dto.CompanyName) ? "Eyewa" : dto.CompanyName;
                existing.AppTitle = string.IsNullOrWhiteSpace(dto.AppTitle) ? $"{existing.CompanyName} Admin Portal" : dto.AppTitle;
                existing.CompanyLogoBase64 = dto.CompanyLogoBase64;
                existing.CompanyLogoUrl = dto.CompanyLogoUrl;
                existing.UpdatedBy = dto.UpdatedBy ?? "Admin";
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                _dbLogger?.LogError("SaveCompanyConfiguration error: " + ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }
    }
}
