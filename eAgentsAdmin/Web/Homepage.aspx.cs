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
    public partial class Homepage : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "getnotice")
                    {
                        string leasenum = Session["loginname"].ToString();
                        ApiUtil api = new ApiUtil();
                        string json = api.Api_GetNotice("('L')", leasenum, "", ""); //这里使用 in 语法
                        JsonApi.JsonApi_GetNotice_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetNotice_Res>(json);
                        if (res.result == "100")
                        {
                            result = json;
                        }
                        else
                        {
                            result = res.message;
                        }
                    }
                    else if (action == "changelease")
                    {
                        string leasenum = Request.Form["leasenum"];
                        Session["leasenumber"] = leasenum;

                        ApiUtil api = new ApiUtil();
                        string jsonLeaseInfo = api.Api_GetLeaseInfo(leasenum);
                        JsonApi.JsonApi_GetLeaseInfo_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetLeaseInfo_Res>(jsonLeaseInfo);
                        Session["premises"] = res.leaseinfo[0].premises.Replace("'", @"\'");
                        Session["shoparea"] = res.leaseinfo[0].premises.Replace("'", @"\'");
                        Session["custname"] = res.leaseinfo[0].custname.Replace("'", @"\'");
                        Session["custnumber"] = res.leaseinfo[0].custnum;
                        Session["homepageoutstanding"] = api.GetOutstanding(leasenum);

                        if (res.result == "100")
                        {
                            result = "ok";
                        }
                        else
                        {
                            result = res.message;
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    //do nothing
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    Log.WriteLog(ex.Message + ex.StackTrace);
                }
                finally
                {
                    Response.Write(result);
                    Response.End();
                }
            }
        }
    }
}