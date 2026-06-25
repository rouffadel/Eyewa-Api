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
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly IDbLoggerService _dbLogger;

        public ProductsController(IProductsService productsService, IDbLoggerService dbLogger)
        {
            _productsService = productsService;
            _dbLogger = dbLogger;
        }

        [Route("FillCategory")]
        [HttpGet]
        public async Task<IActionResult> FillCategory()
        {
            var result = await _productsService.FillCategory();
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("FillBrand")]
        [HttpGet]
        public async Task<IActionResult> FillBrand()
        {
            var result = await _productsService.FillBrand();
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetBrand")]
        [HttpGet]
        public async Task<IActionResult> GetBrand(string BrandName)
        {
            var result = await _productsService.GetBrand(BrandName);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetProduct")]
        [HttpGet]
        public async Task<IActionResult> GetProduct(int CategoryId, int BrandId, int StoreId, string ProductName)
        {
            var result = await _productsService.GetProduct(CategoryId, BrandId, StoreId, ProductName);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }

        [Route("GetQuantity")]
        [HttpGet]
        public async Task<IActionResult> GetQuantity(int ProductId)
        {
            var result = await _productsService.GetQuantity(ProductId);
            if (result.Status == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}
