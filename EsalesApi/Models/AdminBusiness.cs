using Eyewa_new_api.DAL;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

using System.Linq;
using System.Linq;
using System.Net;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Numerics;

using System.Security.Cryptography;

using System.Security.Cryptography.X509Certificates;

using System.Security.Cryptography.Xml;

using System.Text;


using System.Threading.Tasks;

using System.Xml;

using Zatca.EInvoice.SDK;
using Zatca.EInvoice.SDK.Contracts.Models;
using static Eyewa_new_api.Models.Common;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace Eyewa_new_api.Models
{
    public class AdminBusiness
    {
        private string _LoginName;
        private string _Password;
        private string _LoginType;
        private string _MobileNo;
        private string _Year;
        private string _Month;
        private string _FromDate;
        private string _ToDate;
        private int _LoginID;
        private Hashtable ht;
        private DLogin product;
        private DataSet ds;
        private DLogin Dobj;
        private DLogin Sale;
        private string Transation;
        private DCheckPermission Dobj1;
        private string WhereCondition = string.Empty;
        private static readonly HttpClient client = new HttpClient();

        public string FromDate
        {
            get => this._FromDate;
            set => this._FromDate = value;
        }

        public string ToDate
        {
            get => this._ToDate;
            set => this._ToDate = value;
        }

        public string Password
        {
            get => this._Password;
            set => this._Password = value;
        }

        public string LoginName
        {
            get => this._LoginName;
            set => this._LoginName = value;
        }

        public string Year
        {
            get => this._Year;
            set => this._Year = value;
        }

        public string Month
        {
            get => this._Month;
            set => this._Month = value;
        }

        public int LoginID
        {
            get => this._LoginID;
            set => this._LoginID = value;
        }

        public int StoreID { get; set; }

        public string dt1 { get; set; }

        public string dt2 { get; set; }

        public string LoginType
        {
            get => this._LoginType;
            set => this._LoginType = value;
        }

        public string MobileNo
        {
            get => this._MobileNo;
            set => this._MobileNo = value;
        }
        public static string converttodate(string date)
        {
            string[] strArray = date.Split('-');
            if (strArray[0].Length == 1)
                strArray[0] = 0.ToString() + strArray[0];
            date = strArray[2] + "-" + strArray[1] + "-" + strArray[0];
            return date;
        }
        #region EVerifyUserLogin
        /// <summary>
        /// method to get login
        /// </summary>
        /// <param name="LoginName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public Task<TransactResult> EVerifyUserLogin(string LoginName, string Password)
        {
            TransactResult tres = new TransactResult();
            try
            {
                Common.LoginCls obj = new Common.LoginCls();
                this.WhereCondition = "";
                this.Dobj = new DLogin();
                this.ds = new DataSet();
                this.ht = new Hashtable();
                if (LoginName != null)
                    this.WhereCondition = " and L.LoginName='" + LoginName + "' and L.password='" + Password + "' ";
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"VerifyRoleswiseUserlogin");
                this.ds = this.Dobj.GetTransaction("SP_GetDataLogin", this.ht);
                obj.result = ds;
                if (ds != null && ds.Tables[0].Rows.Count > 0 && ds.Tables.Count > 0)
                {

                    int RoleId = Convert.ToInt32(ds.Tables[0].Rows[0]["RoleId"]);
                    int LoginId = Convert.ToInt32(ds.Tables[0].Rows[0]["LoginID"]);
                    if (LoginId != 1)
                    {
                        DataSet ds1 = Echeckpermissions(RoleId);
                        obj.Add = Convert.ToBoolean(ds1.Tables[0].Rows[0]["ADD"].ToString());
                        obj.View = Convert.ToBoolean(ds1.Tables[0].Rows[0]["VIEW"].ToString());
                        obj.Delete = Convert.ToBoolean(ds1.Tables[0].Rows[0]["DELETE"].ToString());
                        obj.Edit = Convert.ToBoolean(ds1.Tables[0].Rows[0]["EDIT"].ToString());
                    }
                    else
                    {
                        obj.View = true;
                        obj.Add = true;
                        obj.Delete = true;
                        obj.Edit = true;
                    }

                }
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = obj;

            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region
        public DataSet Echeckpermissions(int RoleId)
        {
            this.ht = new Hashtable();
            string ScreenUrl = "Sales.aspx";
            this.Dobj1 = new DCheckPermission();
            this.ds = new DataSet();
            this.WhereCondition = " and  R.RoleId='" + (object)RoleId + "' and SM.ScreenUrl like '../Screens/" + ScreenUrl + "%'";
            this.ht.Add((object)"@wherecondition", (object)this.WhereCondition);
            this.ht.Add((object)"@Transaction", (object)"Checkpermissions");
            this.ds = this.Dobj1.GetTransaction("SP_GetDataRoleScreenMapping", this.ht);
            return this.ds;
        }
        #endregion

        #region ddlStore
        /// <summary>
        /// method to get the stores
        /// </summary>
        /// <param name="LoginId"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public Task<TransactResult> ddlStore(int LoginId, int StoreId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                string str = string.Empty;
                if (this.LoginID == 1)
                    this.Transation = nameof(ddlStore);
                else if (LoginId != 1 && StoreId != 0)
                {
                    this.Transation = "ddlStoreForUser";
                    this.WhereCondition = " and L.LoginID =" + (object)LoginId;
                }
                else
                {
                    this.Transation = "ddlStoreForOrgUser";
                    str = " and L.LoginID =" + (object)LoginId;
                }
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@whereCondition2", (object)str);
                this.ht.Add((object)"@Transaction", (object)this.Transation);
                this.ds = this.Sale.GetTransaction("SP_GetDataStoreDeliveryNoteNew", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region InsertSales
        /// <summary>
        /// method to insert the sales for invoice
        /// </summary>
        /// <param name="sales"></param>
        /// <returns></returns>
        public Task<TransactResult> InsertSales(SalesCls sales)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.ht.Add((object)"@SalesID", (object)0);
                this.ht.Add((object)"@LoginID", (object)sales.LoginId);
                this.ht.Add((object)"@CustomerName", (object)sales.CustomerName);
                this.ht.Add((object)"@CustomerNo", (object)sales.CustomerNo);
                this.ht.Add((object)"@StoreID", (object)sales.StoreId);
                this.ht.Add((object)"@GridData", (object)"");
                this.ht.Add((object)"@SalesDetailsID", (object)0);
                this.ht.Add((object)"@GrossTotal", (object)0);
                this.ht.Add((object)"@TotalDiscount", (object)0);
                this.ht.Add((object)"@NetTotal", (object)0);
                this.ht.Add((object)"@UserID", (object)0);
                this.ht.Add((object)"@InvoiceNo", (object)sales.InvoiceNo);
                this.ht.Add((object)"@InvoiceDate", (object)sales.InvoiceDate);
                this.ht.Add((object)"@Remarks", (object)"");
                this.ht.Add((object)"@Balance", (object)0);
                this.ht.Add((object)"@PaidAmount", (object)0);
                this.ht.Add((object)"@PaymentMode1", (object)"");
                this.ht.Add((object)"@Transaction", (object)nameof(InsertSales));
                this.ht.Add((object)"@TotalTax", (object)0);
                this.ds = this.Sale.GetTransaction("SP_Sales_NewwithTax", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region DDLCategory
        /// <summary>
        /// method to get the category
        /// </summary>
        /// <returns></returns>
        public Task<TransactResult> DDLCategory()
        {
            TransactResult tres = new TransactResult();
            this.ds = new DataSet();
            try
            {
                this.ht = new Hashtable();
                this.product = new DLogin();
                this.WhereCondition = string.Empty;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)nameof(DDLCategory));
                this.ds = this.product.GetTransaction("SP_GetDataProducts", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region ddlBrand
        /// <summary>
        /// method to get the brand
        /// </summary>
        /// <returns></returns>
        public Task<TransactResult> ddlBrand()
        {
            TransactResult tres = new TransactResult();
            this.ds = new DataSet();
            try
            {
                this.ht = new Hashtable();
                this.product = new DLogin();
                this.WhereCondition = string.Empty;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)nameof(ddlBrand));
                this.ds = this.product.GetTransaction("SP_GetDataProducts", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region GetBrandID
        /// <summary>
        /// method to get the brand by name
        /// </summary>
        /// <param name="BrandName"></param>
        /// <returns></returns>
        public Task<TransactResult> GetBrandID(string BrandName)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.Sale = new DLogin();
                this.ht = new Hashtable();
                this.WhereCondition = string.Empty;
                string empty = string.Empty;
                if (BrandName != "")
                    this.WhereCondition = " and BrandName like '" + BrandName + "%'";
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)nameof(GetBrandID));
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region GetProductCategorybrandID
        /// <summary>
        /// 
        /// </summary>
        /// <param name="StoreId"></param>
        /// <param name="ProductName"></param>
        /// <returns></returns>
        public Task<TransactResult> GetProductCategorybrandID(int StoreId, string ProductName)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.Sale = new DLogin();
                this.ht = new Hashtable();
                this.WhereCondition = string.Empty;
                if (ProductName != "")
                    this.WhereCondition = " and P.ProductName like '" + ProductName + "%'";
                if (StoreId != 0)
                {

                    this.WhereCondition = this.WhereCondition + " and ST.StoreID =" + (object)StoreId;
                }
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"GetProductCategoryBrandIDValue");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region GetProductIDandValue
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProductName"></param>
        /// <param name="CategoryId"></param>
        /// <param name="BrandId"></param>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public Task<TransactResult> GetProductIDandValue(string ProductName, int CategoryId, int BrandId, int StoreId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.Sale = new DLogin();
                this.ht = new Hashtable();
                this.WhereCondition = string.Empty;
                if (ProductName != "")
                    this.WhereCondition = " and P.ProductName like '" + ProductName + "%'";
                if (CategoryId != 0)
                {

                    this.WhereCondition = this.WhereCondition + " and P.CategoryID=" + (object)CategoryId;
                }
                if (BrandId != 0)
                {

                    this.WhereCondition = this.WhereCondition + " and P.BrandID= " + (object)BrandId;
                }
                if (StoreId != 0)
                {

                    this.WhereCondition = this.WhereCondition + " and ST.StoreID =" + (object)StoreId;
                }
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"GetProductIDValue1");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }

        #endregion

        #region GetOrderLenseGrid
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SalesId"></param>
        /// <returns></returns>
        public Task<TransactResult1> GetOrderLenseGrid(int SalesId)
        {
            TransactResult1 tres = new TransactResult1();
            try
            {
                DataSet dset = new DataSet();
                DataTable dt = new DataTable();
                this.Sale = new DLogin();
                this.ht = new Hashtable();
                this.WhereCondition = string.Empty;
                this.WhereCondition = " and SalesID =" + (object)SalesId;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)nameof(GetOrderLenseGrid));
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                this.ds.Tables[0].TableName = "OrderLense";
                tres.objresult1 = ds;
                // DataTable dt = ds.Tables[0];
                //dset.Tables.Add(dt);
                // dset = ds;
                // dset.Tables[0].TableName = "OrderLense";
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                if (SalesId != 0)
                    this.WhereCondition = " where SaleID =" + (object)SalesId;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"GetPrescriptionDetails");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                //dt = ds.Tables[0];
                //dset.Tables.Add(dt);
                // dset.Tables[1].TableName = "Prescription";
                // this.ds.Tables[0].TableName = "Prescription";
                // dset.Tables.Add(ds.Tables[0]);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult2 = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult1 = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region GetPrescriptiondropdowns
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<TransactResult> GetPrescriptiondropdowns()
        {
            TransactResult tres = new TransactResult();
            this.ds = new DataSet();
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("txt");
                dt.Columns.Add("val");
                float num1 = 16f;
                for (int index = 0; index < 129; ++index)
                {
                    DataRow row = dt.NewRow();
                    if ((double)num1 > 0.0)
                    {
                        row["txt"] = (object)("+" + string.Format("{0:f2}", (object)num1));
                        row["val"] = (object)("+" + string.Format("{0:f2}", (object)num1));
                    }
                    else
                    {
                        row["txt"] = (object)string.Format("{0:f2}", (object)num1);
                        row["val"] = (object)string.Format("{0:f2}", (object)num1);
                    }
                    num1 -= 0.25f;
                    dt.Rows.Add(row);
                }
                this.ds.Tables.Add(dt);
                this.ds.Tables[0].TableName = "SPH";


                dt = new DataTable();
                dt.Columns.Add("txt");
                dt.Columns.Add("val");
                float num2 = 6f;
                for (int index = 0; index < 49; ++index)
                {
                    DataRow row = dt.NewRow();
                    if ((double)num2 > 0.0)
                    {
                        row["txt"] = (object)("+" + string.Format("{0:f2}", (object)num2));
                        row["val"] = (object)("+" + string.Format("{0:f2}", (object)num2));
                    }
                    else
                    {
                        row["txt"] = (object)string.Format("{0:f2}", (object)num2);
                        row["val"] = (object)string.Format("{0:f2}", (object)num2);
                    }
                    num2 -= 0.25f;
                    dt.Rows.Add(row);
                }
                this.ds.Tables.Add(dt);
                this.ds.Tables[1].TableName = "CYL";

                dt = new DataTable();
                dt.Columns.Add("txt");
                dt.Columns.Add("val");
                double num3 = 180.0;
                for (int index = 0; index <= 180; ++index)
                {
                    DataRow row = dt.NewRow();
                    row["txt"] = (object)num3;
                    row["val"] = (object)num3;
                    --num3;
                    dt.Rows.Add(row);
                }

                this.ds.Tables.Add(dt);
                this.ds.Tables[2].TableName = "AXIS";

                dt = new DataTable();
                dt.Columns.Add("txt");
                dt.Columns.Add("val");
                double num4 = 3.5;
                int num5 = 0;
                while (num4 >= 0.0)
                {
                    DataRow row = dt.NewRow();
                    row["txt"] = (object)string.Format("{0:f2}", (object)num4);
                    row["val"] = (object)string.Format("{0:f2}", (object)num4);
                    num4 -= 0.25;
                    dt.Rows.Add(row);
                    ++num5;
                }

                this.ds.Tables.Add(dt);
                this.ds.Tables[3].TableName = "ADD";
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region SaveOrderLenseItems
        /// <summary>
        /// method to save the order lenses
        /// </summary>
        /// <param name="order"></param>
        /// <param name="SalesID"></param>
        /// <returns></returns>
        public Task<TransactResult> SaveOrderLenseItems(OrderLenseItemsCls order, int SalesID)
        {
            TransactResult tres = new TransactResult();
            try
            {
                string str = string.Empty;
                if (order.OrderLenses.Count > 0)
                {
                    for (int i = 0; i < order.OrderLenses.Count; i++)
                    {
                        str = str + order.OrderLenses[i].CategoryId + "~" + order.OrderLenses[i].Brand + "~" + order.OrderLenses[i].Price + "~" + order.OrderLenses[i].Quantity + "~" + order.OrderLenses[i].Total + "$";
                    }
                }
                if (str != "")
                    str = str.Substring(0, str.Length - 1);
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                string GridData = str;
                this.ht.Add((object)"@SalesID", (object)SalesID);
                this.ht.Add((object)"@LoginID", (object)0);
                this.ht.Add((object)"@CustomerName", (object)"");
                this.ht.Add((object)"@CustomerNo", (object)"");
                this.ht.Add((object)"@StoreID", (object)"");
                this.ht.Add((object)"@GridData", (object)GridData);
                this.ht.Add((object)"@SalesDetailsID", (object)0);
                this.ht.Add((object)"@GrossTotal", (object)0);
                this.ht.Add((object)"@Discount", (object)0);
                this.ht.Add((object)"@NetTotal", (object)0);
                this.ht.Add((object)"@UserID", (object)0);
                this.ht.Add((object)"@InvoiceNo", (object)"");
                this.ht.Add((object)"@InvoiceDate", (object)"");
                this.ht.Add((object)"@Balance", (object)0);
                this.ht.Add((object)"@Remarks", (object)"");
                this.ht.Add((object)"@PaidAmount", (object)0);
                this.ht.Add((object)"@PaymentMode1", (object)"");
                this.ht.Add((object)"@Transaction", (object)"SaveOrderLense");
                this.ds = this.Sale.GetTransaction("SP_Sales", this.ht);
                if (this.ds.Tables[0].Rows.Count > 0)
                {
                    string empty = Convert.ToString(this.ds.Tables[0].Rows[0]["Status"]);
                    string str1 = string.Empty;

                    this.ds = new DataSet();
                    for (int index = 0; index < order.PrescriptionDetails.Count; ++index)
                    {
                        string str2 = !(order.PrescriptionDetails[index].sph != "") ? str1 + "~" : str1 + order.PrescriptionDetails[index].sph + "~";
                        string str3 = !(order.PrescriptionDetails[index].cyl != "") ? str2 + "~" : str2 + order.PrescriptionDetails[index].cyl + "~";
                        string str4 = !(order.PrescriptionDetails[index].axis != "") ? str3 + "~" : str3 + order.PrescriptionDetails[index].axis + "~";
                        str1 = !(order.PrescriptionDetails[index].add != "") ? str4 + "~" : str4 + order.PrescriptionDetails[index].add + "~";
                    }
                    string str5 = !(order.PrescriptionIpd.sphtext != "") ? str1 + "~" : str1 + order.PrescriptionIpd.sphtext + "~";
                    string str6 = !(order.PrescriptionIpd.cyltext != "") ? str5 + "~" : str5 + order.PrescriptionIpd.cyltext + "~";
                    string str7 = !(order.PrescriptionIpd.axistext != "") ? str6 + "~" : str6 + order.PrescriptionIpd.axistext + "~";
                    string str8 = !(order.PrescriptionIpd.addtext != "") ? str7 + "~" : str7 + order.PrescriptionIpd.addtext + "~";
                    if (str8 != "")
                        str8 = str8.Substring(0, str8.Length - 1);
                    GridData = str8;
                    this.ds = new DataSet();
                    this.ht = new Hashtable();
                    this.Sale = new DLogin();
                    this.ht.Add((object)"@SalesID", (object)SalesID);
                    this.ht.Add((object)"@LoginID", (object)0);
                    this.ht.Add((object)"@CustomerName", (object)"");
                    this.ht.Add((object)"@CustomerNo", (object)"");
                    this.ht.Add((object)"@StoreID", (object)"");
                    this.ht.Add((object)"@GridData", (object)GridData);
                    this.ht.Add((object)"@SalesDetailsID", (object)0);
                    this.ht.Add((object)"@GrossTotal", (object)0);
                    this.ht.Add((object)"@Discount", (object)0);
                    this.ht.Add((object)"@NetTotal", (object)0);
                    this.ht.Add((object)"@UserID", (object)0);
                    this.ht.Add((object)"@InvoiceNo", (object)"");
                    this.ht.Add((object)"@InvoiceDate", (object)"");
                    this.ht.Add((object)"@Remarks", (object)"");
                    this.ht.Add((object)"@Balance", (object)0);
                    this.ht.Add((object)"@PaidAmount", (object)0);
                    this.ht.Add((object)"@PaymentMode1", (object)"");
                    this.ht.Add((object)"@Transaction", (object)"SavePrescription");
                    this.ds = this.Sale.GetTransaction("SP_Sales", this.ht);
                }
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region ddlUser
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<TransactResult> ddlUser(int LoginId, int StoreId)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                this.Transation = string.Empty;
                this.Transation = nameof(ddlUser);
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)this.Transation);
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region ddlSalesMan
        public Task<TransactResult> ddlSalesMan(int LoginID, int StoreID)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                this.Transation = string.Empty;
                if (this.LoginID == 1)
                {
                    this.Transation = nameof(ddlSalesMan);
                    if (this.StoreID != 0)
                        this.WhereCondition = " and StoreID =" + (object)StoreID;
                }
                else
                {
                    if (this.StoreID != 0)
                        this.WhereCondition = " and L.StoreID =" + (object)StoreID;
                    this.Transation = "ddlSalesManForUser";
                    // ESales esales = this;
                    this.WhereCondition = this.WhereCondition + " and L.LoginID=" + (object)LoginID;
                }
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)this.Transation);
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion




        private void LoadSalesPrintData(int salesId, GetSalesCls sobj)
        {
            Hashtable ht = new Hashtable();
            ht.Add("@WhereCondition", " Where S.SaleID =" + salesId);
            ht.Add("@Transaction", "GetSalesDetailGrid");

            DLogin sale = new DLogin();
            DataSet ds = sale.GetTransaction("SP_GetDataSales", ht);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
                sobj.SalesDetails = ds;

            ht = new Hashtable();
            ht.Add("@WhereCondition", " and S.SaleID =" + salesId);
            ht.Add("@Transaction", "GetPrintPopup");

            ds = sale.GetTransaction("SP_GetDataSales", ht);

            if (ds.Tables[0].Rows.Count > 0)
                sobj.SalesPrint = ds;
        }
        public Task<TransactResult> GetSalesGrid(string CustomerName, string CustomerNo, string InvoiceNo, string FromDate, string ToDate, string SerialNo, int LoginID, int StoreID)
        {
            TransactResult tres = new TransactResult();
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                string str = string.Empty;
                this.Transation = string.Empty;
                StoreID = !(StoreID.ToString() != "0") ? 0 : Convert.ToInt32(StoreID);
                CustomerName = !(CustomerName != "") ? "" : CustomerName;
                CustomerNo = !(CustomerNo != "") ? "" : CustomerNo;
                FromDate = !(FromDate != "") ? "" : FromDate;
                ToDate = !(ToDate != "") ? "" : ToDate;
                InvoiceNo = !(InvoiceNo != "") ? "" : InvoiceNo;
                //  LoginID = Convert.ToInt32(this.Session["LOGINID"]);
                //  OrganisationUser = Convert.ToInt32(this.Session["StoreID"]);
                SerialNo = !(SerialNo != "") ? "" : SerialNo;
                if (this.StoreID != 0)
                    this.WhereCondition = " and S.StoreID =" + (object)StoreID;
                if (CustomerName != "")
                {
                    this.WhereCondition = this.WhereCondition + " and S.CustomerName like '" + CustomerName + "%'";
                }
                if (CustomerNo != "")
                {
                    this.WhereCondition = this.WhereCondition + " and S.CustomerNo like '" + CustomerNo + "%'";
                }
                if (InvoiceNo != "")
                {
                    this.WhereCondition = this.WhereCondition + "and S.InvoiceNo like '%" + InvoiceNo + "%'";
                }
                if (FromDate != "" && ToDate != "" && FromDate != null && ToDate != null)
                {

                    this.WhereCondition = this.WhereCondition + " and cast(S.CreatedDate as date) between '" + converttodate(FromDate) + "' and '" + converttodate(ToDate) + "'";
                }
                if (SerialNo != "")
                {

                    this.WhereCondition = this.WhereCondition + " and REVERSE(SUBSTRING(REVERSE(S.InvoiceNo), 1,CHARINDEX('-', REVERSE(S.InvoiceNo)) - 1))  ='" + SerialNo + "'";
                }
                if (LoginID == 1)
                    this.Transation = nameof(GetSalesGrid);
                else if (StoreID == 0)
                {
                    this.Transation = "GetSalesGridForOrgUser";
                    str = str + " and L.LoginID =" + (object)LoginID;
                }
                else
                {
                    this.Transation = "GetSalesGridForUser";

                    this.WhereCondition = this.WhereCondition + " and L.LoginID =" + (object)LoginID;
                }
                this.ht.Add((object)"@WhereCondition2", (object)str);
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)this.Transation);
                this.ds = this.Sale.GetTransaction("GetDataSalesNew", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }


        #region GetInvoiceDetails
        public Task<TransactResult> GetInvoiceDetails(int SalesId)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                if (SalesId != 0)
                    this.WhereCondition = " and SaleID =" + (object)SalesId;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"InvoiceDetails");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region GetPrint
        public Task<TransactResult> GetPrint(int SalesId)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;
                if (SalesId != 0)
                    this.WhereCondition = " and S.SaleID = " + (object)SalesId;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"GetPrintPopup");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion




        #region GetQuantity
        public Task<TransactResult> GetQuantity(int ProductId)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;

                this.WhereCondition = " and ProductId =" + (object)ProductId;
                this.ht.Add((object)"@WhereCondition", (object)this.WhereCondition);
                this.ht.Add((object)"@Transaction", (object)"GetQuantity1");
                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region
        public Task<TransactResult> DeleteSales(int SalesID, int LoginID)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.ht.Add((object)"@SalesID", (object)SalesID);
                this.ht.Add((object)"@LoginID", (object)LoginID);
                this.ht.Add((object)"@CustomerName", (object)"");
                this.ht.Add((object)"@CustomerNo", (object)"");
                this.ht.Add((object)"@StoreID", (object)0);
                this.ht.Add((object)"@GridData", (object)"");
                this.ht.Add((object)"@SalesDetailsID", (object)0);
                this.ht.Add((object)"@GrossTotal", (object)0);
                this.ht.Add((object)"@Discount", (object)0);
                this.ht.Add((object)"@NetTotal", (object)0);
                this.ht.Add((object)"@UserID", (object)0);
                this.ht.Add((object)"@InvoiceNo", (object)"");
                this.ht.Add((object)"@InvoiceDate", (object)"");
                this.ht.Add((object)"@Remarks", (object)"");
                this.ht.Add((object)"@Balance", (object)0);
                this.ht.Add((object)"@PaidAmount", (object)0);
                this.ht.Add((object)"@PaymentMode1", (object)"");
                this.ht.Add((object)"@Transaction", (object)nameof(DeleteSales));
                this.ds = this.Sale.GetTransaction("SP_Sales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        #region
        public Task<TransactResult> DeleteSalesDetails(int SalesID, int LoginID, int SalesDetailID)
        {
            TransactResult tres = new TransactResult();
            string empty = string.Empty;
            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.ht.Add((object)"@SalesID", (object)0);
                this.ht.Add((object)"@LoginID", (object)LoginID);
                this.ht.Add((object)"@CustomerName", (object)"");
                this.ht.Add((object)"@CustomerNo", (object)"");
                this.ht.Add((object)"@StoreID", (object)StoreID);
                this.ht.Add((object)"@GridData", (object)"");
                this.ht.Add((object)"@SalesDetailsID", (object)SalesDetailID);
                this.ht.Add((object)"@GrossTotal", (object)0);
                this.ht.Add((object)"@Discount", (object)0);
                this.ht.Add((object)"@NetTotal", (object)0);
                this.ht.Add((object)"@UserID", (object)0);
                this.ht.Add((object)"@InvoiceNo", (object)"");
                this.ht.Add((object)"@InvoiceDate", (object)"");
                this.ht.Add((object)"@Remarks", (object)"");
                this.ht.Add((object)"@Balance", (object)0);
                this.ht.Add((object)"@PaidAmount", (object)0);
                this.ht.Add((object)"@PaymentMode1", (object)"");
                this.ht.Add((object)"@Transaction", (object)nameof(DeleteSalesDetails));
                this.ds = this.Sale.GetTransaction("SP_Sales", this.ht);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message.ToString();
                tres.objresult = null;
            }
            return Task.FromResult(tres);
        }
        #endregion

        public void TwitterMsg(string message)
        {
            string authorizationHeader = string.Empty;
            var responseResult = "";
            try
            {
                string twitterURL = "https://api.twitter.com/1.1/statuses/update.json";

                string oauth_consumer_key = "Padavahini Telugu";
                string oauth_consumer_secret = "Fadel1@3";
                string oauth_token = "1326480081112375296-4o5KZfvwzEPZIS0lWZXRIDpCykCmvm";
                string oauth_token_secret = "Xt7fIv1QjP2L24gB5DdXMfZfU1hCDrsKdfIRmw2SJ6HQL";

                // set the oauth version and signature method
                string oauth_version = "1.0";
                string oauth_signature_method = "HMAC-SHA1";

                // create unique request details
                string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
                System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

                // create oauth signature
                string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

                string baseString = string.Format(
                    baseFormat,
                    oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp, oauth_token,
                    oauth_version,
                    Uri.EscapeDataString(message)
                );

                string oauth_signature = null;
                using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oauth_consumer_secret) + "&" + Uri.EscapeDataString(oauth_token_secret))))
                {
                    oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(twitterURL) + "&" + Uri.EscapeDataString(baseString))));
                }

                // create the request header
                string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

                authorizationHeader = string.Format(
                   authorizationFormat,
                   Uri.EscapeDataString(oauth_consumer_key),
                   Uri.EscapeDataString(oauth_nonce),
                   Uri.EscapeDataString(oauth_signature),
                   Uri.EscapeDataString(oauth_signature_method),
                   Uri.EscapeDataString(oauth_timestamp),
                   Uri.EscapeDataString(oauth_token),
                   Uri.EscapeDataString(oauth_version)
               );

                HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(twitterURL);
                objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
                objHttpWebRequest.Method = "POST";
                objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                using (Stream objStream = objHttpWebRequest.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message));
                    objStream.Write(content, 0, content.Length);
                }




                //success posting
                WebResponse objWebResponse = objHttpWebRequest.GetResponse();
                StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
                responseResult = objStreamReader.ReadToEnd().ToString();
            }
            catch (Exception ex)
            {
                responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
            }

        }

        public string GenerateInvoiceXmlSection(SaveSalesDetails save)
        {
            var sb = new StringBuilder();

            decimal totalLineAmount = 0;
            decimal totalTax = 0;
            decimal discount = string.IsNullOrEmpty(save.Discount) ? 0 : Convert.ToDecimal(save.Discount);

            // -------------------------------
            // 1️⃣ FIRST LOOP → CALCULATE TOTALS
            // -------------------------------
            foreach (var item in save.SalesGrids)
            {
                decimal qty = string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDecimal(item.Quantity);
                decimal price = string.IsNullOrEmpty(item.SellingPrice) ? 0 : Convert.ToDecimal(item.SellingPrice);
                decimal taxPer = string.IsNullOrEmpty(item.TaxPer) ? 15 : Convert.ToDecimal(item.TaxPer);

                decimal lineAmount = qty * price;
                decimal taxAmount = (lineAmount * taxPer) / 100;

                totalLineAmount += lineAmount;
                totalTax += taxAmount;
            }

            decimal taxExclusive = totalLineAmount;
            decimal taxInclusive = totalLineAmount + totalTax;
            decimal payableAmount = taxInclusive - discount;

            // -------------------------------
            // 2️⃣ PaymentMeans
            // -------------------------------
            string paymentCode = "10";

            if (save.PaymentMode == "Card")
                paymentCode = "48";
            else if (save.PaymentMode == "Bank")
                paymentCode = "30";

            sb.Append($@"
<cac:PaymentMeans>
    <cbc:PaymentMeansCode>{paymentCode}</cbc:PaymentMeansCode>
</cac:PaymentMeans>
");

            // -------------------------------
            // 3️⃣ AllowanceCharge (Discount)
            // -------------------------------
            sb.Append($@"
<cac:AllowanceCharge>
    <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
    <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
    <cbc:Amount currencyID=""SAR"">{discount:F2}</cbc:Amount>

    <cac:TaxCategory>
        <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
        <cbc:Percent>15</cbc:Percent>
        <cac:TaxScheme>
            <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
        </cac:TaxScheme>
    </cac:TaxCategory>

    <!-- REQUIRED duplicate for ZATCA -->
    <cac:TaxCategory>
        <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
        <cbc:Percent>15</cbc:Percent>
        <cac:TaxScheme>
            <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
        </cac:TaxScheme>
    </cac:TaxCategory>

</cac:AllowanceCharge>
");

            // -------------------------------
            // 4️⃣ Tax Total
            // -------------------------------
            sb.Append($@"
<cac:TaxTotal>
    <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
</cac:TaxTotal>

<cac:TaxTotal>
    <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
    <cac:TaxSubtotal>
        <cbc:TaxableAmount currencyID=""SAR"">{taxExclusive:F2}</cbc:TaxableAmount>
        <cbc:TaxAmount currencyID=""SAR"">{totalTax:F2}</cbc:TaxAmount>
        <cac:TaxCategory>
            <cbc:ID schemeID=""UN/ECE 5305"" schemeAgencyID=""6"">S</cbc:ID>
            <cbc:Percent>15.00</cbc:Percent>
            <cac:TaxScheme>
                <cbc:ID schemeID=""UN/ECE 5153"" schemeAgencyID=""6"">VAT</cbc:ID>
            </cac:TaxScheme>
        </cac:TaxCategory>
    </cac:TaxSubtotal>
</cac:TaxTotal>
");

            // -------------------------------
            // 5️⃣ Legal Monetary Total
            // -------------------------------
            sb.Append($@"
<cac:LegalMonetaryTotal>
    <cbc:LineExtensionAmount currencyID=""SAR"">{totalLineAmount:F2}</cbc:LineExtensionAmount>
    <cbc:TaxExclusiveAmount currencyID=""SAR"">{taxExclusive:F2}</cbc:TaxExclusiveAmount>
    <cbc:TaxInclusiveAmount currencyID=""SAR"">{taxInclusive:F2}</cbc:TaxInclusiveAmount>
    <cbc:AllowanceTotalAmount currencyID=""SAR"">{discount:F2}</cbc:AllowanceTotalAmount>
    <cbc:PrepaidAmount currencyID=""SAR"">0.00</cbc:PrepaidAmount>
    <cbc:PayableAmount currencyID=""SAR"">{payableAmount:F2}</cbc:PayableAmount>
</cac:LegalMonetaryTotal>
");

            // -------------------------------
            // 6️⃣ Invoice Lines (AFTER totals)
            // -------------------------------
            int lineId = 1;

            foreach (var item in save.SalesGrids)
            {
                decimal qty = string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDecimal(item.Quantity);
                decimal price = string.IsNullOrEmpty(item.SellingPrice) ? 0 : Convert.ToDecimal(item.SellingPrice);
                decimal taxPer = string.IsNullOrEmpty(item.TaxPer) ? 15 : Convert.ToDecimal(item.TaxPer);

                decimal lineAmount = qty * price;
                decimal taxAmount = (lineAmount * taxPer) / 100;
                decimal roundingAmount = lineAmount + taxAmount;

                sb.Append($@"
<cac:InvoiceLine>
    <cbc:ID>{lineId}</cbc:ID>
    <cbc:InvoicedQuantity unitCode=""PCE"">{qty:F6}</cbc:InvoicedQuantity>
    <cbc:LineExtensionAmount currencyID=""SAR"">{lineAmount:F2}</cbc:LineExtensionAmount>

    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""SAR"">{taxAmount:F2}</cbc:TaxAmount>
        <cbc:RoundingAmount currencyID=""SAR"">{roundingAmount:F2}</cbc:RoundingAmount>
    </cac:TaxTotal>

    <cac:Item>
        <cbc:Name>Optical Lens</cbc:Name>
<cac:SellersItemIdentification>
    <cbc:ID>{item.ProductId}</cbc:ID>
</cac:SellersItemIdentification>
        <cac:ClassifiedTaxCategory>
            <cbc:ID>Z</cbc:ID>
            <cbc:Percent>{taxPer:F2}</cbc:Percent>
            <cac:TaxScheme>
                <cbc:ID>VAT</cbc:ID>
            </cac:TaxScheme>
        </cac:ClassifiedTaxCategory>
    </cac:Item>

    <cac:Price>
        <cbc:PriceAmount currencyID=""SAR"">{price:F2}</cbc:PriceAmount>
    </cac:Price>
</cac:InvoiceLine>
");

                lineId++;
            }

            return sb.ToString();
        }
        private void UpdateInvoiceId(XmlDocument doc, int salesId)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            XmlNode node = doc.SelectSingleNode(
                "/*[local-name()='Invoice']/*[local-name()='ID']",
                nsmgr
            );

            if (node != null)
            {
                node.InnerText = "INV-" + salesId;
            }
        }
        #region SaveSalesDetails
        public async Task<TransactResult> SaveSalesDetails(SaveSalesDetails save)
        {
            TransactResult tres = new TransactResult();

            try
            {
                GetSalesCls sobj = new GetSalesCls();
                this.ds = new DataSet();
                this.ht = new Hashtable();

                string str1 = "";
                string str2 = "";
                string GridData = "";

                // -------------------------------
                // 1️⃣ Prepare Grid Data
                // -------------------------------
                for (int i = 0; i < save.SalesGrids.Count; i++)
                {
                    str1 += save.SalesGrids[i].CategoryId + "~" +
                            save.SalesGrids[i].BrandId + "~" +
                            save.SalesGrids[i].ProductId + "~" +
                            save.SalesGrids[i].ProductValue + "~" +
                            save.SalesGrids[i].Quantity + "~" +
                            save.SalesGrids[i].Discount + "~" +
                            save.SalesGrids[i].SellingPrice + "~" +
                            save.SalesGrids[i].Tax + "~" +
                            save.SalesGrids[i].TaxPer + "$";
                }

                GridData = string.IsNullOrEmpty(str1) ? "" : str1.TrimEnd('$');

                float GrossTotal = string.IsNullOrEmpty(save.GrossTotal) ? 0 : Convert.ToSingle(save.GrossTotal);
                float Discount = string.IsNullOrEmpty(save.Discount) ? 0 : Convert.ToSingle(save.Discount);
                float NetTotal = string.IsNullOrEmpty(save.NetTotal) ? 0 : Convert.ToSingle(save.NetTotal);
                float Tax = string.IsNullOrEmpty(save.Tax) ? 0 : Convert.ToSingle(save.Tax);
                float Balance = string.IsNullOrEmpty(save.Balance) ? NetTotal : Convert.ToSingle(save.Balance);

                float PaidAmount = string.IsNullOrEmpty(save.AdvancePaidAmount) ? 0 :
                    Convert.ToSingle(save.AdvancePaidAmount);

                string CustomerName = save.CustomerName ?? "";
                string CustomerNo = save.CustomerNo ?? "";
                int SalesManID = save.SalesManId == "0" ? 0 : Convert.ToInt32(save.SalesManId);

                // -------------------------------
                // 2️⃣ Save Sale To DB
                // -------------------------------
                this.Sale = new DLogin();

                ht.Add("@SalesID", save.SalesId);
                ht.Add("@LoginID", save.LoginId);
                ht.Add("@CustomerName", CustomerName);
                ht.Add("@CustomerNo", CustomerNo);
                ht.Add("@StoreID", save.StoreId);
                ht.Add("@GridData", GridData);
                ht.Add("@GrossTotal", GrossTotal);
                ht.Add("@SalesDetailsID", 0);
                ht.Add("@InvoiceNo", "");

                ht.Add("@InvoiceDate", "");
                ht.Add("@Remarks", "");


                ht.Add("@TotalDiscount", Discount);
                ht.Add("@NetTotal", NetTotal);
                ht.Add("@UserID", SalesManID);
                ht.Add("@Balance", Balance);
                ht.Add("@PaidAmount", PaidAmount);
                ht.Add("@PaymentMode1", save.PaymentMode);
                ht.Add("@Transaction", "InsertSalesDetails");
                ht.Add("@TotalTax", Tax);


                ds = Sale.GetTransaction("SP_Sales_NewwithTax", ht);

                string status = string.Empty;
                int salesId = save.SalesId;
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row0 = ds.Tables[0].Rows[0];
                    if (ds.Tables[0].Columns.Contains("Status"))
                        status = Convert.ToString(row0["Status"]);

                    // stored-proc might return the new id in different column names; try common ones
                    if (ds.Tables[0].Columns.Contains("SaleID") && row0["SaleID"] != DBNull.Value)
                        salesId = Convert.ToInt32(row0["SaleID"]);
                    else if (ds.Tables[0].Columns.Contains("SalesID") && row0["SalesID"] != DBNull.Value)
                        salesId = Convert.ToInt32(row0["SalesID"]);
                    else if (ds.Tables[0].Columns.Contains("ID") && row0["ID"] != DBNull.Value)
                        salesId = Convert.ToInt32(row0["ID"]);
                }

                if (status == "Success" || status == "Payment is successfull.")
                {
                    // 🔵 ZATCA PROCESS (Separate try-catch)
                    try
                    {
                        //var path = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Samples\Standard\Debit\Standard_Debit_Note.xml";
                        //var xmlPath = @"D:\syed_raufsirproj\invoice.xml";

                        ////string xmlPath = @"D:\syed_raufsirproj\newproj\Simplified_Invoice.xml";

                        ////new
                        //XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
                        //doc.Load(xmlPath);
                        ////new

                        ////var doc = LoadXml(xmlPath);

                        //decimal dec = Convert.ToDecimal(Discount);

                        ////UpdateInvoiceId(doc, save.SalesId);

                        //string dynamicXml = GenerateInvoiceXmlSection(save);

                        //// ✅ STEP 2: Load template
                        //string fullXml = File.ReadAllText(xmlPath);

                        //var invoiceno = new StringBuilder();

                        //invoiceno.Append($@"<cbc:ID>{save.SalesId}</cbc:ID>");


                        //fullXml = fullXml.Replace("<!--INVOICE_DYNAMIC_ID-->", invoiceno.ToString());

                        //// ✅ STEP 3: Inject dynamic section
                        //fullXml = fullXml.Replace("<!--INVOICE_DYNAMIC_SECTION-->", dynamicXml);
                        
                        await GenerateSimplifiedInvoiceworking(save, save.SalesId);

                        //await GenerateSimplifiedInvoice(fullXml, save.SalesId);


                        //var invoiceData = GetInvoiceDataForZatca(salesId);



                        //SaveZatcaResponse(salesId, uuid, hash, response);
                    }
                    catch (Exception zatcaEx)
                    {

                        throw new Exception("ZATCA ERROR: " + zatcaEx.Message);
                    }

                    LoadSalesPrintData(salesId, sobj);
                }

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = sobj;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
                tres.objresult = null;
            }

            return tres;
        }
       
        public static string GenerateSignedPropertiesDigest(XmlDocument doc)
        {
            var c14n = new XmlDsigC14NTransform();
            c14n.LoadInput(doc);

            using (Stream stream = (Stream)c14n.GetOutput(typeof(Stream)))
            using (SHA256 sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(stream));
            }
        }
        


      

        public async Task<string> GenerateSerialNumber(string vatNumber)
            {
                if (string.IsNullOrWhiteSpace(vatNumber))
                    throw new ArgumentException("VAT number is required");

                // Fixed UUID for consistency (important!)
                string uuid1 = Guid.NewGuid().ToString();
                string uuid2 = Guid.NewGuid().ToString();

                return $"1-{uuid1}|2-{vatNumber}|3-{uuid2}";
            }
        
        public async Task<string> GenerateComplianceCsid(string csrPath, string otp)
        {
            string csrPem = File.ReadAllText(csrPath);

            csrPem = csrPem
                .Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
                .Replace("-----END CERTIFICATE REQUEST-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Clear();
            //added new 
     //       string binarySecurityToken = "TUlJQ0Z6Q0NBYjJnQXdJQkFnSUdBWno0YjUvaE1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpFMk1qQTFOakkxV2hjTk16RXdNekUxTWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFUNmdNL1QrRGZBQVY3b3pyUE5KRjN0NVc3c2YvZWw4WGNzbCt5bnllTzZwRlllSWlDZzE3eEIwWWlYTmU4RWJRY3BGOUZrbi9QU1V3N0RMS0JqU1BGMm80RzZNSUczTUF3R0ExVWRFd0VCL3dRQ01BQXdnYVlHQTFVZEVRU0JuakNCbTZTQm1EQ0JsVEU3TURrR0ExVUVCQXd5TVMxVVUxUjhNaTFVVTFSOE15MHhNak0wTlRZM09DMHhNak0wTFRFeU16UXRNVEl6TkMweE1qTTBOVFkzT0Rrd01USXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRRE4yL0lKTE5nWjZTRkwyWGZQdTNIWjJyckFUUE9keEwwY0N3YWgyZUlZVGdJZ2IvS29OSExFcmdzRGQvdzVlRm9URmFOb3ZpUkZNUEV3dTdKTkJoWGJFTXc9";
     //       string secret = "DbGTOAW9Eyg4ntfs3VAQJOMpZ7QHbCA71lu0hKW+Q9k=";
     //       var auth = Convert.ToBase64String(
     //    Encoding.UTF8.GetBytes($"{binarySecurityToken}:{secret}")
     //);

     //       client.DefaultRequestHeaders.Authorization =
     //           new AuthenticationHeaderValue("Basic", auth);
//added new
            client.DefaultRequestHeaders.Add("Accept-Version", "V2");
            client.DefaultRequestHeaders.Add("OTP", otp);


            var body = new
            {
                csr = csrPem
            };

            string json = JsonConvert.SerializeObject(body);
            //for complain use this 
            var response = await client.PostAsync(
                  "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/compliance",
               // "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance",
               //"https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance",
                new StringContent(json, Encoding.UTF8, "application/json")
            );
            //for prod  
            // var response = await client.PostAsync(
            //    "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/production/csids",
            //    //"https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance",
            //    new StringContent(json, Encoding.UTF8, "application/json")
            //);
            string result = await response.Content.ReadAsStringAsync();

            return await response.Content.ReadAsStringAsync();
        }

       

        public async Task<string> GenerateProductionCsid(string csrPath)
        {

            string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjFPcUtJeU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TkRBeU1UUTBOakF5V2hjTk16RXdOREF4TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRRG8zazllbE5UZGJYRHRUK2VGaVpEUjYxUytncm5heS83NTVjbXRNWnZ3WlFJZ041QTJaWkh3NEU0MEN3djZZV2hJamRzTHYwSXA2bmFPa0RZenpUcG1PWUE9";
            string secret = "+XWfmvpwPj1AYtsSgpbEL1mqkrpgLa0IYzWTFZuYL8I=";


            //string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjBnZFg3YU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJME1UVXlOek01V2hjTk16RXdNekl6TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFUWXQ1MmxIUXk4Qm1hckoyMEZhVER1NUpBVUpYQkdqRFdvL1ptNzdRZ3BvRGt5M3VoVXQ3TmZzYStrMlZlQnowVmVVVWN3Y093MTk1MHY5cjQ0U0M1Um80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lGNEtXbFl2aElPMlIydjZhWTJSdzh4UmhKRm51dk82SWNRcjgwUldYNkk2QWlFQW9GSkE0TGQ3RXA1d3J5bjVSWHczUUFyWGJwNTBKajJuQ2QzcERQdmRFdTA9";
            //string secret = "I9yHluoLjLZ+/2eLj3Qn5xT4wvxzGNUdF0hzI2TJJdA=";
            //string binarySecurityToken = "TUlJQ0ZqQ0NBYjJnQXdJQkFnSUdBWjBieXpkdE1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJek1UYzBNekV4V2hjTk16RXdNekl5TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSTXN4ZlBNUTZTNVBRQjI5VnV2MkFGYk5waWFsT3BvYWZUNy9JRDc1Mmh4ZmthOWdSSkJJRHh1NHJiekU1c2hzYng1V1UreTBlUXBUZnVsNk1JSlNGa280RzZNSUczTUF3R0ExVWRFd0VCL3dRQ01BQXdnYVlHQTFVZEVRU0JuakNCbTZTQm1EQ0JsVEU3TURrR0ExVUVCQXd5TVMxVFNVUjhNaTFUU1VSOE15MDFOVEJsT0RRd01DMWxNamxpTFRReFpEUXRZVGN4TmkwME5EWTJOVFUwTkRBd01EQXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGNBTUVRQ0lGcnlIdzZaKytDZDkyYTVveDZYNzJCZllnRnVUdmNoVW14ZlI3ZUxRZ1JwQWlCUkZYbG8xL01OZWNwVm1OaVVBSEtHTi9XQnpJbnJNUHJJelBzNTljQ1hYZz09";
            //string secret = "KSlgdzUeP/epNoUKmV5jsx9/gc6HT049BBxpW6G79yA=";
            string csrPem = File.ReadAllText(csrPath);

            csrPem = csrPem
                .Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
                .Replace("-----END CERTIFICATE REQUEST-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            using (HttpClient client = new HttpClient())
            {
                var auth = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{binarySecurityToken}:{secret}")
                );

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", auth);

                client.DefaultRequestHeaders.Add("Accept-Version", "V2");

                // ✅ REQUIRED IN REAL FLOW

                client.DefaultRequestHeaders.Add("compliance_request_id", "1775141167666");

                var body = new { csr = csrPem };

                var content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(
                    "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/production/csids",
                    content
                );

                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<string> GetProductionCsid()
        {
            // 🔴 Replace with your Compliance CSID values

            string binarySecurityToken = "TUlJQ0Z6Q0NBYjJnQXdJQkFnSUdBWjBkNC96WE1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJME1ETXlPVEk0V2hjTk16RXdNekl6TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSTXN4ZlBNUTZTNVBRQjI5VnV2MkFGYk5waWFsT3BvYWZUNy9JRDc1Mmh4ZmthOWdSSkJJRHh1NHJiekU1c2hzYng1V1UreTBlUXBUZnVsNk1JSlNGa280RzZNSUczTUF3R0ExVWRFd0VCL3dRQ01BQXdnYVlHQTFVZEVRU0JuakNCbTZTQm1EQ0JsVEU3TURrR0ExVUVCQXd5TVMxVFNVUjhNaTFUU1VSOE15MDFOVEJsT0RRd01DMWxNamxpTFRReFpEUXRZVGN4TmkwME5EWTJOVFUwTkRBd01EQXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRQ2pQOXRvR2lIQ01Rd3kxZDVYVUYyc01PRXk4OFhRQmpRdlN3bHlKeXF3TlFJZ0ZHT2k5UGYxRzdWY1MxOUMwYmlINTZndndYNTZSQnhQb2hiSlFwcXhBMFk9";
            string secret = "IaN4RJJxVZb8n+es1X5k9Icg6rbILrRhhxngKDve7Hg=";

            //string binarySecurityToken = "TUlJQ0ZqQ0NBYjJnQXdJQkFnSUdBWjBieXpkdE1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJek1UYzBNekV4V2hjTk16RXdNekl5TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSTXN4ZlBNUTZTNVBRQjI5VnV2MkFGYk5waWFsT3BvYWZUNy9JRDc1Mmh4ZmthOWdSSkJJRHh1NHJiekU1c2hzYng1V1UreTBlUXBUZnVsNk1JSlNGa280RzZNSUczTUF3R0ExVWRFd0VCL3dRQ01BQXdnYVlHQTFVZEVRU0JuakNCbTZTQm1EQ0JsVEU3TURrR0ExVUVCQXd5TVMxVFNVUjhNaTFUU1VSOE15MDFOVEJsT0RRd01DMWxNamxpTFRReFpEUXRZVGN4TmkwME5EWTJOVFUwTkRBd01EQXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGNBTUVRQ0lGcnlIdzZaKytDZDkyYTVveDZYNzJCZllnRnVUdmNoVW14ZlI3ZUxRZ1JwQWlCUkZYbG8xL01OZWNwVm1OaVVBSEtHTi9XQnpJbnJNUHJJelBzNTljQ1hYZz09";
            //string secret = "KSlgdzUeP/epNoUKmV5jsx9/gc6HT049BBxpW6G79yA=";

            // Endpoint
            //string url = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/production/csids";

            string url = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/production/csids";

            using (var client = new HttpClient())
            {
                // ✅ Create Basic Auth
                var auth = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{binarySecurityToken}:{secret}")
                );

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", auth);

                // Required headers
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                client.DefaultRequestHeaders.Add("compliance_request_id", "1234567890123");
                var requestBody = "{}"; // empty JSON body

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                try
                {
                    var result = await response.Content.ReadAsStringAsync();
                   

                    Console.WriteLine("Status: " + response.StatusCode);
                    Console.WriteLine("Response: " + result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                return "s";
            }
        }
        //prod csid generate 

           
      
            public static string GenerateHash(string xml)
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(xml);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

                // SAME as sample transforms

                var nodesToRemove = doc.SelectNodes(
                    "//ext:UBLExtensions | //cac:Signature | //cac:AdditionalDocumentReference[cbc:ID='QR']",
                    nsmgr
                );

                foreach (XmlNode node in nodesToRemove)
                    node.ParentNode.RemoveChild(node);

                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                transform.LoadInput(doc);

                using (Stream s = (Stream)transform.GetOutput(typeof(Stream)))
                using (SHA256 sha = SHA256.Create())
                {
                    return Convert.ToBase64String(sha.ComputeHash(s));
                }
            }


      
        
        
      
        public static async Task<string> ComplianceCheck(
string uuid,
string hash,
string base64Xml,
string token,
string secret)
        {
            string auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(token + ":" + secret)
            );

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", auth);

            client.DefaultRequestHeaders.Add("Accept-Version", "V2");

            var body = new
            {
                invoiceHash = hash,
                uuid = uuid,
                invoice = base64Xml
            };

            string json = JsonConvert.SerializeObject(body);

            var response = await client.PostAsync(
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance/invoices",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }
        
           
        
        private static string GenerateZatcaQr(
string sellerName,
string vatNo,
string invoiceDate,
string total,
string vatAmount)
        {
            List<byte> qrBytes = new List<byte>();

            void AddField(int tag, string value)
            {
                byte[] valueBytes = Encoding.UTF8.GetBytes(value ?? "");
                qrBytes.Add((byte)tag);
                qrBytes.Add((byte)valueBytes.Length);
                qrBytes.AddRange(valueBytes);
            }

            AddField(1, sellerName);
            AddField(2, vatNo);
            AddField(3, invoiceDate);
            AddField(4, total.Replace(",", "."));
            AddField(5, vatAmount.Replace(",", "."));

            return Convert.ToBase64String(qrBytes.ToArray());
        }



        #endregion
        //added
        public Task<TransactResult> GetZatcaQrBySalesId(int salesId)
        {
            TransactResult tres = new TransactResult();

            try
            {

                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;

                if (salesId != 0)
                    this.WhereCondition = " Where S.SaleID =" + salesId;

                this.ht.Add("@WhereCondition", this.WhereCondition);
                this.ht.Add("@Transaction", "GetSalesDetailGrid1");

                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Invoice data not found");

                var row = ds.Tables[0].Rows[0];

                string sellerName = "Naimat Al Basar";
                string vatNo = "310254659700003";
                //openssl ecparam -name secp256k1 -genkey -noout -out privatekey.pem
                //openssl req -new -key privatekey.pem -out csr.pem
                string invoiceDate = Convert.ToDateTime(row["InvoiceDate"])
                                        .ToString("yyyy-MM-ddTHH:mm:ss");
                string total = row["NetTotal"]?.ToString() ?? "0";
                string vatAmount = row["TotalTax"]?.ToString() ?? "0";


                //
                Hashtable ht = new Hashtable();
                ht.Add("@SalesId", salesId);

                DLogin db = new DLogin();
                DataSet ds1 = db.GetTransaction("SP_GetZatcaInvoiceBySalesId", ht);

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("ZATCA QR not found");

                var row1 = ds1.Tables[0].Rows[0];

                // ✅ Get QR from DB (already TLV Base64)
                string qrBase64 = row1["QRCode"].ToString();

                QRCodeGenerator qrGenerator = new QRCodeGenerator();

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrBase64, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                string folder = @"C:\Temp";
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, "zatca_qr.png");
                File.WriteAllBytes(filePath, qrBytes);

                string qrImageBase64 = Convert.ToBase64String(qrBytes);


                //using (Bitmap bitMap = qrCode.GetGraphic(20))
                //{
                //    using (MemoryStream ms = new MemoryStream())
                //    {
                //        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //        qrImageBase64 = Convert.ToBase64String(ms.ToArray());
                //    }
                //}

                // ✅ Send to UI
                tres.qrcodeimg = "data:image/png;base64," + qrImageBase64;
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;

            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }

            return Task.FromResult(tres);
        }
        //added
        public Task<TransactResult> GetSalesDetailsGrid(int SalesID)
        {
            TransactResult tres = new TransactResult();

            try
            {
                this.ds = new DataSet();
                this.ht = new Hashtable();
                this.Sale = new DLogin();
                this.WhereCondition = string.Empty;

                if (SalesID != 0)
                    this.WhereCondition = " Where S.SaleID =" + SalesID;

                this.ht.Add("@WhereCondition", this.WhereCondition);
                this.ht.Add("@Transaction", "GetSalesDetailGrid1");

                this.ds = this.Sale.GetTransaction("SP_GetDataSales", this.ht);

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Invoice data not found");

                var row = ds.Tables[0].Rows[0];

                string sellerName = "Naimat Al Basar";
                string vatNo = "310254659700003";
                //openssl ecparam -name secp256k1 -genkey -noout -out privatekey.pem
                //openssl req -new -key privatekey.pem -out csr.pem
                string invoiceDate = Convert.ToDateTime(row["InvoiceDate"])
                                        .ToString("yyyy-MM-ddTHH:mm:ss");
                string total = row["NetTotal"]?.ToString() ?? "0";
                string vatAmount = row["TotalTax"]?.ToString() ?? "0";

                // ✅ Generate ZATCA Base64 TLV QR
                string base64Qr = GenerateZatcaQr(
                    sellerName,
                    vatNo,
                    invoiceDate,
                    total,
                    vatAmount
                );

                // ✅ Generate QR Image from Base64 string
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(base64Qr, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);
                string qrImageBase64 = Convert.ToBase64String(qrBytes);

                tres.qrcodeimg = "data:image/png;base64," + qrImageBase64;
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = ds;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
                tres.objresult = null;
            }

            return Task.FromResult(tres);
        }
        public static string GenerateDigest(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // REMOVE ext:UBLExtensions
            foreach (XmlNode node in doc.SelectNodes("//ext:UBLExtensions", nsmgr))
                node.ParentNode.RemoveChild(node);

            // REMOVE cac:Signature
            foreach (XmlNode node in doc.SelectNodes("//cac:Signature", nsmgr))
                node.ParentNode.RemoveChild(node);

            // REMOVE QR
            foreach (XmlNode node in doc.SelectNodes("//cac:AdditionalDocumentReference[cbc:ID='QR']", nsmgr))
                node.ParentNode.RemoveChild(node);

            doc.Normalize(); // ✅ VERY IMPORTANT

            var transform = new System.Security.Cryptography.Xml.XmlDsigC14NTransform();
            transform.LoadInput(doc);

            using (Stream s = (Stream)transform.GetOutput(typeof(Stream)))
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(s);
                return Convert.ToBase64String(hash);
            }
        }




        public async Task<string> GenerateSimplifiedInvoice(string fullxml,int salesId)
        {
            //string privateKey = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-private-key-20260312101013.key"; // .key file
           
            string privateKey = "MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgI6yQt8wMiaGkuS7ZBxEjO8phjbBjVWf4tBodePeTkcKgBwYFK4EEAAqhRANCAASfD0EvcxeoLuFbNk/ZcLp4LsWV8vyEk0Uvyg+eh02P2wBA/68riPa9yYtyO7n3jsepMpxR6jvzNzXg6RaIanB5";
           

            string binaryToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjBvMWxTdk1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJMk1EWXpNREl6V2hjTk16RXdNekkxTWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFUWXQ1MmxIUXk4Qm1hckoyMEZhVER1NUpBVUpYQkdqRFdvL1ptNzdRZ3BvRGt5M3VoVXQ3TmZzYStrMlZlQnowVmVVVWN3Y093MTk1MHY5cjQ0U0M1Um80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lGNFZsODNocy9wc1ZtS0l0QW9xOXBXL1IvMEVHRjB2Tzducnl0MmNwMGR0QWlFQXpHQ0pBVlZ2b3NvUld3NERtRmNJZVhadEZFcDRLTzRKNjRiUjdZbnZNdzg9";

            //var path = @"D:\invoice.xml";
            //var doc = LoadXml(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fullxml);

            string uuid = Guid.NewGuid().ToString();

            UpdateDynamicFields(doc, uuid);

            //added
            string qrCode = InsertQR(doc);
            //added

            var hashDoc = PrepareForHash(doc);

            var canonical = Canonicalize(hashDoc);



            var digest1 = GenerateDigest1(canonical);   // ✅ FIX
            var digest2 = GenerateDigest2(doc);



            string signature = SignSignedInfo(doc, privateKey);

            //Inject(doc, digest1,digest2, signature, binaryToken);

            Inject(doc, digest1, digest2, signature, binaryToken);


            var response =  await SendInvoice1(uuid, digest1, doc.OuterXml);

            SaveZatcaData(
    salesId,
    uuid,
    digest1,
    doc.OuterXml,
    qrCode,
    response
);
            return response;

        }

        public void SaveZatcaData(int salesId, string uuid, string hash, string xml, string qr, string response)
{
    Hashtable ht = new Hashtable();

    ht.Add("@SalesId", salesId);
    ht.Add("@UUID", uuid);
    ht.Add("@InvoiceHash", hash);
    ht.Add("@InvoiceXml", xml);
    ht.Add("@QRCode", qr);
    ht.Add("@ZatcaResponse", response);
    ht.Add("@Status", response.Contains("SUCCESS") ? "Success" : "Failed");

    DLogin db = new DLogin();
    db.GetTransaction("SP_SaveZatcaInvoice", ht);
}
        //newcode added
     

        XmlDocument LoadXml(string path)
        {
            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null   // ✅ IMPORTANT
            };
            doc.Load(path);
            return doc;
        }
        void UpdateDynamicFields(XmlDocument doc, string uuid)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            doc.SelectSingleNode("//cbc:UUID", ns).InnerText = uuid;
            doc.SelectSingleNode("//cbc:IssueDate", ns).InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd");
            //doc.SelectSingleNode("//cbc:ActualDeliveryDate", ns).InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd");

            doc.SelectSingleNode("//cbc:IssueTime", ns).InnerText = DateTime.Now.ToString("HH:mm:ss");
        }

        XmlDocument PrepareForHash(XmlDocument original)
        {
            XmlDocument clone = (XmlDocument)original.Clone();

            var ns = new XmlNamespaceManager(clone.NameTable);
            ns.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // Remove UBLExtensions
            foreach (XmlNode node in clone.SelectNodes("//ext:UBLExtensions", ns))
                node.ParentNode.RemoveChild(node);

            // Remove Signature
            foreach (XmlNode node in clone.SelectNodes("//cac:Signature", ns))
                node.ParentNode.RemoveChild(node);

            // ✅ REMOVE QR (MISSING IN YOUR CODE)
            foreach (XmlNode node in clone.SelectNodes("//cac:AdditionalDocumentReference[cbc:ID='QR']", ns))
                node.ParentNode.RemoveChild(node);

            return clone;
        }
        byte[] Canonicalize(XmlDocument doc)
        {
            var transform = new XmlDsigC14NTransform();
            transform.LoadInput(doc);

            using (Stream s = (Stream)transform.GetOutput(typeof(Stream)))
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }
        string GenerateDigest2(XmlDocument doc)
        {
            var node = doc.SelectSingleNode("//*[@Id='xadesSignedProperties']");

            if (node == null)
                throw new Exception("SignedProperties node not found");

            // ✅ Create new document ONLY with this node
            XmlDocument tempDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            XmlNode imported = tempDoc.ImportNode(node, true);
            tempDoc.AppendChild(imported);

            var transform = new XmlDsigC14NTransform();
            transform.LoadInput(tempDoc);

            using (Stream s = (Stream)transform.GetOutput(typeof(Stream)))
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);

                using (SHA256 sha = SHA256.Create())
                {
                    return Convert.ToBase64String(sha.ComputeHash(ms.ToArray()));
                }
            }
        }


        string GenerateDigest1(byte[] canonicalData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(canonicalData));
            }
        }
        

        string SignSignedInfo(XmlDocument doc, string privateKeyBase64)
        {
            // ✅ Safe selection (ignore namespace issues)
            var signedInfoNode = doc.SelectSingleNode("//*[local-name()='SignedInfo']");

            if (signedInfoNode == null)
                throw new Exception("SignedInfo not found");

            // ✅ Wrap node in new document
            XmlDocument tempDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            XmlNode imported = tempDoc.ImportNode(signedInfoNode, true);
            tempDoc.AppendChild(imported);

            // ✅ Canonicalize
            var transform = new XmlDsigC14NTransform();
            transform.LoadInput(tempDoc);

            byte[] canonical;
            //string SignSignedInfo(XmlDocument doc, string privateKeyBase64)
            //{
            //    var ns = new XmlNamespaceManager(doc.NameTable);
            //    ns.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            //    var signedInfoNode = doc.SelectSingleNode("//ds:SignedInfo", ns);

            //    var transform = new XmlDsigC14NTransform();
            //    transform.LoadInput(signedInfoNode);

            //    byte[] canonical;

            using (Stream s = (Stream)transform.GetOutput(typeof(Stream)))
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                canonical = ms.ToArray();
            }
            byte[] keyBytes = Convert.FromBase64String(privateKeyBase64);

            AsymmetricKeyParameter keyParam = PrivateKeyFactory.CreateKey(keyBytes);
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)keyParam;

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true, privateKey);
            signer.BlockUpdate(canonical, 0, canonical.Length);

            byte[] signature = signer.GenerateSignature();

            return Convert.ToBase64String(signature);

            
        }
        
       
        void Inject(XmlDocument doc, string digest1, string digest2, string signature, string binarySecurityToken)
        {
            // ✅ Dynamic values
            string uuid = Guid.NewGuid().ToString();
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string time = DateTime.UtcNow.ToString("HH:mm:ss");
            string signingTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");



            // ✅ Certificate extraction
            var cert = new X509Certificate2(Convert.FromBase64String(binarySecurityToken));
            string issuer = cert.Issuer;
            string serial = cert.SerialNumber;

            // ✅ Certificate digest
            string certDigest;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(cert.RawData);
                certDigest = Convert.ToBase64String(hash);
            }

            // ✅ Namespace setup
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            //added
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            //added
            ns.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");

            // =========================
            // ✅ Inject DIGESTS
            // =========================

            // Digest 1 (invoice hash)
            doc.SelectSingleNode("(//ds:DigestValue)[1]", ns).InnerText = digest1;

            // Digest 2 (SignedProperties)
            doc.SelectSingleNode("(//ds:DigestValue)[2]", ns).InnerText = digest2;

            // =========================
            // ✅ SIGNATURE
            // =========================
            doc.SelectSingleNode("//ds:SignatureValue", ns).InnerText = signature;

            // =========================
            // ✅ CERTIFICATE
            // =========================
            doc.SelectSingleNode("//ds:X509Certificate", ns).InnerText = binarySecurityToken;

            // =========================
            // ✅ SIGNING INFO
            // =========================
            doc.SelectSingleNode("//xades:SigningTime", ns).InnerText = signingTime;

            doc.SelectSingleNode("//xades:CertDigest/ds:DigestValue", ns).InnerText = certDigest;

            doc.SelectSingleNode("//ds:X509IssuerName", ns).InnerText = issuer;

            //added
            string serialHex = cert.SerialNumber;

            // Convert HEX → DECIMAL

            byte[] hexBytes = Enumerable.Range(0, serialHex.Length / 2)
    .Select(x => Convert.ToByte(serialHex.Substring(x * 2, 2), 16))
    .ToArray();

            var serialDecimal = new BigInteger(hexBytes);
            //var serialDecimal = BigInteger.Parse(serialHex, NumberStyles.HexNumber);

            //added

            doc.SelectSingleNode("//ds:X509SerialNumber", ns).InnerText = serialDecimal.ToString(); ;





        }





        //aded
        string GenerateQRCode(XmlDocument doc)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // 1. Seller Name
            string sellerName = doc.SelectSingleNode("//cac:PartyLegalEntity/cbc:RegistrationName", ns)?.InnerText;

            // 2. VAT Number
            string vatNumber = doc.SelectSingleNode("//cac:PartyTaxScheme/cbc:CompanyID", ns)?.InnerText;

            // 3. Timestamp (IssueDate + IssueTime)
            string issueDate = doc.SelectSingleNode("//cbc:IssueDate", ns)?.InnerText;
            string issueTime = doc.SelectSingleNode("//cbc:IssueTime", ns)?.InnerText;
            string timestamp = $"{issueDate}T{issueTime}";

            // 4. Invoice Total (with VAT)
            string invoiceTotal = doc.SelectSingleNode("//cac:LegalMonetaryTotal/cbc:PayableAmount", ns)?.InnerText;

            // 5. VAT Total
            string vatTotal = doc.SelectSingleNode("//cac:TaxTotal[1]/cbc:TaxAmount", ns)?.InnerText;

            // TLV Encoding
            byte[] tlv = BuildTLV(
                sellerName,
                vatNumber,
                timestamp,
                invoiceTotal,
                vatTotal
            );

            return Convert.ToBase64String(tlv);
        }
        byte[] BuildTLV(params string[] values)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                for (int i = 0; i < values.Length; i++)
                {
                    byte tag = (byte)(i + 1);
                    byte[] valueBytes = Encoding.UTF8.GetBytes(values[i] ?? "");

                    stream.WriteByte(tag);
                    stream.WriteByte((byte)valueBytes.Length);
                    stream.Write(valueBytes, 0, valueBytes.Length);
                }

                return stream.ToArray();
            }
        }
        string InsertQR(XmlDocument doc)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            string qrValue = GenerateQRCode(doc);

            var qrNode = doc.SelectSingleNode(
                "//cac:AdditionalDocumentReference[cbc:ID='QR']/cac:Attachment/cbc:EmbeddedDocumentBinaryObject",
                ns
            );

            if (qrNode != null)
            {
                qrNode.InnerText = qrValue;
            }
            return qrValue;
        }


        public static async Task<string> SendInvoice1(string uuid, string hash, string xml)
        {
            try
            {
                //string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjBvMWxTdk1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJMk1EWXpNREl6V2hjTk16RXdNekkxTWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFUWXQ1MmxIUXk4Qm1hckoyMEZhVER1NUpBVUpYQkdqRFdvL1ptNzdRZ3BvRGt5M3VoVXQ3TmZzYStrMlZlQnowVmVVVWN3Y093MTk1MHY5cjQ0U0M1Um80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lGNFZsODNocy9wc1ZtS0l0QW9xOXBXL1IvMEVHRjB2Tzducnl0MmNwMGR0QWlFQXpHQ0pBVlZ2b3NvUld3NERtRmNJZVhadEZFcDRLTzRKNjRiUjdZbnZNdzg9";
                //string secret = "MgReLr2lsi+FadJiasjN2HOB0LttHSm0MxGTnz8CcpQ=";

                //string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjBvMWxTdk1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TXpJMk1EWXpNREl6V2hjTk16RXdNekkxTWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFUWXQ1MmxIUXk4Qm1hckoyMEZhVER1NUpBVUpYQkdqRFdvL1ptNzdRZ3BvRGt5M3VoVXQ3TmZzYStrMlZlQnowVmVVVWN3Y093MTk1MHY5cjQ0U0M1Um80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lGNFZsODNocy9wc1ZtS0l0QW9xOXBXL1IvMEVHRjB2Tzducnl0MmNwMGR0QWlFQXpHQ0pBVlZ2b3NvUld3NERtRmNJZVhadEZFcDRLTzRKNjRiUjdZbnZNdzg9";
                //string secret = "MgReLr2lsi+FadJiasjN2HOB0LttHSm0MxGTnz8CcpQ=";

                //prd test

                //string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjFPcUtJeU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TkRBeU1UUTBOakF5V2hjTk16RXdOREF4TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRRG8zazllbE5UZGJYRHRUK2VGaVpEUjYxUytncm5heS83NTVjbXRNWnZ3WlFJZ041QTJaWkh3NEU0MEN3djZZV2hJamRzTHYwSXA2bmFPa0RZenpUcG1PWUE9";
                //string secret = "+XWfmvpwPj1AYtsSgpbEL1mqkrpgLa0IYzWTFZuYL8I=";

                //mainprd
                string binarySecurityToken = "TUlJRklEQ0NCTWVnQXdJQkFnSVRYQUFDSnVWUUV1OVBSRUYvYWdBQkFBSW01VEFLQmdncWhrak9QUVFEQWpCaU1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJzd0dRWURWUVFERXhKUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFd0hoY05Nall3TkRBeU1UVXlPRE13V2hjTk16QXdOREF5TURNME1URTNXakJXTVFzd0NRWURWUVFHRXdKVFFURWdNQjRHQTFVRUNoTVhUbUZwYldGMElFRnNMVUpoYzJGeUlFOXdkR2xqWVd3eEN6QUpCZ05WQkFzVEFrbFVNUmd3RmdZRFZRUURFdzh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SURhVENDQTJVd2dkTUdBMVVkRVFTQnl6Q0J5S1NCeFRDQndqRm9NR1lHQTFVRUJBeGZNUzB5WWpCaFlUWmtNUzB6TVRNMUxUUTFPV0l0T0dVd1lTMHhZV0ZtTjJRd1l6UTBPVE44TWkwek1UQXlOVFEyTlRrM01EQXdNRE44TXkwM09EbG1ZMk0yTUMweE1UYzVMVFF3TTJFdE9ESmhZaTAxT1dJek16Z3lZelF6TkdNeEh6QWRCZ29Ka2lhSmsvSXNaQUVCREE4ek1UQXlOVFEyTlRrM01EQXdNRE14RFRBTEJnTlZCQXdNQkRFeE1EQXhEakFNQmdOVkJCb01CVWh2Wm5WbU1SWXdGQVlEVlFRUERBMVBjSFJwWTJGc0lGTjBiM0psTUIwR0ExVWREZ1FXQkJTTVUxVEpxTWYzT1AxclhNRjVvdDhPZmpaM3VEQWZCZ05WSFNNRUdEQVdnQlJaeUhLZWVUVnA2cnpabGRUWDRHUnBBZ1lRR1RDQjVRWURWUjBmQklIZE1JSGFNSUhYb0lIVW9JSFJob0hPYkdSaGNEb3ZMeTlEVGoxUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFb01Ta3NRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMRU5PUFVORVVDeERUajFRZFdKc2FXTWxNakJMWlhrbE1qQlRaWEoyYVdObGN5eERUajFUWlhKMmFXTmxjeXhEVGoxRGIyNW1hV2QxY21GMGFXOXVMRVJEUFdWNGRIcGhkR05oTEVSRFBXZHZkaXhFUXoxc2IyTmhiRDlqWlhKMGFXWnBZMkYwWlZKbGRtOWpZWFJwYjI1TWFYTjBQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMUpNUkdsemRISnBZblYwYVc5dVVHOXBiblF3Z2M0R0NDc0dBUVVGQndFQkJJSEJNSUcrTUlHN0JnZ3JCZ0VGQlFjd0FvYUJybXhrWVhBNkx5OHZRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMVU5CTEVOT1BVRkpRU3hEVGoxUWRXSnNhV01sTWpCTFpYa2xNakJUWlhKMmFXTmxjeXhEVGoxVFpYSjJhV05sY3l4RFRqMURiMjVtYVdkMWNtRjBhVzl1TEVSRFBXVjRkSHBoZEdOaExFUkRQV2R2ZGl4RVF6MXNiMk5oYkQ5alFVTmxjblJwWm1sallYUmxQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMlZ5ZEdsbWFXTmhkR2x2YmtGMWRHaHZjbWwwZVRBT0JnTlZIUThCQWY4RUJBTUNCNEF3UEFZSkt3WUJCQUdDTnhVSEJDOHdMUVlsS3dZQkJBR0NOeFVJZ1lhb0hZVFEreEtHN1owa2g4NzdHZFBBVldhSCtxVmxoZG1FUGdJQlpBSUJEakFkQmdOVkhTVUVGakFVQmdnckJnRUZCUWNEQXdZSUt3WUJCUVVIQXdJd0p3WUpLd1lCQkFHQ054VUtCQm93R0RBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dyQmdFRkJRY0RBakFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUFDaUptSGh6K2dzb0hwV0NZQXZtWDNOZm5OdzI1ZHljSkwzV2tpblVsM3JnSWdWam5wcE5Fbm9kSnl3QURrRk01N1FuVEZWcTVsaWtmS29keDZ5aCtQbUI0PQ==";
                string secret = "h13pi0OHIpDFzGcDw6AYnTuMKdrkSCCU64UbnXg/cI8=";


                string auth = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(binarySecurityToken + ":" + secret)
                );

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", auth);
                //client.DefaultRequestHeaders.Add("Accept-Version", "2.0");
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                client.DefaultRequestHeaders.Add("Accept-Language", "en");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Clearance-Status", "0"); // Only for B2B


                // ✅ BASE64 ENCODE XML
                string base64Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

                var requestBody = new
                {
                    invoiceHash = hash,
                    uuid = uuid,
                    invoice = base64Xml
                };

                string json = JsonConvert.SerializeObject(requestBody);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //"https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance/invoices",
                var response = await client.PostAsync(
                    "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/invoices/reporting/single",
                    //            "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/compliance/invoices",
                              //"https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/invoices/reporting/single",
                             // "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance/invoices",
                    content
                );
                string result = await response.Content.ReadAsStringAsync();
                //added new
                if (response.Headers.Contains("requestID"))
                {
                    var requestId = response.Headers.GetValues("requestID").FirstOrDefault();
                    Console.WriteLine("Request ID: " + requestId);
                }
                else
                {
                    Console.WriteLine("requestID header not found");
                }
                //added 
                if (!response.IsSuccessStatusCode)
                {
                    // var error = JsonConvert.DeserializeObject<dynamic>(result);

                    Console.WriteLine("STATUS: " + response.StatusCode);
                    Console.WriteLine("FULL RESPONSE: " + result);

                    throw new Exception("ZATCA ERROR:\n" + result);

                    // Return detailed error for Bad Requests so we can see EXACTLY which validation rule failed
                    //throw new Exception("ZATCA Validation Failed (Status " + response.StatusCode + "):\nResponse:\n" + result);
                }

                return  result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }
            // return await response.Content.ReadAsStringAsync();
        }




        //newcodeadded

        public static string ConvertPkcs8ToEcPrivateKey(string pkcs8Base64)
        {
            // PKCS8 bytes from ZATCA
            byte[] pkcs8Bytes = Convert.FromBase64String(pkcs8Base64);

            // Read PKCS8 structure
            var privateKeyInfo = PrivateKeyInfo.GetInstance(pkcs8Bytes);

            // Convert to EC private key parameters
            AsymmetricKeyParameter key =
                PrivateKeyFactory.CreateKey(privateKeyInfo);

            // Write EC PRIVATE KEY PEM
            using (StringWriter sw = new StringWriter())
            {
                PemWriter writer = new PemWriter(sw);
                writer.WriteObject(key);
                writer.Writer.Flush();

                return sw.ToString();
            }
        }

        public async Task<string> GenerateSimplifiedInvoiceworking(SaveSalesDetails save, int salesId)
        {
            //string certBase64 = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjFKcGYxeU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TkRBeE1UVXlOVEF6V2hjTk16RXdNek14TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSZm96QnoxQ0toL2t6OUtxUXhxaVVTRFRVd0RBMm95bTBUVmtPNUEwZDV0dTAvWEZ3S1FTeklScjkyS3owT0NBVGhza29DSk40cERhVWRZNWVuVGU1NW80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRQ202R1cvanI3RDJZc2ZpRGljMGRCUUlrNE1rT1hNR1V6ck1PNWhtbElvNkFJZ1NaSklLVExOaDBSbnkyZGpuOEtaR1dBcEhaZXd3WTZqRmRJaktQbWsyekU9";
            //string privateKeyBase64 = "MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQg26FWUc6dXAsdiVHbmm7UWcZ/nCOtObk748UfrNJjX4agBwYFK4EEAAqhRANCAARfozBz1CKh/kz9KqQxqiUSDTUwDA2oym0TVkO5A0d5tu0/XFwKQSzIRr92Kz0OCAThskoCJN4pDaUdY5enTe55";


            //string certBase64 = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjFPcUtJeU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TkRBeU1UUTBOakF5V2hjTk16RXdOREF4TWpFd01EQXdXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNBMGdBTUVVQ0lRRG8zazllbE5UZGJYRHRUK2VGaVpEUjYxUytncm5heS83NTVjbXRNWnZ3WlFJZ041QTJaWkh3NEU0MEN3djZZV2hJamRzTHYwSXA2bmFPa0RZenpUcG1PWUE9";
            string privateKeyBase64 = "MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgZ5dxHpu9v4z24twTDCcrCIG2YRcaU3teZ1h1zV8XMVKgBwYFK4EEAAqhRANCAAR0+XfABg4XOa5eiBVt2NLDEwVNxCnlO1PL+Js87ARbw3zURx7BiEGCIWnNti0VxtolBNpUWBh4mr4ov/8xBfjY";
            string certBase64 = "TUlJRklEQ0NCTWVnQXdJQkFnSVRYQUFDSnVWUUV1OVBSRUYvYWdBQkFBSW01VEFLQmdncWhrak9QUVFEQWpCaU1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJzd0dRWURWUVFERXhKUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFd0hoY05Nall3TkRBeU1UVXlPRE13V2hjTk16QXdOREF5TURNME1URTNXakJXTVFzd0NRWURWUVFHRXdKVFFURWdNQjRHQTFVRUNoTVhUbUZwYldGMElFRnNMVUpoYzJGeUlFOXdkR2xqWVd3eEN6QUpCZ05WQkFzVEFrbFVNUmd3RmdZRFZRUURFdzh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SURhVENDQTJVd2dkTUdBMVVkRVFTQnl6Q0J5S1NCeFRDQndqRm9NR1lHQTFVRUJBeGZNUzB5WWpCaFlUWmtNUzB6TVRNMUxUUTFPV0l0T0dVd1lTMHhZV0ZtTjJRd1l6UTBPVE44TWkwek1UQXlOVFEyTlRrM01EQXdNRE44TXkwM09EbG1ZMk0yTUMweE1UYzVMVFF3TTJFdE9ESmhZaTAxT1dJek16Z3lZelF6TkdNeEh6QWRCZ29Ka2lhSmsvSXNaQUVCREE4ek1UQXlOVFEyTlRrM01EQXdNRE14RFRBTEJnTlZCQXdNQkRFeE1EQXhEakFNQmdOVkJCb01CVWh2Wm5WbU1SWXdGQVlEVlFRUERBMVBjSFJwWTJGc0lGTjBiM0psTUIwR0ExVWREZ1FXQkJTTVUxVEpxTWYzT1AxclhNRjVvdDhPZmpaM3VEQWZCZ05WSFNNRUdEQVdnQlJaeUhLZWVUVnA2cnpabGRUWDRHUnBBZ1lRR1RDQjVRWURWUjBmQklIZE1JSGFNSUhYb0lIVW9JSFJob0hPYkdSaGNEb3ZMeTlEVGoxUVVscEZTVTVXVDBsRFJWTkRRVEl0UTBFb01Ta3NRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMRU5PUFVORVVDeERUajFRZFdKc2FXTWxNakJMWlhrbE1qQlRaWEoyYVdObGN5eERUajFUWlhKMmFXTmxjeXhEVGoxRGIyNW1hV2QxY21GMGFXOXVMRVJEUFdWNGRIcGhkR05oTEVSRFBXZHZkaXhFUXoxc2IyTmhiRDlqWlhKMGFXWnBZMkYwWlZKbGRtOWpZWFJwYjI1TWFYTjBQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMUpNUkdsemRISnBZblYwYVc5dVVHOXBiblF3Z2M0R0NDc0dBUVVGQndFQkJJSEJNSUcrTUlHN0JnZ3JCZ0VGQlFjd0FvYUJybXhrWVhBNkx5OHZRMDQ5VUZKYVJVbE9WazlKUTBWVFEwRXlMVU5CTEVOT1BVRkpRU3hEVGoxUWRXSnNhV01sTWpCTFpYa2xNakJUWlhKMmFXTmxjeXhEVGoxVFpYSjJhV05sY3l4RFRqMURiMjVtYVdkMWNtRjBhVzl1TEVSRFBXVjRkSHBoZEdOaExFUkRQV2R2ZGl4RVF6MXNiMk5oYkQ5alFVTmxjblJwWm1sallYUmxQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMlZ5ZEdsbWFXTmhkR2x2YmtGMWRHaHZjbWwwZVRBT0JnTlZIUThCQWY4RUJBTUNCNEF3UEFZSkt3WUJCQUdDTnhVSEJDOHdMUVlsS3dZQkJBR0NOeFVJZ1lhb0hZVFEreEtHN1owa2g4NzdHZFBBVldhSCtxVmxoZG1FUGdJQlpBSUJEakFkQmdOVkhTVUVGakFVQmdnckJnRUZCUWNEQXdZSUt3WUJCUVVIQXdJd0p3WUpLd1lCQkFHQ054VUtCQm93R0RBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dyQmdFRkJRY0RBakFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUFDaUptSGh6K2dzb0hwV0NZQXZtWDNOZm5OdzI1ZHljSkwzV2tpblVsM3JnSWdWam5wcE5Fbm9kSnl3QURrRk01N1FuVEZWcTVsaWtmS29keDZ5aCtQbUI0PQ==";


            var xmlPath = @"D:\syed_raufsirproj\invoice.xml";

            //var xmlPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Samples\Simplified\Debit\Simplified_Debit_Note.xml";


            //string xmlPath = @"D:\syed_raufsirproj\newproj\Simplified_Invoice.xml";

            //new
            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(xmlPath);
           
            UpdateInvoiceId(doc, save.SalesId);

            string dynamicXml = GenerateInvoiceXmlSection(save);

            

            var invoiceno = new StringBuilder();

            invoiceno.Append($@"<cbc:ID>{save.SalesId}</cbc:ID>");

            // ✅ Convert XML → string
            string xmlString = doc.OuterXml;


            // ✅ Replace placeholders
            xmlString = xmlString.Replace(
                "<!--INVOICE_DYNAMIC_ID-->",
                invoiceno.ToString());

            xmlString = xmlString.Replace(
                "<!--INVOICE_DYNAMIC_SECTION-->",
                dynamicXml);


            // ✅ Reload into XmlDocument (VERY IMPORTANT)
            doc.LoadXml(xmlString);

            //invoiceno.Append($@"<cbc:ID>{save.SalesId}</cbc:ID>");


            //doc = doc.Replace("<!--INVOICE_DYNAMIC_ID-->", invoiceno.ToString());

            //// ✅ STEP 3: Inject dynamic section
            //doc = doc.Replace("<!--INVOICE_DYNAMIC_SECTION-->", dynamicXml);




            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(fullxml);

            string uuid = Guid.NewGuid().ToString();

            UpdateDynamicFields(doc, uuid);



            byte[] certBytes = Convert.FromBase64String(certBase64);
            X509Certificate2 cert = new X509Certificate2(certBytes);



            

            EInvoiceHashGenerator invoicehshgen = new EInvoiceHashGenerator();

            var hashs = invoicehshgen.GenerateEInvoiceHashing(doc);

            byte[] level1 = Convert.FromBase64String(certBase64);
            string certificateContent =
                System.Text.Encoding.UTF8.GetString(level1);

            string ecPrivateKeyPem =
    ConvertPkcs8ToEcPrivateKey(privateKeyBase64);

            string cleanedprivatekey = ecPrivateKeyPem.Replace("-----BEGIN EC PRIVATE KEY-----", "")
                                      .Replace("-----END EC PRIVATE KEY-----", "")
                                      .Replace("\n", "").Replace("\r", "").Trim();

            EInvoiceSigner signer = new EInvoiceSigner();
            SignResult signResult = signer.SignDocument(doc, certificateContent, cleanedprivatekey);

            var props = signResult.GetType().GetProperties();


            var res = signResult.SignedEInvoice.OuterXml;

           var response= await SendInvoice1(uuid, hashs.Hash, res);
            var qrCode = GetQR(res);

            SaveZatcaData(
    salesId,
    uuid,
    hashs.Hash,
    doc.OuterXml,
    qrCode,
    response
);
            return "s";

        }

        string GetQR(string xmlstring)
        {
            string xml = xmlstring;   // ← your shared XML string

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            var qrValue = "";
            var qrNode = doc.SelectSingleNode(
                "//cac:AdditionalDocumentReference[cbc:ID='QR']/cac:Attachment/cbc:EmbeddedDocumentBinaryObject",
                ns
            );

            if (qrNode != null)
            {
                qrValue=qrNode.InnerText ;
            }
            return qrValue;
        }


        //newcodeadded





    }
}
