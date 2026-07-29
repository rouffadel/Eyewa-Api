using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Helpers
{
    public static class ReceiptGenerator
    {
        public static string GenerateHtmlReceipt(SaveSalesDetails save, string qrBase64)
        {
            string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .receipt-container {{ width: 400px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; margin-bottom: 20px; }}
                    .header h2 {{ margin: 0; color: #4CAF50; }}
                    .item-row {{ display: flex; justify-content: space-between; margin-bottom: 10px; border-bottom: 1px dashed #eee; padding-bottom: 5px; }}
                    .total-row {{ display: flex; justify-content: space-between; font-weight: bold; font-size: 1.1em; margin-top: 15px; border-top: 2px solid #333; padding-top: 10px; }}
                    .qr-section {{ text-align: center; margin-top: 30px; }}
                    .qr-section img {{ width: 150px; height: 150px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 0.9em; color: #777; }}
                </style>
            </head>
            <body>
                <div class='receipt-container'>
                    <div class='header'>
                        <h2>Eyewa Store</h2>
                        <p>Thank you for your purchase!</p>
                    </div>
                    
                    <div class='item-row'><span>Sales ID:</span> <span>{save.SalesId}</span></div>
                    <div class='item-row'><span>Customer:</span> <span>{save.CustomerName}</span></div>
                    <div class='item-row'><span>Phone:</span> <span>{save.CustomerNo}</span></div>
                    
                    <br/>
                    <div class='item-row'><span>Gross Total:</span> <span>{save.GrossTotal} SAR</span></div>
                    <div class='item-row'><span>Discount:</span> <span>{save.Discount} SAR</span></div>
                    <div class='item-row'><span>VAT:</span> <span>{save.Tax} SAR</span></div>
                    
                    <div class='total-row'><span>Net Total:</span> <span>{save.NetTotal} SAR</span></div>
                    <div class='item-row'><span>Amount Paid:</span> <span>{save.PaidAmount} SAR</span></div>
                    <div class='item-row'><span>Balance:</span> <span>{save.Balance} SAR</span></div>

                    <div class='qr-section'>
                        <img src='cid:qrcodeimg' alt='ZATCA QR Code' />
                    </div>
                    
                    <div class='footer'>
                        Please keep this receipt for your records.
                    </div>
                </div>
            </body>
            </html>";

            return html;
        }

        public static byte[] GenerateImageReceipt(SaveSalesDetails save, string qrBase64)
        {
            // Set up canvas dimensions
            int width = 500;
            int height = 800;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Fill background
                g.Clear(Color.White);

                // Set up fonts and brushes
                using (Font titleFont = new Font("Arial", 22, FontStyle.Bold))
                using (Font subtitleFont = new Font("Arial", 14, FontStyle.Regular))
                using (Font boldFont = new Font("Arial", 16, FontStyle.Bold))
                using (Font regularFont = new Font("Arial", 16, FontStyle.Regular))
                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                using (SolidBrush lightBrush = new SolidBrush(Color.DarkGray))
                using (Pen dashedPen = new Pen(Color.LightGray, 2) { DashStyle = DashStyle.Dash })
                {
                    // Enable high quality rendering
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    int currentY = 30;
                    int marginX = 40;
                    int rightAlignX = width - marginX;

                    // Header
                    StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
                    StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };

                    g.DrawString("Eyewa Store", titleFont, textBrush, new RectangleF(0, currentY, width, 50), centerFormat);
                    currentY += 40;
                    g.DrawString("Thank you for your purchase!", subtitleFont, lightBrush, new RectangleF(0, currentY, width, 30), centerFormat);
                    currentY += 50;

                    // Divider
                    g.DrawLine(dashedPen, marginX, currentY, rightAlignX, currentY);
                    currentY += 20;

                    // Customer Info
                    g.DrawString("Sales ID:", regularFont, textBrush, marginX, currentY);
                    g.DrawString(save.SalesId.ToString(), boldFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 35;

                    g.DrawString("Customer:", regularFont, textBrush, marginX, currentY);
                    g.DrawString(save.CustomerName, boldFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 35;

                    g.DrawString("Phone:", regularFont, textBrush, marginX, currentY);
                    g.DrawString(save.CustomerNo, boldFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 45;

                    // Divider
                    g.DrawLine(dashedPen, marginX, currentY, rightAlignX, currentY);
                    currentY += 20;

                    // Amounts
                    g.DrawString("Gross Total:", regularFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.GrossTotal} SAR", regularFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 35;

                    g.DrawString("Discount:", regularFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.Discount} SAR", regularFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 35;

                    g.DrawString("VAT:", regularFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.Tax} SAR", regularFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 45;

                    // Solid Divider
                    g.DrawLine(new Pen(Color.Black, 2), marginX, currentY, rightAlignX, currentY);
                    currentY += 20;

                    // Net Total
                    g.DrawString("Net Total:", boldFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.NetTotal} SAR", boldFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 40;

                    g.DrawString("Amount Paid:", regularFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.PaidAmount} SAR", regularFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 35;

                    g.DrawString("Balance:", boldFont, textBrush, marginX, currentY);
                    g.DrawString($"{save.Balance} SAR", boldFont, textBrush, rightAlignX, currentY, rightFormat);
                    currentY += 50;

                    // Draw QR Code
                    if (!string.IsNullOrEmpty(qrBase64))
                    {
                        try
                        {
                            string cleanBase64 = qrBase64;
                            if (cleanBase64.Contains(","))
                            {
                                cleanBase64 = cleanBase64.Substring(cleanBase64.IndexOf(",") + 1);
                            }

                            byte[] qrBytes = Convert.FromBase64String(cleanBase64);
                            using (MemoryStream ms = new MemoryStream(qrBytes))
                            using (Image qrImage = Image.FromStream(ms))
                            {
                                // Draw QR Code in the center
                                int qrSize = 200;
                                int qrX = (width - qrSize) / 2;
                                g.DrawImage(qrImage, qrX, currentY, qrSize, qrSize);
                            }
                        }
                        catch
                        {
                            // Ignore if QR parsing fails
                        }
                    }

                    // Convert to byte array
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
