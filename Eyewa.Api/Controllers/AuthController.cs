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
    [Route("api/auth")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IDbLoggerService _dbLogger;

        public AuthController(IAuthService authService, IDbLoggerService dbLogger)
        {
            _authService = authService;
            _dbLogger = dbLogger;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string username, string password, int roleId = 2)
        {
            var result = await _authService.Register(username, password, roleId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(string token)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _authService.RefreshToken(token, ipAddress);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("RevokeToken")]
        public async Task<IActionResult> RevokeToken(string token)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _authService.RevokeToken(token, ipAddress);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("VerifyUserLogin")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyUserLogin(string LoginName, string Password)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _authService.VerifyUserLogin(LoginName, Password, ipAddress);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers(int LoginId, int StoreId)
        {
            var result = await _authService.GetUsers(LoginId, StoreId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetSalesMan")]
        [HttpGet]
        public async Task<IActionResult> GetSalesMan(int LoginId, int StoreId)
        {
            var result = await _authService.GetSalesMan(LoginId, StoreId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("SyncLegacyUsers")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SyncLegacyUsers()
        {
            await _authService.SyncLegacyUsersAsync();
            return Ok(new { Message = "Synchronization triggered successfully. Check logs for details." });
        }
    }
}


