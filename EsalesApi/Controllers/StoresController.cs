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
    }
}
