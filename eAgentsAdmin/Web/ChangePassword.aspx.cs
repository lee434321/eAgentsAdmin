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
    public partial class ChangePassword : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";

                try
                {
                    if (action == "change")
                    {
                        string pswold = Request.Form["pswold"];
                        string pswnew = Request.Form["pswnew"];
                        string userid = Session["loginname"].ToString();
                        string loginType = Session["logintype"].ToString();

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_ChangePsw(userid, loginType, pswold, pswnew);
                        JsonApi.JsonApi_ChangePsw_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_ChangePsw_Res>(json);
                        if (res.result == "100")
                        {
                            result = "ok";
                            Session["password"] = pswnew;
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