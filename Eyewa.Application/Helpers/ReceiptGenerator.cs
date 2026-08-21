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
            // Parse decimal values safely
            double.TryParse(save.GrossTotal, out double grossTotalVal);
            double.TryParse(save.Discount, out double discountVal);
            double.TryParse(save.Tax, out double taxVal);
            double.TryParse(save.NetTotal, out double netTotalVal);
            double.TryParse(save.PaidAmount, out double paidAmountVal);
            double.TryParse(save.Balance, out double balanceVal);

            // Construct invoice number dynamically
            string invoiceNo = $"2020-{DateTime.Now.ToString("ddMMyyyy")}-{save.SalesId}";

            // Square Canvas: 750px width x 750px height
            int width = 750;
            int height = 750;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // --- 1. Header (110px height) ---
                Color goldColor = Color.FromArgb(207, 171, 58);
                Color darkColor = Color.FromArgb(26, 26, 26);

                using (SolidBrush goldBrush = new SolidBrush(goldColor))
                using (SolidBrush darkBrush = new SolidBrush(darkColor))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                using (Pen goldPen = new Pen(goldColor, 2))
                {
                    // Draw backgrounds
                    g.FillRectangle(goldBrush, 0, 0, 240, 110);
                    g.FillRectangle(darkBrush, 240, 0, width - 240, 110);

                    // Left Block Contact Info
                    using (Font contactFont = new Font("Arial", 9.5f, FontStyle.Bold))
                    {
                        g.DrawString("📞  013 5883617", contactFont, whiteBrush, 15, 30);
                        g.DrawString("✉  nb.optical@hotmail.com", contactFont, whiteBrush, 15, 60);
                    }

                    // Middle Logo Block (inside dark background)
                    g.DrawEllipse(goldPen, 265, 30, 26, 26);
                    g.DrawEllipse(goldPen, 300, 30, 26, 26);
                    g.DrawLine(goldPen, 291, 43, 300, 43);
                    using (Font logoNameFont = new Font("Arial", 11f, FontStyle.Bold))
                    {
                        g.DrawString("NB", logoNameFont, goldBrush, 281, 10);
                    }
                    using (Font subLogoFont = new Font("Arial", 4.5f, FontStyle.Bold))
                    {
                        g.DrawString("NAIMAT AL BASAR OPTICAL", subLogoFont, goldBrush, 255, 62);
                    }

                    // Right Text Block
                    using (Font arabicTitleFont = new Font("Arial", 18f, FontStyle.Bold))
                    using (Font englishTitleFont = new Font("Arial", 9f, FontStyle.Bold))
                    using (Font vatFont = new Font("Arial", 8.5f, FontStyle.Bold))
                    using (Font typeFont = new Font("Arial", 7.5f, FontStyle.Bold))
                    {
                        StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };
                        g.DrawString("نظارات نعمة البصر", arabicTitleFont, whiteBrush, width - 15, 12, rightFormat);
                        g.DrawString("NAIMAT AL BASAR OPTICAL", englishTitleFont, goldBrush, width - 15, 45, rightFormat);
                        g.DrawString("الرقم المميز : ٣١٠٢٥٤٦٥٩٧٠٠٠٠٣", vatFont, whiteBrush, width - 15, 64, rightFormat);
                        g.DrawString("Simplified Tax Invoice   فاتورة ضريبية مبسطة", typeFont, goldBrush, width - 15, 82, rightFormat);
                    }
                }

                // --- 2. Metadata Section (110px to 175px) ---
                int currentY = 110;
                using (Pen blackPen = new Pen(Color.Black, 2.5f))
                using (Font metaFont = new Font("Arial", 11f, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString($"Inv No   :   {invoiceNo}", metaFont, textBrush, 15, currentY + 10);
                    g.DrawString($"Name    :   {save.CustomerName}", metaFont, textBrush, 400, currentY + 10);
                    
                    g.DrawString($"Date      :   {DateTime.Now.ToString("yyyy-MM-dd")}", metaFont, textBrush, 15, currentY + 36);
                    g.DrawString($"Number :   {save.CustomerNo}", metaFont, textBrush, 400, currentY + 36);

                    currentY += 68;
                    g.DrawLine(blackPen, 0, currentY, width, currentY);
                }

                // --- 3. Products Table ---
                currentY += 8;
                using (Pen doublePen = new Pen(Color.Black, 1.5f))
                using (Font tableHeaderFont = new Font("Arial", 10.5f, FontStyle.Bold))
                using (Font tableCellFont = new Font("Arial", 10.5f, FontStyle.Bold))
                using (SolidBrush headerBg = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawRectangle(doublePen, 15, currentY, width - 30, 110);
                    g.DrawRectangle(doublePen, 17, currentY + 2, width - 34, 106);

                    int[] colWidths = { 90, 110, 100, 80, 60, 60, 80, 140 };
                    int startX = 15;

                    for (int i = 0; i < colWidths.Length; i++)
                    {
                        g.FillRectangle(headerBg, startX, currentY + 4, colWidths[i], 32);
                        g.DrawRectangle(Pens.Black, startX, currentY + 4, colWidths[i], 32);
                        startX += colWidths[i];
                    }

                    string[] headers = { "Category", "Brand", "Model", "Price", "Qty", "VAT", "Discount", "Total" };
                    startX = 15;
                    StringFormat cellFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        g.DrawString(headers[i], tableHeaderFont, Brushes.Black, new RectangleF(startX, currentY + 4, colWidths[i], 32), cellFormat);
                        startX += colWidths[i];
                    }

                    int rowY = currentY + 36;
                    
                    // Row 1
                    g.DrawRectangle(Pens.Black, 15, rowY, colWidths[0], 32);
                    g.DrawString("CR39", tableCellFont, Brushes.Black, new RectangleF(15, rowY, colWidths[0], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0], rowY, colWidths[1], 32);
                    g.DrawString("privo bc", tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0], rowY, colWidths[1], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1], rowY, colWidths[2], 32);
                    g.DrawString("", tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1], rowY, colWidths[2], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1] + colWidths[2], rowY, colWidths[3], 32);
                    g.DrawString((grossTotalVal / 2).ToString("0.00"), tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1] + colWidths[2], rowY, colWidths[3], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], rowY, colWidths[4], 32);
                    g.DrawString("2", tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], rowY, colWidths[4], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], rowY, colWidths[5], 32);
                    g.DrawString("0", tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], rowY, colWidths[5], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5], rowY, colWidths[6], 32);
                    g.DrawString("0", tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5], rowY, colWidths[6], 32), cellFormat);

                    g.DrawRectangle(Pens.Black, 15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5] + colWidths[6], rowY, colWidths[7], 32);
                    g.DrawString(grossTotalVal.ToString("0.00"), tableCellFont, Brushes.Black, new RectangleF(15 + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5] + colWidths[6], rowY, colWidths[7], 32), cellFormat);

                    // Row 2 Spacer
                    rowY += 32;
                    for (int i = 0; i < colWidths.Length; i++)
                    {
                        g.DrawRectangle(Pens.Black, 15 + (i > 0 ? SumWidths(colWidths, i) : 0), rowY, colWidths[i], 32);
                    }

                    currentY += 112;
                }

                // --- 4. Prescription Details ---
                currentY += 10;
                using (Pen doublePen = new Pen(Color.Black, 1.5f))
                using (Font tableHeaderFont = new Font("Arial", 10.5f, FontStyle.Bold))
                using (Font tableCellFont = new Font("Arial", 10.5f, FontStyle.Bold))
                using (SolidBrush headerBg = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawRectangle(doublePen, 15, currentY, width - 30, 130);
                    g.DrawRectangle(doublePen, 17, currentY + 2, width - 34, 126);

                    int[] rxColWidths = { 300, 100, 100, 100, 120 };
                    int rxStartY = currentY + 4;
                    StringFormat cellFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    string[] rxHeaders = { "Prescription Details", "SPH", "CYL", "AXIS", "ADD" };
                    int currentX = 15;
                    for (int i = 0; i < rxHeaders.Length; i++)
                    {
                        g.FillRectangle(headerBg, currentX, rxStartY, rxColWidths[i], 30);
                        g.DrawRectangle(Pens.Black, currentX, rxStartY, rxColWidths[i], 30);
                        g.DrawString(rxHeaders[i], tableHeaderFont, Brushes.Black, new RectangleF(currentX, rxStartY, rxColWidths[i], 30), cellFormat);
                        currentX += rxColWidths[i];
                    }

                    // Row 1: Right Eye
                    rxStartY += 30;
                    currentX = 15;
                    string[] rightEyeData = { "Right Eye", "0.00", "0.00", "0", "1.50" };
                    for (int i = 0; i < rightEyeData.Length; i++)
                    {
                        g.DrawRectangle(Pens.Black, currentX, rxStartY, rxColWidths[i], 30);
                        g.DrawString(rightEyeData[i], tableCellFont, Brushes.Black, new RectangleF(currentX, rxStartY, rxColWidths[i], 30), (i == 0) ? new StringFormat { LineAlignment = StringAlignment.Center } : cellFormat);
                        currentX += rxColWidths[i];
                    }

                    // Row 2: Left Eye
                    rxStartY += 30;
                    currentX = 15;
                    string[] leftEyeData = { "Left Eye", "0.00", "0.00", "0", "1.50" };
                    for (int i = 0; i < leftEyeData.Length; i++)
                    {
                        g.DrawRectangle(Pens.Black, currentX, rxStartY, rxColWidths[i], 30);
                        g.DrawString(leftEyeData[i], tableCellFont, Brushes.Black, new RectangleF(currentX, rxStartY, rxColWidths[i], 30), (i == 0) ? new StringFormat { LineAlignment = StringAlignment.Center } : cellFormat);
                        currentX += rxColWidths[i];
                    }

                    // Row 3: IPD
                    rxStartY += 30;
                    currentX = 15;
                    g.DrawRectangle(Pens.Black, currentX, rxStartY, rxColWidths[0], 30);
                    g.DrawString("IPD", tableCellFont, Brushes.Black, new RectangleF(currentX, rxStartY, rxColWidths[0], 30), new StringFormat { LineAlignment = StringAlignment.Center });
                    
                    g.DrawRectangle(Pens.Black, currentX + rxColWidths[0], rxStartY, rxColWidths[1], 30);
                    g.DrawString("61", tableCellFont, Brushes.Black, new RectangleF(currentX + rxColWidths[0], rxStartY, rxColWidths[1], 30), cellFormat);

                    g.DrawRectangle(Pens.Black, currentX + rxColWidths[0] + rxColWidths[1], rxStartY, rxColWidths[2], 30);
                    g.DrawRectangle(Pens.Black, currentX + rxColWidths[0] + rxColWidths[1] + rxColWidths[2], rxStartY, rxColWidths[3], 30);
                    g.DrawRectangle(Pens.Black, currentX + rxColWidths[0] + rxColWidths[1] + rxColWidths[2] + rxColWidths[3], rxStartY, rxColWidths[4], 30);

                    currentY += 134;
                }

                // --- 5. Footer Row with QR Code and Totals Box ---
                currentY += 10;
                int qrSize = 135;
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
                            g.DrawImage(qrImage, 15, currentY, qrSize, qrSize);
                            g.DrawRectangle(Pens.Black, 15, currentY, qrSize, qrSize);
                        }
                    }
                    catch
                    {
                        g.DrawRectangle(Pens.Black, 15, currentY, qrSize, qrSize);
                    }
                }
                else
                {
                    g.DrawRectangle(Pens.Black, 15, currentY, qrSize, qrSize);
                }

                int totalsTableX = 180;
                int totalsTableWidth = width - totalsTableX - 15;
                using (Pen doublePen = new Pen(Color.Black, 1.5f))
                using (Font totalsFont = new Font("Arial", 10f, FontStyle.Bold))
                using (SolidBrush totalsBg = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawRectangle(doublePen, totalsTableX, currentY, totalsTableWidth, 135);
                    g.DrawRectangle(doublePen, totalsTableX + 2, currentY + 2, totalsTableWidth - 4, 131);

                    int colLabelW = 120;
                    int colVal1W = 80;
                    int colTitleW = 180;
                    int colVal2W = 120;

                    int rowY = currentY + 4;
                    StringFormat rightAlignFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    // Row 1
                    g.FillRectangle(totalsBg, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawString("Details:", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4, rowY, colLabelW, 30), centerFormat);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW, rowY, colVal1W, 30);

                    g.FillRectangle(totalsBg, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawString("Total Amount:", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30), rightAlignFormat);

                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30);
                    g.DrawString(grossTotalVal.ToString("0.00"), totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30), centerFormat);

                    // Row 2
                    rowY += 30;
                    g.FillRectangle(totalsBg, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawString("Payment Mode", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4, rowY, colLabelW, 30), centerFormat);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW, rowY, colVal1W, 30);
                    g.DrawString(string.IsNullOrEmpty(save.PaymentMode) ? "Cash" : save.PaymentMode, totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW, rowY, colVal1W, 30), centerFormat);

                    g.FillRectangle(totalsBg, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawString("Amount Paid:", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30), rightAlignFormat);

                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30);
                    g.DrawString(paidAmountVal.ToString("0.00"), totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30), centerFormat);

                    // Row 3
                    rowY += 30;
                    g.DrawRectangle(Pens.Black, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW, rowY, colVal1W, 30);

                    g.FillRectangle(totalsBg, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawString("Total VAT:", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30), rightAlignFormat);

                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30);
                    g.DrawString(taxVal.ToString("0.00"), totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30), centerFormat);

                    // Row 4
                    rowY += 30;
                    g.DrawRectangle(Pens.Black, totalsTableX + 4, rowY, colLabelW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW, rowY, colVal1W, 30);

                    g.FillRectangle(totalsBg, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30);
                    g.DrawString("Balance:", totalsFont, Brushes.Black, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W, rowY, colTitleW, 30), rightAlignFormat);

                    g.DrawRectangle(Pens.Black, totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30);
                    using (SolidBrush redBrush = new SolidBrush(Color.Red))
                    {
                        g.DrawString(balanceVal.ToString("0.00"), totalsFont, redBrush, new RectangleF(totalsTableX + 4 + colLabelW + colVal1W + colTitleW, rowY, colVal2W, 30), centerFormat);
                    }
                }

                // --- 6. Bottom Notes & Address Banner ---
                currentY = 655;
                using (SolidBrush darkBrush = new SolidBrush(darkColor))
                using (SolidBrush goldBrush = new SolidBrush(goldColor))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(darkBrush, 0, currentY, width, height - currentY);
                    g.DrawLine(new Pen(goldColor, 2f), 0, currentY, width, currentY);

                    StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
                    
                    using (Font notesArFont = new Font("Arial", 9.5f, FontStyle.Bold))
                    using (Font notesEnFont = new Font("Arial", 8f, FontStyle.Regular))
                    {
                        g.DrawString("ملاحظة : سياسة الإرجاع (في نفس اليوم) والاستبدال (خلال ٣ أيام) - (النظارات الشمسية فقط)", notesArFont, goldBrush, new RectangleF(0, currentY + 8, width, 20), centerFormat);
                        g.DrawString("Note : Return Policy (Same Day) & Exchange (with in 3 days)-( ONLY SUNGLASSES )", notesEnFont, whiteBrush, new RectangleF(0, currentY + 28, width, 18), centerFormat);
                    }

                    g.DrawLine(new Pen(Color.FromArgb(80, 80, 80), 1), 50, currentY + 48, width - 50, currentY + 48);

                    using (Font addrArFont = new Font("Arial", 9f, FontStyle.Bold))
                    using (Font addrEnFont = new Font("Arial", 8f, FontStyle.Regular))
                    {
                        g.DrawString("الأحساء - الهفوف - الخالدية - خلف مستوصف المحيش - مقابل جامعة الملك فيصل", addrArFont, whiteBrush, new RectangleF(0, currentY + 54, width, 18), centerFormat);
                        g.DrawString("Al-Ahsa Al Hofuf - Khalediyah - Behind Al-Muhaish Clinic - Opp TO King Faisal University", addrEnFont, new SolidBrush(Color.FromArgb(200, 200, 200)), new RectangleF(0, currentY + 72, width, 16), centerFormat);
                    }
                }

                // Output as PNG bytes
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        private static int SumWidths(int[] widths, int count)
        {
            int sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += widths[i];
            }
            return sum;
        }
    }
}
