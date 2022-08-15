using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Commom;
using iPGClientCOM;

namespace PropertyOneAppWeb.Web
{
    public partial class OnlinepayReady : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "payready") 
                {
                    string leasenum = Session["leasenumber"].ToString();
                    string actualamount = Session["payamount"].ToString();
                    string actualpaytype = "100";
                    Session["actualpaytype"] = actualpaytype;
                    string receivecompanycode = "receivecompanycode";
                    Session["receivecompanycode"] = receivecompanycode;
                    string currencycode = "HKD";
                    Session["currencycode"] = currencycode;
                    string bankaccount = "bankaccount";
                    Session["bankaccount"] = bankaccount;
                    string customercode = Session["custnumber"].ToString();
                    string actualpaydate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Session["actualpaydate"] = actualpaydate;
                    string status = "50"; //准备发起支付
                    List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo> actualpayinfo = Session["paydata"] as List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo>;

                    ApiUtil api = new ApiUtil();
                    string result = api.Api_SendOnlinepayResult(
                        leasenum, 
                        actualamount, 
                        actualpaytype, 
                        receivecompanycode, 
                        currencycode, 
                        bankaccount, 
                        customercode, 
                        actualpaydate, 
                        status, 
                        actualpayinfo);
                    Response.Write("ok");
                    Response.End();
                }
            }
        }
    }
}