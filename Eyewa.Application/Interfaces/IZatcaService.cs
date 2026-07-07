using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Threading.Tasks;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;


namespace Eyewa.Application.Interfaces
{
    public interface IZatcaService
    {
        Task<TransactResult> GenerateCsid(string otp);
        Task<TransactResult> Generateuuid(string vat);
        Task<TransactResult> GenerateProdCsid(string otp);
    }
}





