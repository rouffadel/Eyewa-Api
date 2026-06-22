using Eyewa_new_api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Eyewa_new_api.Models.Common;

namespace Eyewa_new_api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        public AdminController()
        {
        }

        public static string converttodate(string date)
        {
            string[] strArray = date.Split('-');
            if (strArray[0].Length == 1)
                strArray[0] = "0" + strArray[0];
            date = strArray[1] + "-" + strArray[0] + "-" + strArray[2];
            return date;
        }

        #region VerifyUserLogin
        /// <summary>
        /// method to verify the user login
        /// </summary>
        [Route("VerifyUserLogin")]
        [HttpGet]
        public async Task<IActionResult> VerifyUserLogin(string LoginName, string Password)
        {
            try
            {
                Common.InfoLogs("VerifyUserLogin Method was started,Controller:Admin LoginName=" + LoginName + " Password=" + Password);
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                if (string.IsNullOrEmpty(LoginName))
                {
                    Common.InfoLogs("VerifyUserLogin Method,Controller:Admin Invalid LoginName");
                    tres.Status = "-100";
                    tres.Message = "Login Name is mandatory";
                    tres.objresult = null;
                    return BadRequest(tres);
                }
                else if (string.IsNullOrEmpty(Password))
                {
                    Common.InfoLogs("VerifyUserLogin Method,Controller:Admin Invalid password");
                    tres.Status = "-100";
                    tres.Message = "Password is mandatory";
                    tres.objresult = null;
                    return BadRequest(tres);
                }
                else
                {
                    tres = await business.EVerifyUserLogin(LoginName, Password);
                }
                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("VerifyUserLogin Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("generate-csid")]
        public async Task<TransactResult> GenerateCsid(string otp)
        {
            TransactResult tres = new TransactResult();

            try
            {
                AdminBusiness obj = new AdminBusiness();
                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260402081257.csr";
                string result = await obj.GenerateComplianceCsid(csrPath, otp);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
                tres.objresult = null;
            }

            return tres;
        }

        [HttpPost]
        [Route("Generateuuid")]
        public async Task<TransactResult> Generateuuid(string vat)
        {
            TransactResult tres = new TransactResult();

            try
            {
                AdminBusiness obj = new AdminBusiness();
                string result = await obj.GenerateSerialNumber(vat);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
                tres.objresult = null;
            }

            return tres;
        }

        [HttpPost]
        [Route("GenerateProdCsid")]
        public async Task<TransactResult> GenerateProdCsid(string otp)
        {
            TransactResult tres = new TransactResult();

            try
            {
                AdminBusiness obj = new AdminBusiness();
                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260402081257.csr";
                string result = await obj.GenerateProductionCsid(csrPath);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
                tres.objresult = null;
            }

            return tres;
        }
        #endregion

        #region FillStore
        [Route("FillStore")]
        [HttpGet]
        public async Task<IActionResult> FillStore(int LoginId, int StoreId)
        {
            try
            {
                Common.InfoLogs("FillStore Method was started,Controller:Admin LoginId=" + LoginId + " StoreId=" + StoreId);
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlStore(LoginId, StoreId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillStore Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region InsertSales
        [Route("InsertSales")]
        [HttpPost]
        public async Task<IActionResult> InsertSales([FromBody] SalesCls sales)
        {
            try
            {
                Common.InfoLogs("InsertSales Method was started,Controller:Admin sales=" + JsonConvert.SerializeObject(sales));
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                sales.StoreId = !(sales.StoreId.ToString() != "0") ? 0 : Convert.ToInt32(sales.StoreId);
                sales.CustomerName = !(sales.CustomerName != "") ? "" : sales.CustomerName;
                sales.CustomerNo = !(sales.CustomerNo != "") ? "" : sales.CustomerNo;
                sales.InvoiceNo = !(sales.InvoiceNo != "") ? "" : sales.InvoiceNo;
                sales.InvoiceDate = !(sales.InvoiceDate != "") ? "" : converttodate(sales.InvoiceDate);
                tres = await business.InsertSales(sales);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("InsertSales Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region FillCategory
        [Route("FillCategory")]
        [HttpGet]
        public async Task<IActionResult> FillCategory()
        {
            try
            {
                Common.InfoLogs("FillCategory Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DDLCategory();

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillCategory Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region FillBrand
        [Route("FillBrand")]
        [HttpGet]
        public async Task<IActionResult> FillBrand()
        {
            try
            {
                Common.InfoLogs("FillBrand Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlBrand();

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillBrand Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetBrand
        [Route("GetBrand")]
        [HttpGet]
        public async Task<IActionResult> GetBrand(string BrandName)
        {
            try
            {
                Common.InfoLogs("GetBrand Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetBrandID(BrandName);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetBrand Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetProduct
        [Route("GetProduct")]
        [HttpGet]
        public async Task<IActionResult> GetProduct(int CategoryId, int BrandId, int StoreId, string ProductName)
        {
            try
            {
                Common.InfoLogs("GetProduct Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                if (CategoryId == 0 || BrandId == 0)
                {
                    tres = await business.GetProductCategorybrandID(StoreId, ProductName);
                }
                else
                {
                    tres = await business.GetProductIDandValue(ProductName, CategoryId, BrandId, StoreId);
                }

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetProduct Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetOrderLense
        [Route("GetOrderLense")]
        [HttpGet]
        public async Task<IActionResult> GetOrderLense(int SalesId)
        {
            try
            {
                Common.InfoLogs("GetOrderLense Method was started,Controller:Admin");
                TransactResult1 tres = new TransactResult1();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetOrderLenseGrid(SalesId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetOrderLense Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetPrescriptionDropDowns
        [Route("GetPrescriptionDropDowns")]
        [HttpGet]
        public async Task<IActionResult> GetPrescriptionDropDowns()
        {
            try
            {
                Common.InfoLogs("GetPrescriptionDropDowns Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetPrescriptiondropdowns();

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetPrescriptionDropDowns Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveOrderLense
        [Route("SaveOrderLense")]
        [HttpPost]
        public async Task<IActionResult> SaveOrderLense([FromBody] OrderLenseItemsCls order)
        {
            try
            {
                Common.InfoLogs("SaveOrderLense Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                int SalesId = order.SalesId;
                tres = await business.SaveOrderLenseItems(order, SalesId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("SaveOrderLense Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetUsers
        [Route("GetUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers(int LoginId, int StoreId)
        {
            try
            {
                Common.InfoLogs("GetUsers Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlUser(LoginId, StoreId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetUsers Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Route("test")]
        [HttpPost]
        public async Task<IActionResult> test()
        {
            try
            {
                Common.InfoLogs("test Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("test Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetSalesMan
        [Route("GetSalesMan")]
        [HttpGet]
        public async Task<IActionResult> GetSalesMan(int LoginId, int StoreId)
        {
            try
            {
                Common.InfoLogs("GetSalesMan Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlSalesMan(LoginId, StoreId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesMan Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region SaveSalesDetails
        [Route("SaveSalesDetails")]
        [HttpPost]
        public async Task<IActionResult> SaveSalesDetails([FromBody] SaveSalesDetails save)
        {
            try
            {
                Common.InfoLogs("SaveSalesDetails Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.SaveSalesDetails(save);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("SaveSalesDetails Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetSalesGrid
        [Route("GetSalesGrid")]
        [HttpPost]
        public async Task<IActionResult> GetSalesGrid([FromBody] SearchSalesCls obj)
        {
            try
            {
                Common.InfoLogs("GetSalesGrid Method was started,Controller:Admin ");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetSalesGrid(obj.CustomerName, obj.CustomerNo, obj.InvoiceNo, obj.FromDate, obj.ToDate, obj.SerialNo, obj.LoginID, obj.StoreID);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesGrid Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetInvoiceDetails
        [Route("GetInvoiceDetails")]
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(int SalesId)
        {
            try
            {
                Common.InfoLogs("GetInvoiceDetails Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetInvoiceDetails(SalesId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetInvoiceDetails Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetSalesPrint
        [Route("GetSalesPrint")]
        [HttpGet]
        public async Task<IActionResult> GetSalesPrint(int SalesId)
        {
            try
            {
                Common.InfoLogs("GetSalesPrint Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetPrint(SalesId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesPrint Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetSalesDetailsGrid
        [Route("GetSalesDetailsGrid")]
        [HttpGet]
        public async Task<IActionResult> GetSalesDetailsGrid(int SalesId)
        {
            try
            {
                Common.InfoLogs("GetSalesDetailsGrid Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetZatcaQrBySalesId(SalesId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesDetailsGrid Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetQuantity
        [Route("GetQuantity")]
        [HttpGet]
        public async Task<IActionResult> GetQuantity(int ProductId)
        {
            try
            {
                Common.InfoLogs("GetQuantity Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetQuantity(ProductId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetQuantity Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteSales
        [Route("DeleteSales")]
        [HttpGet]
        public async Task<IActionResult> DeleteSales(int SaleId, int LoginId)
        {
            try
            {
                Common.InfoLogs("DeleteSales Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DeleteSales(SaleId, LoginId);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("DeleteSales Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region DeleteSalesDetails
        [Route("DeleteSalesDetails")]
        [HttpGet]
        public async Task<IActionResult> DeleteSalesDetails(int SaleId, int LoginId, int SalesDetailID)
        {
            try
            {
                Common.InfoLogs("DeleteSalesDetails Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DeleteSalesDetails(SaleId, LoginId, SalesDetailID);

                return Ok(tres);
            }
            catch (Exception ex)
            {
                Common.InfoLogs("DeleteSalesDetails Method error occured,Controller:Admin " + ex.Message.ToString());
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
