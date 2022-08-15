using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace PropertyOneAppWeb.Commom
{
    public class GlobalUtilSystem
    {
        //本地数据库连接
        public static string sdb_local = ConfigurationManager.AppSettings["ConnStrLocal"].ToString();
    }    
}