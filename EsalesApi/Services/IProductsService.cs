using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface IProductsService
    {
        Task<TransactResult> FillCategory();
        Task<TransactResult> FillBrand();
        Task<TransactResult> GetBrand(string brandName);
        Task<TransactResult> GetProduct(int categoryId, int brandId, int storeId, string productName);
        Task<TransactResult> GetQuantity(int productId);
    }
}
