using System.Threading.Tasks;
using Eyewa_new_api.Models;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Services
{
    public interface IZatcaService
    {
        Task<TransactResult> GenerateCsid(string otp);
        Task<TransactResult> Generateuuid(string vat);
        Task<TransactResult> GenerateProdCsid(string otp);
    }
}
