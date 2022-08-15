using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyOneAppWeb.Commom
{
    public class ConnectionString
    {
        public static readonly ConnectionString Instance = new ConnectionString();
        public string dbconnect;
        string dbFlg = "MAP";
        public ConnectionString()
        {
            dbconnect = System.Configuration.ConfigurationManager.ConnectionStrings[dbFlg].ConnectionString;
        }
    }
}