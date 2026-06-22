using Eyewa_new_api.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Eyewa_new_api.DAL
{
    public class DCheckPermission
    {
        private SQLHelper SqlObj;
        private DataSet ds;

        public DataSet GetTransaction(string SpName, Hashtable ht)
        {
            this.SqlObj = new SQLHelper();
            this.ds = new DataSet();
            this.ds = this.SqlObj.ExecuteSP(SpName, ht);
            return this.ds;
        }
    }
}
