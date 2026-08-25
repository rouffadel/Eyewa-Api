using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eyewa.Application.Interfaces;
using Eyewa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/taxes")]
    [AllowAnonymous]
    public class TaxController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IDbExecutorService _dbExecutor;
        private readonly IDbLoggerService _dbLogger;

        public TaxController(IApplicationDbContext context, IDbExecutorService dbExecutor, IDbLoggerService dbLogger)
        {
            _context = context;
            _dbExecutor = dbExecutor;
            _dbLogger = dbLogger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTaxes()
        {
            try
            {
                // Query database taxes
                var taxes = await _context.Taxes.ToListAsync();
                if (taxes == null || !taxes.Any())
                {
                    // Fallback to sample/default VAT rate if table is empty
                    taxes = new List<Tax>
                    {
                        new Tax { TaxId = 1, TaxName = "VAT Standard", TaxCode = "VAT15", TaxRate = 15.00m, IsDefault = true, IsActive = true },
                        new Tax { TaxId = 2, TaxName = "Zero Tax", TaxCode = "VAT0", TaxRate = 0.00m, IsDefault = false, IsActive = true }
                    };
                }
                return Ok(taxes);
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetTaxes error: " + ex.Message, ex.StackTrace);
                // Return default list if table query fails
                var defaultTaxes = new List<Tax>
                {
                    new Tax { TaxId = 1, TaxName = "VAT Standard", TaxCode = "VAT15", TaxRate = 15.00m, IsDefault = true, IsActive = true },
                    new Tax { TaxId = 2, TaxName = "Zero Tax", TaxCode = "VAT0", TaxRate = 0.00m, IsDefault = false, IsActive = true }
                };
                return Ok(defaultTaxes);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTax([FromBody] Tax tax)
        {
            if (tax == null) return BadRequest("Invalid tax payload");
            try
            {
                if (tax.IsDefault)
                {
                    var existingDefaults = await _context.Taxes.Where(t => t.IsDefault).ToListAsync();
                    foreach (var d in existingDefaults)
                    {
                        d.IsDefault = false;
                    }
                }

                _context.Taxes.Add(tax);
                await _context.SaveChangesAsync();
                return Ok(tax);
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("CreateTax error: " + ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTax(int id, [FromBody] Tax tax)
        {
            if (tax == null) return BadRequest("Invalid tax payload");
            try
            {
                var existing = await _context.Taxes.FindAsync(id);
                if (existing == null)
                {
                    return NotFound($"Tax with ID {id} not found");
                }

                if (tax.IsDefault && !existing.IsDefault)
                {
                    var existingDefaults = await _context.Taxes.Where(t => t.IsDefault && t.TaxId != id).ToListAsync();
                    foreach (var d in existingDefaults)
                    {
                        d.IsDefault = false;
                    }
                }

                existing.TaxName = tax.TaxName;
                existing.TaxCode = tax.TaxCode;
                existing.TaxRate = tax.TaxRate;
                existing.IsDefault = tax.IsDefault;
                existing.IsActive = tax.IsActive;

                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("UpdateTax error: " + ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTax(int id)
        {
            try
            {
                var existing = await _context.Taxes.FindAsync(id);
                if (existing != null)
                {
                    _context.Taxes.Remove(existing);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { status = "200", message = "Tax deleted successfully" });
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("DeleteTax error: " + ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }
    }
}
