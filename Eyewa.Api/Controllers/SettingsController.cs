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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IDbLoggerService _dbLogger;

        public SettingsController(ISettingsService settingsService, IDbLoggerService dbLogger)
        {
            _settingsService = settingsService;
            _dbLogger = dbLogger;
        }

        [HttpGet("GetNotificationSettings")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var result = await _settingsService.GetNotificationSettings();
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("SaveNotificationSettings")]
        public async Task<IActionResult> SaveNotificationSettings([FromBody] NotificationSettings settings)
        {
            var result = await _settingsService.SaveNotificationSettings(settings);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}


