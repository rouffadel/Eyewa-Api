using EsalesApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
//using Tweetinvi;
using static EsalesApi.Models.Common;

namespace EsalesApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        public AdminController()
        {
           // Auth.SetUserCredentials("f68tWlQtdHe0AVzTPPyNxHO1O", "BHPZ6ZfLh2yKN5tqMv7CNkPXCB6uOcoJeq6tgmBAaueNE7bsEf", "1326480081112375296-4o5KZfvwzEPZIS0lWZXRIDpCykCmvm", "Xt7fIv1QjP2L24gB5DdXMfZfU1hCDrsKdfIRmw2SJ6HQL");
        }
        public static string converttodate(string date)
        {
            string[] strArray = date.Split('-');
            if (strArray[0].Length == 1)
                strArray[0] = 0.ToString() + strArray[0];
            date = strArray[1] + "-" + strArray[0] + "-" + strArray[2];
            return date;
        }
        #region VerifyUserLogin
        /// <summary>
        /// method to verify the user login
        /// </summary>
        /// <param name="_req"></param>
        /// <param name="LoginName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [Route("VerifyUserLogin")]
        [HttpGet]
        public async Task<IHttpActionResult> VerifyUserLogin(HttpRequestMessage _req, string LoginName, string Password)
        {
            try
            {
                Common.InfoLogs("VerifyUserLogin Method was started,Controller:Admin LoginName=" + LoginName + " Password=" + Password);
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                if (LoginName == null && LoginName == "")
                {
                    Common.InfoLogs("VerifyUserLogin Method,Controller:Admin Invalid LoginName");
                    tres.Status = "-100";
                    tres.Message = "Login Name is mandatory";
                    tres.objresult = null;
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, tres.Message));
                }
                else if (Password == null || Password == "")
                {
                    Common.InfoLogs("VerifyUserLogin Method,Controller:Admin Invalid password");
                    tres.Status = "-100";
                    tres.Message = "Password is mandatory";
                    tres.objresult = null;
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, tres.Message));
                }
                else
                {
                    tres = await business.EVerifyUserLogin(LoginName, Password);
                }
                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("VerifyUserLogin Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
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

                //string csrPath = @"C:\Users\Dell\csr.pem";

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

                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260323104740.csr";

                //string csrPath = @"C:\Users\Dell\csr.pem";

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
                //string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260324085010.csr";

                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260402081257.csr";


                string result = await obj.GenerateProductionCsid(csrPath);

                //string result = await obj.GetProductionCsid();

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
        public async Task<IHttpActionResult> FillStore(HttpRequestMessage _req, int LoginId, int StoreId)
        {
            try
            {
                Common.InfoLogs("FillStore Method was started,Controller:Admin LoginId=" + LoginId + " StoreId=" + StoreId);
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlStore(LoginId, StoreId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillStore Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region InsertSales

        [Route("InsertSales")]
        [HttpPost]
        public async Task<IHttpActionResult> InsertSales(HttpRequestMessage _req,[FromBody]SalesCls sales)
        {
            try
            {
                Common.InfoLogs("InsertSales Method was started,Controller:Admin sales="+JsonConvert.SerializeObject(sales));
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                sales.StoreId = !(sales.StoreId.ToString() != "0") ? 0 : Convert.ToInt32(sales.StoreId);
                sales.CustomerName = !(sales.CustomerName != "") ? "" : sales.CustomerName;
                sales.CustomerNo = !(sales.CustomerNo != "") ? "" : sales.CustomerNo;
                sales.InvoiceNo = !(sales.InvoiceNo != "") ? "" : sales.InvoiceNo;
                sales.InvoiceDate = !(sales.InvoiceDate != "") ? "" :converttodate(sales.InvoiceDate);
                tres = await business.InsertSales(sales);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillStore Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region FillCategory

        [Route("FillCategory")]
        [HttpGet]
        public async Task<IHttpActionResult> FillCategory(HttpRequestMessage _req)
        {
            try
            {
                Common.InfoLogs("FillCategory Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DDLCategory();

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillCategory Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region FillBrand

        [Route("FillBrand")]
        [HttpGet]
        public async Task<IHttpActionResult> FillBrand(HttpRequestMessage _req)
        {
            try
            {
                Common.InfoLogs("FillBrand Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlBrand();

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("FillBrand Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetBrand

        [Route("GetBrand")]
        [HttpGet]
        public async Task<IHttpActionResult> GetBrand(HttpRequestMessage _req,string BrandName)
        {
            try
            {
                Common.InfoLogs("GetBrand Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetBrandID(BrandName);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetBrand Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetProduct

        [Route("GetProduct")]
        [HttpGet]
        public async Task<IHttpActionResult> GetProduct(HttpRequestMessage _req, int CategoryId,int BrandId,int StoreId,string ProductName)
        {
            try
            {
                Common.InfoLogs("GetBrand Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                if(CategoryId==0 || BrandId==0)
                {
                    tres = await business.GetProductCategorybrandID(StoreId, ProductName);
                }
                else
                {
                    tres = await business.GetProductIDandValue(ProductName,CategoryId,BrandId,StoreId);
                }
               

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetBrand Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetOrderLense

        [Route("GetOrderLense")]
        [HttpGet]
        public async Task<IHttpActionResult> GetOrderLense(HttpRequestMessage _req,int SalesId)
        {
            try
            {
                Common.InfoLogs("GetOrderLense Method was started,Controller:Admin");
                TransactResult1 tres = new TransactResult1();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetOrderLenseGrid(SalesId);

                return ResponseMessage(Request.CreateResponse<TransactResult1>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetOrderLense Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetPrescriptionDropDowns

        [Route("GetPrescriptionDropDowns")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPrescriptionDropDowns(HttpRequestMessage _req)
        {
            try
            {
                Common.InfoLogs("GetPrescriptionDropDowns Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetPrescriptiondropdowns();

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetPrescriptionDropDowns Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region SaveOrderLense

        [Route("SaveOrderLense")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveOrderLense(HttpRequestMessage _req, [FromBody]OrderLenseItemsCls order)
        {
            try
            {
                Common.InfoLogs("SaveOrderLense Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();
                int SalesId = order.SalesId;
                tres = await business.SaveOrderLenseItems(order,SalesId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("SaveOrderLense Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetUsers

        [Route("GetUsers")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUsers(HttpRequestMessage _req, int LoginId,int StoreId)
        {
            try
            {
                Common.InfoLogs("GetUsers Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlUser(LoginId,StoreId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetUsers Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        [Route("test")]
        [HttpPost]
        public async Task<IHttpActionResult> test()
        {
            try
            {
                Common.InfoLogs("GetUsers Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

              //var res= await business.GenerateSimplifiedInvoice1();
                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetUsers Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion
        #region GetSalesMan

        [Route("GetSalesMan")]
        [HttpGet]
        public async Task<IHttpActionResult> GetSalesMan(HttpRequestMessage _req, int LoginId, int StoreId)
        {
            try
            {
                Common.InfoLogs("GetSalesMan Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.ddlSalesMan(LoginId, StoreId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesMan Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion
        #region SaveSalesDetails

        [Route("SaveSalesDetails")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveSalesDetails(HttpRequestMessage _req,[FromBody]SaveSalesDetails save)
        {
            try
            {
                Common.InfoLogs("GetSalesMan Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.SaveSalesDetails(save);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesMan Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetSalesGrid

        [Route("GetSalesGrid")]
        [HttpPost]
        public async Task<IHttpActionResult> GetSalesGrid(HttpRequestMessage _req,[FromBody]SearchSalesCls obj)
        {
            try
            {
                Common.InfoLogs("GetSalesGrid Method was started,Controller:Admin ");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetSalesGrid(obj.CustomerName, obj.CustomerNo, obj.InvoiceNo, obj.FromDate, obj.ToDate, obj.SerialNo, obj.LoginID, obj.StoreID);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesGrid Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetInvoiceDetails

        [Route("GetInvoiceDetails")]
        [HttpGet]
        public async Task<IHttpActionResult> GetInvoiceDetails(HttpRequestMessage _req, int SalesId)
        {
            try
            {
                Common.InfoLogs("GetInvoiceDetails Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetInvoiceDetails(SalesId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetInvoiceDetails Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region GetSalesPrint

        [Route("GetSalesPrint")]
        [HttpGet]
        public async Task<IHttpActionResult> GetSalesPrint(HttpRequestMessage _req, int SalesId)
        {
            try
            {
                Common.InfoLogs("GetSalesPrint Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetPrint(SalesId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesPrint Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion
        #region GetSalesDetailsGrid

        [Route("GetSalesDetailsGrid")]
        [HttpGet]
        public async Task<IHttpActionResult> GetSalesDetailsGrid(HttpRequestMessage _req, int SalesId)
        {
            try
            {
                Common.InfoLogs("GetSalesDetailsGrid Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetZatcaQrBySalesId(SalesId);


                //tres = await business.GetSalesDetailsGrid(SalesId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetSalesDetailsGrid Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion
        #region GetQuantity

        [Route("GetQuantity")]
        [HttpGet]
        public async Task<IHttpActionResult> GetQuantity(HttpRequestMessage _req, int ProductId)
        {
            try
            {
                Common.InfoLogs("GetQuantity Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.GetQuantity(ProductId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("GetQuantity Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        #region DeleteSales

        [Route("DeleteSales")]
        [HttpGet]
        public async Task<IHttpActionResult> DeleteSales(HttpRequestMessage _req, int SaleId,int LoginId)
        {
            try
            {
                Common.InfoLogs("DeleteSales Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DeleteSales(SaleId,LoginId);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("DeleteSales Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion


        #region DeleteSales

        [Route("DeleteSalesDetails")]
        [HttpGet]
        public async Task<IHttpActionResult> DeleteSalesDetails(HttpRequestMessage _req, int SaleId, int LoginId,int SalesDetailID)
        {
            try
            {
                Common.InfoLogs("DeleteSalesDetails Method was started,Controller:Admin");
                TransactResult tres = new TransactResult();
                AdminBusiness business = new AdminBusiness();

                tres = await business.DeleteSalesDetails(SaleId, LoginId, SalesDetailID);

                return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
            }
            catch (Exception ex)
            {
                Common.InfoLogs("DeleteSalesDetails Method error occured,Controller:Admin " + ex.Message.ToString());
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        #endregion

        //#region
        //[Route("TwitterMessage")]
        //[HttpGet]
        //public async Task<IHttpActionResult> TwitterMessage(HttpRequestMessage _req, string Message)
        //{
        //    try
        //    {
        //        Common.InfoLogs("DeleteSalesDetails Method was started,Controller:Admin");
        //        TransactResult tres = new TransactResult();
        //        AdminBusiness business = new AdminBusiness();

        //        business.TwitterMsg(Message);

        //        return ResponseMessage(Request.CreateResponse<TransactResult>(HttpStatusCode.OK, tres));
        //    }
        //    catch (Exception ex)
        //    {
        //        Common.InfoLogs("DeleteSalesDetails Method error occured,Controller:Admin " + ex.Message.ToString());
        //        return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
        //    }
        //}
        //#endregion
    }
}
