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
    [Route("api/stores")]
    [Authorize]
    public class StoresController : ControllerBase
    {
        private readonly IStoresService _storesService;
        private readonly IDbLoggerService _dbLogger;

        public StoresController(IStoresService storesService, IDbLoggerService dbLogger)
        {
            _storesService = storesService;
            _dbLogger = dbLogger;
        }

        [Route("FillStore")]
        [HttpGet]
        public async Task<IActionResult> FillStore(int LoginId, int StoreId)
        {
            var result = await _storesService.FillStore(LoginId, StoreId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Authorize]
        [Route("GetUserStores")]
        [HttpGet]
        public async Task<IActionResult> GetUserStores()
        {
            var result = await _storesService.GetUserStores(User);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }
    }
}


