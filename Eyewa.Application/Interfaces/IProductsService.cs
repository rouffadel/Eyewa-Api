using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;


namespace Eyewa.Application.Interfaces
{
    public interface IProductsService
    {
        Task<TransactResult> FillCategory();
        Task<TransactResult> FillBrand();
        Task<TransactResult> GetBrand(string brandName);
        Task<TransactResult> GetProduct(int categoryId, int brandId, int storeId, string productName);
        Task<TransactResult> GetQuantity(int productId);
        Task<TransactResult> SearchProductByKey(int StoreId, String ProductName);
        Task<TransactResult> GetCategoryBrandByProduct(int productID);
        Task<TransactResult> GetAllProductsByStoreId(int storeId);

    }
}





