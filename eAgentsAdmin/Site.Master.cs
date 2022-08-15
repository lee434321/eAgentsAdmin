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
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //不需要浏览器缓存
            /*
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            */
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