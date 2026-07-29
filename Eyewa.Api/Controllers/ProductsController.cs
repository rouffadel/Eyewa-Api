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

        [Route("SearchProductByKey")]
        [HttpPost]
        public async Task<IActionResult> SearchProductByKey(int StoreId, string ProductName)
        {
            var result = await _productsService.SearchProductByKey(StoreId, ProductName);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }

        [Route("GetAllProductsByStoreId")]
        [HttpGet]
        public async Task<IActionResult> GetAllProductsByStoreId(int StoreId)
        {
            var result = await _productsService.GetAllProductsByStoreId(StoreId);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }

        [Route("GetCategoryBrandByProduct")]
        [HttpPost]
        public async Task<IActionResult> GetCategoryBrandByProduct(int productID)
        {
            var result = await _productsService.GetCategoryBrandByProduct(productID);

            if (result.Status == "200")
                return Ok(result);

            return BadRequest(result);
        }
    }
}


