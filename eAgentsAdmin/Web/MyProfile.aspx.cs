using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;

namespace PropertyOneAppWeb.Web
{
    public partial class MyProfile : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (action != null && action == "search")
            {
                try
                {
                    string leasenum = Session["leasenumber"].ToString();
                    ApiUtil api = new ApiUtil();
                    string json = api.Api_GetLeaseInfo(leasenum);
                    JsonApi.JsonApi_GetLeaseInfo_Res jsonRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetLeaseInfo_Res>(json);
                    JsonApi.JsonApi_GetLeaseInfo_Leaseinfo jsonInfo = jsonRes.leaseinfo[0];
                    string res = JsonConvert.SerializeObject(jsonInfo);
                    Response.Write(res);
                    Response.End();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    //do nothing
                }
                catch
                {
                    Response.Write("");
                    Response.End();
                }
            }
        }
    }
}