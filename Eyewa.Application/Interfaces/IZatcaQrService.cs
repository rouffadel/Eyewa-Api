using System;

namespace Eyewa.Application.Interfaces
{
    public interface IZatcaQrService
    {
        string GenerateZatcaQrCode(string sellerName, string vatRegistrationNumber, DateTime timestamp, decimal invoiceTotal, decimal vatTotal);
        byte[] GenerateQrCodeImage(string qrCodeBase64);
    }
}
