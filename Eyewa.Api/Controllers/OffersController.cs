using Eyewa.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eyewa.Api.Controllers
{
    [ApiController]
    [Route("api/offers")]
    [Authorize]
    public class OffersController : ControllerBase
    {
        private readonly IDbExecutorService _dbExecutor;
        private readonly IDbLoggerService _dbLogger;

        public OffersController(IDbExecutorService dbExecutor, IDbLoggerService dbLogger)
        {
            _dbExecutor = dbExecutor;
            _dbLogger = dbLogger;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOffers()
        {
            try
            {
                _dbLogger.LogInfo("GetActiveOffers endpoint started");

                var offersQuery = @"
                    SELECT 
                        OfferID, 
                        OfferName, 
                        OfferCode, 
                        DiscountValue, 
                        DiscountType, 
                        StartDate, 
                        EndDate, 
                        OfferType, 
                        BuyQuantity, 
                        GetQuantity
                    FROM Offer
                    WHERE IsActive = 1 AND IsDeleted = 0 
                      AND (StartDate IS NULL OR StartDate <= GETDATE()) 
                      AND (EndDate IS NULL OR EndDate >= GETDATE())";

                var mappingsQuery = @"
                    SELECT 
                        op.OfferID, 
                        op.ProductID, 
                        op.ProductRole,
                        p.ProductName,
                        p.ProductValue,
                        p.CategoryID,
                        c.CategoryName,
                        p.BrandID,
                        ISNULL(b.BrandName, '') AS BrandName
                    FROM OfferProduct op
                    INNER JOIN Products p ON p.ProductID = op.ProductID
                    INNER JOIN Category c ON c.CategoryID = p.CategoryID
                    LEFT JOIN Brand b ON b.BrandID = p.BrandID";

                var offersResult = await _dbExecutor.ExecuteQueryAsync(offersQuery);
                var mappingsResult = await _dbExecutor.ExecuteQueryAsync(mappingsQuery);

                var response = new List<object>();

                foreach (var offer in offersResult)
                {
                    var offerId = Convert.ToInt32(offer["OfferID"]);
                    var buyProductIds = mappingsResult
                        .Where(m => Convert.ToInt32(m["OfferID"]) == offerId && Convert.ToString(m["ProductRole"]) == "Buy")
                        .Select(m => Convert.ToInt32(m["ProductID"]))
                        .ToList();

                    var getProductIds = mappingsResult
                        .Where(m => Convert.ToInt32(m["OfferID"]) == offerId && Convert.ToString(m["ProductRole"]) == "Get")
                        .Select(m => Convert.ToInt32(m["ProductID"]))
                        .ToList();

                    var selectedGetProductDetails = mappingsResult
                        .Where(m => Convert.ToInt32(m["OfferID"]) == offerId && Convert.ToString(m["ProductRole"]) == "Get")
                        .Select(m => new
                        {
                            productId = Convert.ToInt32(m["ProductID"]),
                            productName = Convert.ToString(m["ProductName"]),
                            productValue = Convert.ToDecimal(m["ProductValue"]),
                            categoryId = Convert.ToInt32(m["CategoryID"]),
                            categoryName = Convert.ToString(m["CategoryName"]),
                            brandId = m["BrandID"] == DBNull.Value ? (int?)null : Convert.ToInt32(m["BrandID"]),
                            brandName = Convert.ToString(m["BrandName"])
                        })
                        .ToList();

                    var selectedBuyProductDetails = mappingsResult
                        .Where(m => Convert.ToInt32(m["OfferID"]) == offerId && Convert.ToString(m["ProductRole"]) == "Buy")
                        .Select(m => new
                        {
                            productId = Convert.ToInt32(m["ProductID"]),
                            productName = Convert.ToString(m["ProductName"]),
                            productValue = Convert.ToDecimal(m["ProductValue"]),
                            categoryId = Convert.ToInt32(m["CategoryID"]),
                            categoryName = Convert.ToString(m["CategoryName"]),
                            brandId = m["BrandID"] == DBNull.Value ? (int?)null : Convert.ToInt32(m["BrandID"]),
                            brandName = Convert.ToString(m["BrandName"])
                        })
                        .ToList();

                    response.Add(new
                    {
                        offerID = offerId,
                        offerName = Convert.ToString(offer["OfferName"]),
                        offerCode = Convert.ToString(offer["OfferCode"]),
                        discountValue = Convert.ToDecimal(offer["DiscountValue"]),
                        discountType = Convert.ToString(offer["DiscountType"]),
                        startDate = offer["StartDate"],
                        endDate = offer["EndDate"],
                        offerType = Convert.ToString(offer["OfferType"]),
                        buyQuantity = offer["BuyQuantity"] == DBNull.Value || offer["BuyQuantity"] == null ? (int?)null : Convert.ToInt32(offer["BuyQuantity"]),
                        getQuantity = offer["GetQuantity"] == DBNull.Value || offer["GetQuantity"] == null ? (int?)null : Convert.ToInt32(offer["GetQuantity"]),
                        selectedBuyProductIds = buyProductIds,
                        selectedGetProductIds = getProductIds,
                        selectedGetProductDetails = selectedGetProductDetails,
                        selectedBuyProductDetails = selectedBuyProductDetails
                    });
                }

                return Ok(new { status = "200", message = "Success", objresult = response });
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetActiveOffers error: " + ex.Message, ex.StackTrace);
                return BadRequest(new { status = "-100", message = ex.Message });
            }
        }
    }
}
