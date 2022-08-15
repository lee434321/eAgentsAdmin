using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Globalization;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
namespace PropertyOneAppWeb.system
{
    public partial class Homepage : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Helper.Aid.Null2Str(Request.Form["action"]);
            if (!string.IsNullOrEmpty(action))
            {
                JsonRsp rsp = new JsonRsp();
                try
                {
                    if (action == "lz77")
                    {
                        string filePath = Helper.Aid.Null2Str(Request.Form["filePath"]);
                        if (!System.IO.File.Exists(filePath))
                        {
                            rsp.err_code = -1;
                            rsp.err_msg = "file not found[" + filePath + "]";
                        }
                        else
                        {
                            Helper.LZ77 lz7 = new Helper.LZ77();
                            Commom.Log.WriteLog("-- start -- compress");
                            lz7.Compress(filePath);
                            System.IO.Path.GetFullPath(filePath);
                        }
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "action not found.";
                    }                   
                }
                catch (Exception err)
                {
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;                    
                }

                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                Response.End();
            }
            else
            {
                string t = Request.Url.AbsoluteUri;
                int posi = t.IndexOf("system/Homepage.aspx");
                Helper.Aid.vPath = t.Substring(0, posi);
                Helper.Aid.yPath = MapPath("~/");

                //string lang = Aid.DbNull2Str(Session["lang"]);
                //lang = lang == "" ? Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) : lang;
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);            
                /*
                 *  Session["ReturnUrl"] = Request.QueryString["ReturnUrl"];
                 */
                if (Session["ReturnUrl"] != null)
                {
                    string retunlink = Session["ReturnUrl"].ToString();
                    Session["ReturnUrl"] = null;

                    //这里判断一下是哪个界面，可能需要动态调整querystring参数
                    if (retunlink.Contains("eoper.aspx")) //如果是跳转到eoper.aspx页
                    {
                        //1. 取formid指定的formname
                        //2. &operid,&formid,&loginname,&groupname,&refno &fwdeseq
                        //retunlink =retunlink.Replace("eoper.aspx","tpls4form/FormA"
                        //http://172.21.112.74:91/system/eoper.aspx?id=67
                        //http://172.21.112.74:91/system/tpls4form/FormA.html?operid=41&formid=328&loginname=kin&groupname=EM WG02 Group&refno=Form_A_41&fwdeseq=10&pidx=1
                    }

                    Response.Redirect(retunlink);
                }
            }            
        }
    }
}