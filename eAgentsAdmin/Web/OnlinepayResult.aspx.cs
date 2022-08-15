using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;

namespace PropertyOneAppWeb.Web
{
    public partial class OnlinepayResult : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string dr = Request.Form["DR"];
            string msg = Request.Form["MSG"];
            if (dr != null && msg != null && dr != "" && msg != "")
            {
                GenPayResult(dr, msg);
            }
        }

        /// <summary>
        /// 向数据库写入支付结果
        /// </summary>
        public void GenPayResult(string dr, string msg)
        {
            /*
            pps p = new pps();
            pps.PPSModel_DR mdr = new pps.PPSModel_DR();
            mdr = p.ExtractDR(dr, msg);
            if (mdr != null)
            {
                if (mdr.OpCode == "00")
                {
                    //如果是支付请求
                    string ppsreferenceno = Session["ppsreferenceno"].ToString();
                    if (mdr.StatusCode == "AP" && mdr.ReferenceNo == ppsreferenceno)
                    {
                        //如果支付成功, 而且reference no和session中的相同
                        string leasenum = Session["leasenumber"].ToString();
                        string actualamount = Session["payamount"].ToString();
                        string actualpaytype = Session["actualpaytype"].ToString();
                        string receivecompanycode = Session["receivecompanycode"].ToString();
                        string currencycode = Session["currencycode"].ToString();
                        string bankaccount = Session["bankaccount"].ToString();
                        string customercode = Session["custnumber"].ToString();
                        string actualpaydate = Session["actualpaydate"].ToString();
                        string status = "30"; //支付成功

                        string ppsmerchantid = mdr.MerchantID;
                        string ppstxcode = mdr.TxCode;
                        string ppsamount = mdr.Amount;
                        string ppsopcode = mdr.OpCode;
                        string ppspayfor = "";
                        string ppslocale = "";
                        string ppsuserdata = mdr.UserData;
                        string ppssiteid = mdr.SiteID;
                        string ppsstatuscode = mdr.StatusCode;
                        string ppsresponsecode = mdr.ResponseCode;
                        string ppsbankaccount = mdr.BankAccount;
                        string ppsvaluedate = mdr.ValueDate;
                        string ppstxdate = mdr.TxDate;
                        string ppspostid = mdr.POSTID;
                        string ppsisn = mdr.ISN;
                        string ppswholemessage = mdr.WholeMessage;
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
                            ppsreferenceno, 
                            ppsmerchantid, 
                            ppstxcode, 
                            ppsamount, 
                            ppsopcode, 
                            ppspayfor, 
                            ppslocale, 
                            ppsuserdata, 
                            ppssiteid, 
                            ppsstatuscode, 
                            ppsresponsecode, 
                            ppsbankaccount, 
                            ppsvaluedate, 
                            ppstxdate, 
                            ppspostid, 
                            ppsisn, 
                            ppswholemessage,
                            actualpayinfo);
                        if (result != null && result != "")
                        {
                            Response.Write("ok");
                            Response.End();
                        }
                        else
                        {
                            Response.Write("fail");
                            Response.End();
                        }
                    }
                }
                else if (mdr.OpCode == "03")
                {
                    //如果是支付结果查询

                }
            }

            */
             
        }
    }
}