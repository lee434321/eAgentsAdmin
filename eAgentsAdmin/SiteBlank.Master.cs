using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PropertyOneAppWeb
{
    public partial class SiteBlank : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /* 不需要浏览器缓存 */
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now - new TimeSpan(1, 0, 0);
            Response.Expires = 0;
            Response.CacheControl = "no-cache"; 
        }
    }
}