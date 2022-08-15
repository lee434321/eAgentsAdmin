using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using PropertyOneAppWeb.system;

namespace PropertyOneAppWeb
{
    /// <summary>
    /// getStatement 的摘要说明
    /// </summary>
    public class getStatement : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {                        
            JsonRsp rsp = new JsonRsp();
            string result = "";
            try
            {
                //localhost:64307/getStatement.ashx?date=20200721&seq=S000354845
                string date = context.Request.QueryString["date"];
                string seq = context.Request.QueryString["seq"];
                string path = context.Server.MapPath("~/"); //D:\lizw\PropertyOneAppWeb\
                string fn = path + @"Statement\" + date + @"\" + seq + ".pdf";
                if (System.IO.File.Exists(fn))
                {
                    FileStream fs = File.OpenRead(fn);
                    byte[] buff = new byte[fs.Length];
                    fs.Read(buff, 0, buff.Length);
                    string binStrData = Convert.ToBase64String(buff);
                    UploadParas data = new UploadParas();
                    data.fileName = seq + ".pdf";
                    data.base64Data = binStrData;
                    rsp.result = data;
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "statement file not exists";
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                }
            }
            catch (Exception err)
            {
                result = err.Message;
            }
            context.Response.Write(result);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}