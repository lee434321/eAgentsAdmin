using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.system;
namespace PropertyOneAppWeb
{
    /// <summary>
    /// Summary description for ajax
    /// </summary>
    public class ajax : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string t = context.Request.Url.AbsoluteUri;
            int posi = t.IndexOf("ajax.ashx");
            Aid.vPath = t.Substring(0, posi);
            Aid.yPath = context.Request.MapPath("~") + "\\";

            string action = context.Request.QueryString["act"];
            string result = "";
            if (action == "getpdfdata")
            {
                string jsonStr = Aid.Stream2Str(context.Request.InputStream);
                Req513 req = Newtonsoft.Json.JsonConvert.DeserializeObject<Req513>(jsonStr);
                Log.WriteLog("jsonStr：" + jsonStr);
                result = GetStaticHtml(req.data.operid, req.data.fmtbid);
            }
            else if (action == "demo")
            {
                result = Demo();
            }
            else
            {
                result = "action not found";
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

        private string GetStaticHtml(int operid, int fmtbid, string fmtbcode = "")
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                EformBiz50 bz3 = new EformBiz50(tx);
                rsp = bz3.GetStaticHtml2Pdf(operid);//前台调用                
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }

        private string Demo()
        {
            string result = "";

            string tstr = "";

            byte[] bs = System.Text.Encoding.Default.GetBytes(tstr);
            string[] aHex ={
                            "1b","70","00","32","ff","20","20","20","20","20","1c","21","08","1b","21","10",
                            "20","c4","fe","b2","a8","d2","bb","b5","ea","1c","21","00","1b","21","00","0a",
                            "ca","d5","d2","f8","d4","b1","a3","ba","31","30","30","31","20","c5","c6","ba",
                            "c5","a3","ba","50","30","30","31","39","0a","b5","a5","be","dd","ba","c5","a3",
                            "ba","32","30","32","31","30","37","32","39","31","34","35","34","32","34","36",
                            "35","31","30","30","31","39","0a","cf","c2","b5","a5","ca","b1","bc","e4","a3",
                            "ba","32","30","32","31","2d","30","37","2d","32","39","20","31","34","3a","35",
                            "34","3a","32","36","0a","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d",
                            "2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d",
                            "2d","2d","2d","2d","2d","0a","c9","cc","c6","b7","c3","fb","b3","c6","20","20",
                            "20","20","20","b5","a5","bc","db","20","20","ca","fd","c1","bf","20","20","20",
                            "d0","a1","bc","c6","0a","bf","a7","e0","ac","bc","a6","b7","b9","20","20","20",
                            "20","20","20","32","34","20","20","20","20","20","31","20","20","20","32","34",
                            "0d","0a","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d",
                            "2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d",
                            "2d","2d","0a","d4","ad","bc","db","a3","ba","32","34","20","20","20","20","20",
                            "20","d7","dc","ca","fd","a3","ba","31","0a","cf","d6","bc","db","a3","ba","32",
                            "34","20","20","20","20","20","20","d6","a7","b8","b6","a3","ba","cf","d6","bd",
                            "f0","3a","32","34","0a","ca","b5","ca","d5","a3","ba","32","34","20","20","20",
                            "20","20","20","d5","d2","c1","e3","a3","ba","30","0a","2d","2d","2d","2d","2d",
                            "2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d",
                            "2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","2d","20","20","20","20","0a",
                            "c9","cf","ba","a3","ca","d0","b1","a6","c9","bd","c7","f8","b8","df","be","b3",
                            "c2","b7","34","37","37","c5","aa","31","34","c2","a5","31","34","30","37","0a",
                            "20","20","20","20","20","20","bb","b6","d3","ad","b9","e2","c1","d9","a3","a1",
                            "0a","0a","0a","0a","1d","56","01","1b","42","02","01"
                           };
            bs = new byte[aHex.Length];

            List<byte> readable = new List<byte>();
            for (int i = 0; i < aHex.Length; i++)
            {
                int iv = Convert.ToInt32(aHex[i], 16);
                bs[i] = (byte)iv;

                if (iv == 10 || iv >= 32)
                {
                    readable.Add((byte)iv);
                }
            }

            tstr = System.Text.Encoding.Default.GetString(readable.ToArray());
            return result;
        }
    }

    public class ReqBase
    {
        public string from;
        public string timestamp;
        public string nonce;
        public string sign;

    }

    public class Req513 : ReqBase
    {
        public Req513_1 data = new Req513_1();
    }

    public class Req513_1
    {
        public int operid { get; set; }
        public int fmtbid { get; set; }         //geteformdata（5.3）或geteforminfo（5.7）接口可以获得
        public string fmtbcode { get; set; }    //geteforminfo（5.7）接口可以获得
    }
}