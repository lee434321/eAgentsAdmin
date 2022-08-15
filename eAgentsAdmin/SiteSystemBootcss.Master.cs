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
using Newtonsoft.Json;

namespace PropertyOneAppWeb
{
    public partial class SiteSystemBootcss : System.Web.UI.MasterPage
    {
        public string DocumentTitle = Helper.DbConsole.GetDbCity();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Log.WriteLog("SiteSystem.master timeout");
                Response.Redirect("/LoginSystem.aspx");
                Response.End();
            }
            else if (Session["groupinfo"] == null)
            {
                Response.Redirect("/LoginSystem.aspx");
                Response.End();
            }
            else
            {
                //不需要浏览器缓存
                Response.Buffer = true;
                Response.ExpiresAbsolute = DateTime.Now - new TimeSpan(1, 0, 0);
                Response.Expires = 0;
                Response.CacheControl = "no-cache";

                //获取当前网址
                string url = Request.Url.ToString();
                Session["currenturl"] = url;
            }
        }
    }
}