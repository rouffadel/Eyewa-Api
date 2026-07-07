using System;
using System.Collections.Generic;
using System.Text;
using QRCoder;
using Eyewa.Application.Interfaces;

namespace Eyewa.Application.Services
{
    public class ZatcaQrService : IZatcaQrService
    {
        public string GenerateZatcaQrCode(string sellerName, string vatRegistrationNumber, DateTime timestamp, decimal invoiceTotal, decimal vatTotal)
        {
            var tlvList = new List<byte>();

            // Tag 1: Seller Name
            tlvList.AddRange(GetTlvBytes(1, sellerName));

            // Tag 2: VAT Registration Number
            tlvList.AddRange(GetTlvBytes(2, vatRegistrationNumber));

            // Tag 3: Timestamp
            tlvList.AddRange(GetTlvBytes(3, timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")));

            // Tag 4: Invoice Total
            tlvList.AddRange(GetTlvBytes(4, invoiceTotal.ToString("0.00")));

            // Tag 5: VAT Total
            tlvList.AddRange(GetTlvBytes(5, vatTotal.ToString("0.00")));

            return Convert.ToBase64String(tlvList.ToArray());
        }

        private byte[] GetTlvBytes(int tag, string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value ?? "");
            var tlv = new List<byte>
            {
                (byte)tag,
                (byte)valueBytes.Length
            };
            tlv.AddRange(valueBytes);
            return tlv.ToArray();
        }

        public byte[] GenerateQrCodeImage(string qrCodeBase64)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrCodeBase64, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
