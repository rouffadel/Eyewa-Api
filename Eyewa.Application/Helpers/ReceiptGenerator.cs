using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Helpers
{
    public static class ReceiptGenerator
    {
        public class ReceiptItemLine
        {
            public string Category { get; set; } = "—";
            public string Brand { get; set; } = "—";
            public string Model { get; set; } = "—";
            public double Price { get; set; }
            public int Quantity { get; set; } = 1;
            public double Tax { get; set; }
            public double Discount { get; set; }
            public double Total { get; set; }
        }

        public static string GenerateHtmlReceipt(SaveSalesDetails save, string qrBase64, object dbResult = null)
        {
            var data = ParseReceiptData(save, dbResult);

            string itemRowsHtml = "";
            foreach (var item in data.Items)
            {
                itemRowsHtml += $@"
                <tr>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Category}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Brand}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Model}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Price:0.00}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Quantity}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Tax:0.00}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Discount:0.00}</td>
                    <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{item.Total:0.00}</td>
                </tr>";
            }

            string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .receipt-container {{ width: 650px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; margin-bottom: 20px; }}
                    .header h2 {{ margin: 0; color: #cfab3a; }}
                    .meta-table {{ width: 100%; margin-bottom: 15px; font-size: 14px; font-weight: bold; }}
                    .data-table {{ width: 100%; border-collapse: collapse; margin-bottom: 15px; font-size: 13px; }}
                    .data-table th {{ background-color: #f0f0f0; border: 1px solid #ccc; padding: 8px; text-align: center; }}
                    .total-row {{ font-weight: bold; font-size: 1.1em; margin-top: 15px; border-top: 2px solid #333; padding-top: 10px; }}
                    .qr-section {{ text-align: center; margin-top: 20px; }}
                    .qr-section img {{ width: 135px; height: 135px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 0.85em; color: #555; background: #1a1a1a; color: #fff; padding: 10px; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='receipt-container'>
                    <div class='header'>
                        <h2>Naimat Al Basar Optical</h2>
                        <p>Simplified Tax Invoice - فاتورة ضريبية مبسطة</p>
                    </div>
                    
                    <table class='meta-table'>
                        <tr>
                            <td>Inv No : {data.InvoiceNo}</td>
                            <td style='text-align:right;'>Name : {data.CustomerName}</td>
                        </tr>
                        <tr>
                            <td>Date : {data.InvoiceDate}</td>
                            <td style='text-align:right;'>Number : {data.CustomerNo}</td>
                        </tr>
                    </table>

                    <table class='data-table'>
                        <thead>
                            <tr>
                                <th>Category</th>
                                <th>Brand</th>
                                <th>Model</th>
                                <th>Price</th>
                                <th>Qty</th>
                                <th>VAT</th>
                                <th>Discount</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemRowsHtml}
                        </tbody>
                    </table>

                    <table class='data-table'>
                        <thead>
                            <tr>
                                <th>Prescription Details</th>
                                <th>SPH</th>
                                <th>CYL</th>
                                <th>AXIS</th>
                                <th>ADD</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td style='padding:6px; border:1px solid #ccc;'>Right Eye</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.SphRight}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.CylRight}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.AxisRight}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.AddRight}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px; border:1px solid #ccc;'>Left Eye</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.SphLeft}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.CylLeft}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.AxisLeft}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.AddLeft}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px; border:1px solid #ccc;'>IPD</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.Ipd}</td>
                                <td style='padding:6px; border:1px solid #ccc;' colspan='3'></td>
                            </tr>
                        </tbody>
                    </table>

                    <div style='display:flex; justify-content:space-between; align-items:center;'>
                        <div class='qr-section'>
                            <img src='cid:qrcodeimg' alt='ZATCA QR Code' />
                        </div>
                        <table style='width:320px; border-collapse:collapse; font-size:13px; font-weight:bold;'>
                            <tr>
                                <td style='padding:6px; background:#f0f0f0; border:1px solid #ccc;'>Details:</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:right;'>Total Amount:</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.NetTotal:0.00}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px; background:#f0f0f0; border:1px solid #ccc;'>Payment Mode</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:right;'>Amount Paid:</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.PaidAmount:0.00}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px; border:1px solid #ccc;'>{data.PaymentMode}</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:right;'>Total VAT:</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center;'>{data.Tax:0.00}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px; border:1px solid #ccc;'></td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:right;'>Balance:</td>
                                <td style='padding:6px; border:1px solid #ccc; text-align:center; color:red;'>{data.Balance:0.00}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <div class='footer'>
                        Thank you for your purchase!
                    </div>
                </div>
            </body>
            </html>";

            return html;
        }

        public static byte[] GenerateImageReceipt(SaveSalesDetails save, string qrBase64, object dbResult = null)
        {
            var data = ParseReceiptData(save, dbResult);

            int width = 500;
            int height = 720;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                using (SolidBrush grayBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
                using (Pen dashedPen = new Pen(Color.FromArgb(200, 200, 200), 1.5f) { DashStyle = DashStyle.Dash })
                using (Pen solidPen = new Pen(Color.Black, 2f))
                {
                    // --- 1. Header ---
                    using (Font titleFont = new Font("Arial", 22f, FontStyle.Bold))
                    using (Font subTitleFont = new Font("Arial", 13f, FontStyle.Regular))
                    {
                        StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString("Eyewa Store", titleFont, textBrush, new RectangleF(0, 30, width, 40), centerFormat);
                        g.DrawString("Thank you for your purchase!", subTitleFont, grayBrush, new RectangleF(0, 75, width, 30), centerFormat);
                    }

                    // Dashed Line 1
                    g.DrawLine(dashedPen, 35, 120, width - 35, 120);

                    // --- 2. Customer & Order Details ---
                    using (Font labelFont = new Font("Arial", 13f, FontStyle.Regular))
                    using (Font valueFont = new Font("Arial", 13f, FontStyle.Bold))
                    {
                        int y = 138;
                        int leftX = 40;
                        int rightX = width - 40;
                        int rowHeight = 36;
                        StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };

                        // Row 1: Sales ID
                        g.DrawString("Sales ID:", labelFont, textBrush, leftX, y);
                        g.DrawString(data.InvoiceNo, valueFont, textBrush, rightX, y, rightFormat);

                        // Row 2: Customer
                        y += rowHeight;
                        g.DrawString("Customer:", labelFont, textBrush, leftX, y);
                        g.DrawString(data.CustomerName, valueFont, textBrush, rightX, y, rightFormat);

                        // Row 3: Phone
                        y += rowHeight;
                        g.DrawString("Phone:", labelFont, textBrush, leftX, y);
                        g.DrawString(data.CustomerNo, valueFont, textBrush, rightX, y, rightFormat);

                        // Dashed Line 2
                        y += rowHeight + 10;
                        g.DrawLine(dashedPen, 35, y, width - 35, y);

                        // --- 3. Subtotals Breakdown ---
                        y += 20;

                        // Row 1: Gross Total
                        g.DrawString("Gross Total:", labelFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar(data.GrossTotal), labelFont, textBrush, rightX, y, rightFormat);

                        // Row 2: Discount
                        y += rowHeight;
                        g.DrawString("Discount:", labelFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar(data.Discount), labelFont, textBrush, rightX, y, rightFormat);

                        // Row 3: VAT
                        y += rowHeight;
                        g.DrawString("VAT:", labelFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar(data.Tax), labelFont, textBrush, rightX, y, rightFormat);

                        // Solid Divider Line
                        y += rowHeight + 10;
                        g.DrawLine(solidPen, 35, y, width - 35, y);

                        // --- 4. Main Totals ---
                        y += 20;

                        // Row 1: Net Total
                        g.DrawString("Net Total:", valueFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar2Decimals(data.NetTotal), valueFont, textBrush, rightX, y, rightFormat);

                        // Row 2: Amount Paid
                        y += rowHeight;
                        g.DrawString("Amount Paid:", labelFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar(data.PaidAmount), labelFont, textBrush, rightX, y, rightFormat);

                        // Row 3: Balance
                        y += rowHeight;
                        g.DrawString("Balance:", valueFont, textBrush, leftX, y);
                        g.DrawString(FormatMoneySar2Decimals(data.Balance), valueFont, textBrush, rightX, y, rightFormat);

                        // --- 5. QR Code ---
                        y += rowHeight + 25;
                        int qrSize = 175;
                        int qrX = (width - qrSize) / 2;

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
                                    g.DrawImage(qrImage, qrX, y, qrSize, qrSize);
                                }
                            }
                            catch
                            {
                                g.DrawRectangle(Pens.Black, qrX, y, qrSize, qrSize);
                            }
                        }
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }

        private class ParsedReceiptData
        {
            public string InvoiceNo { get; set; } = "";
            public string InvoiceDate { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public string CustomerNo { get; set; } = "";

            public List<ReceiptItemLine> Items { get; set; } = new List<ReceiptItemLine>();

            public string SphRight { get; set; } = "0.00";
            public string CylRight { get; set; } = "0.00";
            public string AxisRight { get; set; } = "0";
            public string AddRight { get; set; } = "0.00";

            public string SphLeft { get; set; } = "0.00";
            public string CylLeft { get; set; } = "0.00";
            public string AxisLeft { get; set; } = "0";
            public string AddLeft { get; set; } = "0.00";

            public string Ipd { get; set; } = "—";

            public double GrossTotal { get; set; }
            public double Discount { get; set; }
            public double Tax { get; set; }
            public double NetTotal { get; set; }
            public double PaidAmount { get; set; }
            public double Balance { get; set; }
            public string PaymentMode { get; set; } = "Cash";
        }

        private static ParsedReceiptData ParseReceiptData(SaveSalesDetails save, object dbResult)
        {
            var data = new ParsedReceiptData();
            var mainRow = GetFirstRow(dbResult, "table");
            var paidRow = GetFirstRow(dbResult, "table2");

            data.InvoiceNo = GetDictValue(mainRow, "InvoiceNo", "invoiceNo")?.ToString();
            if (string.IsNullOrWhiteSpace(data.InvoiceNo))
            {
                data.InvoiceNo = $"2020-{DateTime.Now:ddMMyyyy}-{save?.SalesId}";
            }

            data.CustomerName = GetDictValue(mainRow, "CustomerName", "customerName")?.ToString() ?? save?.CustomerName ?? "—";
            data.CustomerNo = GetDictValue(mainRow, "CustomerNo", "customerNo")?.ToString() ?? save?.CustomerNo ?? "—";

            string rawDate = GetDictValue(mainRow, "InvoiceDate", "invoiceDate")?.ToString();
            data.InvoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (!string.IsNullOrWhiteSpace(rawDate) && DateTime.TryParse(rawDate, out DateTime parsedDate))
            {
                data.InvoiceDate = parsedDate.ToString("yyyy-MM-dd");
            }

            // Extract item lines
            var table1Rows = GetRows(dbResult, "table1");
            foreach (var row in table1Rows)
            {
                string category = GetDictValue(row, "CategoryName", "Category")?.ToString();
                string brand = GetDictValue(row, "BrandName", "Brand")?.ToString();
                string product = GetDictValue(row, "ProductName", "Product")?.ToString();
                string model = GetDictValue(row, "ModelNo", "Model")?.ToString();

                double.TryParse(GetDictValue(row, "SellingPrice", "ProductValue")?.ToString(), out double price);
                int.TryParse(GetDictValue(row, "Quantity")?.ToString(), out int qty);
                if (qty <= 0) qty = 1;
                double.TryParse(GetDictValue(row, "Tax")?.ToString(), out double tax);
                double.TryParse(GetDictValue(row, "Discount")?.ToString(), out double discount);

                double total = (price * qty) - discount + tax;

                string brandDisplay = !string.IsNullOrWhiteSpace(brand)
                    ? (!string.IsNullOrWhiteSpace(product) && !product.StartsWith(brand, StringComparison.OrdinalIgnoreCase) ? $"{brand} {product}" : brand)
                    : (product ?? "—");

                data.Items.Add(new ReceiptItemLine
                {
                    Category = string.IsNullOrWhiteSpace(category) ? "—" : category,
                    Brand = string.IsNullOrWhiteSpace(brandDisplay) ? "—" : brandDisplay,
                    Model = string.IsNullOrWhiteSpace(model) ? "—" : model,
                    Price = price,
                    Quantity = qty,
                    Tax = tax,
                    Discount = discount,
                    Total = total
                });
            }

            var table3Rows = GetRows(dbResult, "table3");
            foreach (var row in table3Rows)
            {
                string category = GetDictValue(row, "Category", "CategoryName")?.ToString() ?? "CR39";
                string orderLense = GetDictValue(row, "Orderlense", "OrderLense", "Brand")?.ToString() ?? "—";
                double.TryParse(GetDictValue(row, "Price")?.ToString(), out double price);
                int.TryParse(GetDictValue(row, "Quantity")?.ToString(), out int qty);
                if (qty <= 0) qty = 1;
                double.TryParse(GetDictValue(row, "Total")?.ToString(), out double total);
                if (total == 0) total = price * qty;

                data.Items.Add(new ReceiptItemLine
                {
                    Category = string.IsNullOrWhiteSpace(category) ? "CR39" : category,
                    Brand = string.IsNullOrWhiteSpace(orderLense) ? "—" : orderLense,
                    Model = "—",
                    Price = price,
                    Quantity = qty,
                    Tax = 0,
                    Discount = 0,
                    Total = total
                });
            }

            if (data.Items.Count == 0 && save?.SalesGrids != null)
            {
                foreach (var grid in save.SalesGrids)
                {
                    double.TryParse(grid.SellingPrice, out double price);
                    int.TryParse(grid.Quantity, out int qty);
                    if (qty <= 0) qty = 1;
                    double.TryParse(grid.Discount, out double discount);
                    double.TryParse(grid.Tax, out double tax);
                    double total = (price * qty) - discount + tax;

                    data.Items.Add(new ReceiptItemLine
                    {
                        Category = "—",
                        Brand = $"Product #{grid.ProductId}",
                        Model = "—",
                        Price = price,
                        Quantity = qty,
                        Tax = tax,
                        Discount = discount,
                        Total = total
                    });
                }
            }

            // Extract Prescription Details
            data.SphRight = FormatDiopter(GetDictValue(mainRow, "SPH_RightEye", "spH_RightEye"));
            data.CylRight = FormatDiopter(GetDictValue(mainRow, "CYL_RightEye", "cyL_RightEye"));
            data.AxisRight = FormatAxis(GetDictValue(mainRow, "AXIS_RightEye", "axiS_RightEye"));
            data.AddRight = FormatDiopter(GetDictValue(mainRow, "ADD_RightEye", "adD_RightEye"));

            data.SphLeft = FormatDiopter(GetDictValue(mainRow, "SPH_LeftEye", "spH_LeftEye"));
            data.CylLeft = FormatDiopter(GetDictValue(mainRow, "CYL_LeftEye", "cyL_LeftEye"));
            data.AxisLeft = FormatAxis(GetDictValue(mainRow, "AXIS_LeftEye", "axiS_LeftEye"));
            data.AddLeft = FormatDiopter(GetDictValue(mainRow, "ADD_LeftEye", "adD_LeftEye"));

            data.Ipd = FormatIpd(GetDictValue(mainRow, "SPH_IPD", "spH_IPD"));

            // Extract Totals
            double.TryParse(GetDictValue(mainRow, "GrossTotal")?.ToString() ?? save?.GrossTotal, out double grossTotalVal);
            double.TryParse(GetDictValue(mainRow, "Discount")?.ToString() ?? save?.Discount, out double discountVal);
            double.TryParse(GetDictValue(mainRow, "TotalTax")?.ToString() ?? save?.Tax, out double taxVal);
            double.TryParse(GetDictValue(mainRow, "NetTotal")?.ToString() ?? save?.NetTotal, out double netTotalVal);

            if (netTotalVal == 0 && data.Items.Count > 0)
            {
                netTotalVal = data.Items.Sum(x => x.Total);
            }
            if (grossTotalVal == 0)
            {
                grossTotalVal = netTotalVal + discountVal - taxVal;
            }

            object paidObj = GetDictValue(paidRow, "PaidAmount", "paidAmount") ?? save?.PaidAmount;
            double.TryParse(paidObj?.ToString(), out double paidAmountVal);

            object balanceObj = GetDictValue(mainRow, "Balance", "balance") ?? save?.Balance;
            double.TryParse(balanceObj?.ToString(), out double balanceVal);

            if (paidAmountVal == 0 && balanceVal == 0 && netTotalVal > 0)
            {
                paidAmountVal = netTotalVal;
            }

            data.GrossTotal = grossTotalVal;
            data.Discount = discountVal;
            data.Tax = taxVal;
            data.NetTotal = netTotalVal;
            data.PaidAmount = paidAmountVal;
            data.Balance = balanceVal;
            data.PaymentMode = string.IsNullOrWhiteSpace(save?.PaymentMode) ? "Cash" : save.PaymentMode;

            return data;
        }

        private static Dictionary<string, object> GetFirstRow(object dbResult, string tableName = "table")
        {
            if (dbResult is Dictionary<string, List<Dictionary<string, object>>> dict)
            {
                if (dict.TryGetValue(tableName, out var list) && list != null && list.Count > 0)
                {
                    return list[0];
                }
            }
            else if (dbResult is List<Dictionary<string, object>> listRows && listRows.Count > 0)
            {
                return listRows[0];
            }
            return null;
        }

        private static List<Dictionary<string, object>> GetRows(object dbResult, string tableName)
        {
            if (dbResult is Dictionary<string, List<Dictionary<string, object>>> dict)
            {
                if (dict.TryGetValue(tableName, out var list) && list != null)
                {
                    return list;
                }
            }
            return new List<Dictionary<string, object>>();
        }

        private static object GetDictValue(Dictionary<string, object> dict, params string[] keys)
        {
            if (dict == null) return null;
            foreach (var key in keys)
            {
                foreach (var kvp in dict)
                {
                    if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;
                    }
                }
            }
            return null;
        }

        private static string FormatDiopter(object val)
        {
            if (val == null) return "0.00";
            string s = val.ToString().Trim();
            if (string.IsNullOrEmpty(s) || s == "—") return "0.00";

            if (s.StartsWith("+") || s.StartsWith("-"))
            {
                if (double.TryParse(s, out double d1))
                {
                    return d1 > 0 ? "+" + d1.ToString("0.00") : d1.ToString("0.00");
                }
                return s;
            }

            if (double.TryParse(s, out double d2))
            {
                return (d2 > 0) ? "+" + d2.ToString("0.00") : d2.ToString("0.00");
            }
            return s;
        }

        private static string FormatAxis(object val)
        {
            if (val == null) return "0";
            string s = val.ToString().Trim();
            if (string.IsNullOrEmpty(s) || s == "—") return "0";
            if (double.TryParse(s, out double d))
            {
                return ((int)d).ToString();
            }
            return s;
        }

        private static string FormatIpd(object val)
        {
            if (val == null) return "—";
            string s = val.ToString().Trim();
            if (string.IsNullOrEmpty(s) || s == "—") return "—";
            if (s.EndsWith("mc", StringComparison.OrdinalIgnoreCase)) return s;
            return s + " mc";
        }

        private static string FormatMoneySar(double value)
        {
            if (Math.Abs(value - Math.Floor(value)) < 0.001)
            {
                return $"{((long)value)} SAR";
            }
            return $"{value:0.##} SAR";
        }

        private static string FormatMoneySar2Decimals(double value)
        {
            return $"{value:0.00} SAR";
        }
    }
}
