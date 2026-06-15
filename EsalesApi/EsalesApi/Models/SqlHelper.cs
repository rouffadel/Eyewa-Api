using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace EsalesApi.Models
{
    public class SQLHelper
    {
        private SqlConnection _Con;
        private OleDbConnection _ConAccess;
        private bool _IsCommandExecuted = false;

        public SQLHelper() => this.DefaultCon();

        public SqlConnection Con => this._Con;

        public DataSet ExecuteQueries(string Queries)
        {
            DataSet dataSet = new DataSet();
            SqlCommand selectCommand = new SqlCommand(Queries, this._Con);
            this._Con.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
            try
            {
                sqlDataAdapter.Fill(dataSet);
                this._IsCommandExecuted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlDataAdapter.Dispose();
                selectCommand.Dispose();
                this._Con.Close();
            }
            return dataSet;
        }

        public object CheckPermission(string RoleId, string ScreenCode)
        {
            try
            {
                Hashtable Params = new Hashtable();
                string str = "  Org.OrganizationRoleId like '" + RoleId + "' and SM.ScreenCode  like '" + ScreenCode + "'";
                Params.Add((object)"@WhereCondition", (object)str);
                Params.Add((object)"@Transaction", (object)"GetAllScreensAndRoles");
                object obj = new object();
                return (object)this.ExecuteSP("sp_getdatascreenmaster", Params);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        public Hashtable ExecuteSingleRecord(string Query)
        {
            Hashtable hashtable = new Hashtable();
            SqlCommand sqlCommand = new SqlCommand(Query, this._Con);
            this._Con.Open();
            try
            {
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                if (sqlDataReader.Read())
                {
                    for (int ordinal = 0; ordinal < sqlDataReader.FieldCount; ++ordinal)
                        hashtable.Add((object)sqlDataReader.GetName(ordinal), sqlDataReader[ordinal]);
                }
                this._IsCommandExecuted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlCommand.Dispose();
                this._Con.Close();
            }
            return hashtable;
        }

        public bool HasRecords(string Query)
        {
            bool flag = false;
            SqlCommand sqlCommand = new SqlCommand(Query, this._Con);
            this._Con.Open();
            try
            {
                flag = sqlCommand.ExecuteReader().HasRows;
                this._IsCommandExecuted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlCommand.Dispose();
                this._Con.Close();
            }
            return flag;
        }

        public bool ExecuteNonQuery(string Query)
        {
            bool flag = false;
            SqlCommand sqlCommand = new SqlCommand(Query, this._Con);
            this._Con.Open();
            try
            {
                flag = sqlCommand.ExecuteNonQuery() > 0;
                this._IsCommandExecuted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlCommand.Dispose();
                this._Con.Close();
            }
            return flag;
        }

        public DataTable TableRetrieval(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                SqlCommand selectCommand = new SqlCommand();
                selectCommand.CommandType = CommandType.Text;
                selectCommand.Connection = this._Con;
                selectCommand.CommandText = query;
                this._Con.Open();
                new SqlDataAdapter(selectCommand).Fill(dataTable);
            }
            catch (SystemException ex)
            {
                throw ex;
            }
            finally
            {
                this._Con.Close();
            }
            return dataTable;
        }

        public string ExecuteScalar(string Query)
        {
            string empty = string.Empty;
            SqlCommand sqlCommand = new SqlCommand(Query, this._Con);
            this._Con.Open();
            try
            {
                object obj = sqlCommand.ExecuteScalar();
                if (obj != null)
                    empty = obj.ToString();
                this._IsCommandExecuted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlCommand.Dispose();
                this._Con.Close();
            }
            return empty;
        }

        public DataSet ExecuteSP(string SPName, Hashtable Params)
        {
            DataSet dataSet = new DataSet();
            SqlCommand selectCommand = new SqlCommand();
            selectCommand.Connection = this._Con;
            this._Con.Open();
            try
            {
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.CommandText = SPName;
                selectCommand.CommandTimeout = 100000;
                foreach (DictionaryEntry dictionaryEntry in Params)
                {
                    SqlParameter sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = dictionaryEntry.Key.ToString();
                    sqlParameter.Value = dictionaryEntry.Value;
                    selectCommand.Parameters.Add(sqlParameter);
                }
                new SqlDataAdapter(selectCommand).Fill(dataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                selectCommand.Dispose();
                this._Con.Close();
            }
            return dataSet;
        }

        public DataSet ExecuteSP(string SPName, SqlParameterCollection Params)
        {
            DataSet dataSet = new DataSet();
            SqlCommand selectCommand = new SqlCommand();
            this._Con.Open();
            try
            {
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.CommandText = SPName;
                selectCommand.Connection = this._Con;
                foreach (SqlParameter sqlParameter in (DbParameterCollection)Params)
                    selectCommand.Parameters.Add(sqlParameter);
                new SqlDataAdapter(selectCommand).Fill(dataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                selectCommand.Dispose();
                this._Con.Close();
            }
            return dataSet;
        }

        public DataSet ExecuteGetSP(string SPName, string Cond, string str1)
        {
            DataSet dataSet = new DataSet();
            SqlCommand selectCommand = new SqlCommand();
            selectCommand.Connection = this.Con;
            this.Con.Open();
            try
            {
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.CommandText = SPName;
                string[] strArray1 = Cond.Split('$');
                string[] strArray2 = str1.Split('$');
                for (int index = 0; index < strArray1.Length; ++index)
                {
                    selectCommand.Parameters.Add(strArray2[index].ToString(), SqlDbType.NVarChar, 10);
                    selectCommand.Parameters[index].Value = (object)strArray1[index];
                }
                new SqlDataAdapter(selectCommand).Fill(dataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                selectCommand.Dispose();
                this.Con.Close();
            }
            return dataSet;
        }

        public DataSet ExecuteGetVendorsSP(string SPName, string Cond, string str1)
        {
            DataSet dataSet = new DataSet();
            SqlCommand selectCommand = new SqlCommand();
            selectCommand.Connection = this.Con;
            this.Con.Open();
            try
            {
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.CommandText = SPName;
                string[] strArray1 = Cond.Split('$');
                string[] strArray2 = str1.Split('$');
                for (int index = 0; index < strArray1.Length; ++index)
                {
                    selectCommand.Parameters.Add(strArray2[index].ToString(), SqlDbType.NVarChar, 10);
                    selectCommand.Parameters[index].Value = (object)strArray1[index];
                }
                new SqlDataAdapter(selectCommand).Fill(dataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                selectCommand.Dispose();
                this.Con.Close();
            }
            return dataSet;
        }

        public void Close()
        {
            if (this._Con.State != ConnectionState.Open)
                return;
            this._Con.Close();
        }

        private void DefaultCon() => this._Con = new SqlConnection(ConfigurationManager.ConnectionStrings["eSaleConstr"].ConnectionString);

        public string SmsFunction(string Numbers, string SmsText)
        {
            Hashtable Params = new Hashtable();
            Params.Add((object)"@Transaction", (object)"GETSMSDETAILS");
            string str1 = "sd.CURRENTPREFRENCE=1";
            Params.Add((object)"@wherecondition", (object)str1);
            DataSet dataSet1 = new DataSet();
            DataSet dataSet2 = this.ExecuteSP("Sp_GetData", Params);
            string str2 = dataSet2.Tables[0].Rows[0]["SMSURL"].ToString();
            string str3 = dataSet2.Tables[0].Rows[0]["SMSUSERNAME"].ToString();
            string str4 = dataSet2.Tables[0].Rows[0]["SMSPASSWORD"].ToString();
            string str5 = dataSet2.Tables[0].Rows[0]["SMSFROM"].ToString();
            string str6 = SmsText;
            string str7 = Numbers;
            return SQLHelper.GetPageContent(str2 + "username=" + str3 + "&password=" + str4 + "&to=" + str7 + "&text=" + str6 + "&from=" + str5);
        }

        private static string GetPageContent(string FullUri)
        {
            try
            {
                CookieContainer cookieContainer = new CookieContainer();
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(FullUri);
                httpWebRequest.CookieContainer = cookieContainer;
                return new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }
    }
}