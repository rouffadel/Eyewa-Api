using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Eyewa_new_api.Models
{
    public class Common
    {
        #region Method for logs
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        public static void InfoLogs(string Message)
        {
            string fullpath = "";
            try
            {
                var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                var configuration = builder.Build();
                bool log = Convert.ToBoolean(configuration["AppSettings:Log"]);
                if (log == true)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory + "Logs";
                    if (!Directory.Exists(appPath))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(appPath);
                    }
                    string Filename = "APPServiceLog" + DateTime.Now.ToString("dd-MM-yyyy"); //dateAndTime.ToString("dd/MM/yyyy")
                    fullpath = appPath + "\\" + Filename + ".txt";

                    if (!File.Exists(fullpath))
                    {
                        using (System.IO.FileStream f = System.IO.File.Create(fullpath))
                        {
                            f.Close();
                        }
                        TextWriter tw = new StreamWriter(fullpath);
                        tw.WriteLine(DateTime.Now + " " + Message);
                        tw.Close();
                    }
                    else if (File.Exists(fullpath))
                    {
                        using (StreamWriter w = File.AppendText(fullpath))
                        {
                            w.WriteLine(DateTime.Now + " " + Message);
                            w.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText(fullpath))
                {
                    w.WriteLine(DateTime.Now + " " + ex.StackTrace.ToString());
                    w.Close();
                }
            }
        }
        #endregion

        #region ResponseForApi
        /// <summary>
        /// response for api
        /// </summary>

        public class TransactResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public object objresult { get; set; }

            public string qrcodeimg { get; set; }
        }

        public class TransactResult1
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public object objresult1 { get; set; }
            public object objresult2 { get; set; }
        }
        #endregion

        public class ForQrCode
        {
            public string StoreName { get; set; }

            public string VatNo { get; set; }

            public string InvoiceNo { get; set; }

            public string InvoiceDate { get; set; }

            public string VatAmount { get; set; }
            public string GrossTotal { get; set; }
        }

        public class SalesCls
        {
            public int StoreId { get; set; }
            public string CustomerName { get; set; }
            public string CustomerNo { get; set; }
            public int LoginId { get; set; }
            public string InvoiceNo { get; set; }
            public string InvoiceDate { get; set; }
        }

        public class OrderLenseCls
        {
            public int OrderLenseId { get; set; }
            public string CategoryId { get; set; }
            public string Brand { get; set; }
            public string Price { get; set; }
            public string Quantity { get; set; }
            public string Total { get; set; }
        }
        public class PrescriptionDetailsCls
        {
            public string sph { get; set; }
            public string cyl { get; set; }
            public string axis { get; set; }
            public string add { get; set; }
        }
        public class PrescriptionIPD
        {
            public string sphtext { get; set; }
            public string cyltext { get; set; }
            public string axistext { get; set; }
            public string addtext { get; set; }
        }
        public class OrderLenseItemsCls
        {
            public List<OrderLenseCls> OrderLenses { get; set; }
            public List<PrescriptionDetailsCls> PrescriptionDetails { get; set; }
            public PrescriptionIPD PrescriptionIpd { get; set; }
            public int SalesId { get; set; }
        }
        public class InvoiceItem
        {
            public string Name { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }

            public decimal LineTotal { get; set; }
            public decimal TaxAmount { get; set; }
        }
        public class SaveSalesDetails
        {
            public int SalesId { get; set; }
            public int LoginId { get; set; }
            public int StoreId { get; set; }
            public List<SalesGrid> SalesGrids { get; set; }
            public string GrossTotal { get; set; }
            public string Discount { get; set; }
            public string Tax { get; set; }
            public string NetTotal { get; set; }
            public string Balance { get; set; }
            public string PaidAmount { get; set; }
            public string AdvancePaidAmount { get; set; }
            public string PaymentMode { get; set; }
            public string CustomerName { get; set; }
            public string CustomerNo { get; set; }
            public string SalesManId { get; set; }
        }
        public class SalesGrid
        {
            public int SalesDetailId { get; set; }
            public int CategoryId { get; set; }
            public int BrandId { get; set; }
            public int ProductId { get; set; }
            public string ProductValue { get; set; }
            public string Quantity { get; set; }

            public string Tax { get; set; }

            public string TaxPer { get; set; }

            public string Discount { get; set; }
            public string SellingPrice { get; set; }
        }
        public class GetSalesCls
        {
            public object SalesDetails { get; set; }
            public object SalesPrint { get; set; }
        }
        public class SearchSalesCls
        {
            public string CustomerName { get; set; }
            public string CustomerNo { get; set; }
            public string InvoiceNo { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string SerialNo { get; set; }
            public int LoginID { get; set; }
            public int StoreID { get; set; }
        }
        public class LoginCls
        {
            public object result { get; set; }
            public bool View { get; set; }
            public bool Add { get; set; }
            public bool Edit { get; set; }
            public bool Delete { get; set; }
        }
    }
}
