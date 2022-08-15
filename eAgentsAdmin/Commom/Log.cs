using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyOneAppWeb.Commom
{
    class Log
    {
        public static void WriteLog(string logText)
        {
            string fileName = "Log_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + ".txt";

            string path = "";
            if (System.Web.HttpContext.Current == null)
                path = Bizhub.Path;
            else
                path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;

            string fileToWrite = path + @"Log\" + fileName;
            logText = "\r\n" + System.DateTime.Now.ToString() + " " + logText + " \r\n";
            System.IO.File.AppendAllText(fileToWrite, logText);
        }
    }
}
