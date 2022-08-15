using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ftpa
{
    public class Common
    {
        public static string FtpDbConnStr { get { return System.Configuration.ConfigurationManager.AppSettings["pmsftp_connStr"]; } }
        
        public Common()
        {
            //FtpDbConnStr = System.Configuration.ConfigurationManager.AppSettings["pmsftp_connStr"];
        }
    }
    public class Util
    {
    }
}