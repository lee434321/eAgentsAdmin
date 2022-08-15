using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Commom;
using System.Collections;

namespace PropertyOneAppWeb.Web
{
    public partial class Onlinepay : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "search")
                {
                    try
                    {
                        ArrayList arraySort = GlobalUtil.GetSortSenderFromBootgrid(Request);
                        string jsonOnlinepay;
                        if (arraySort != null)
                        {
                            jsonOnlinepay = GenJsonOnlinepay("search", arraySort[0].ToString(), arraySort[1].ToString());
                        }
                        else
                        {
                            jsonOnlinepay = GenJsonOnlinepay("search", "", "");
                        }
                        Response.Write(jsonOnlinepay);
                        Response.End();
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write(GlobalUtil.bootErrorStr);
                        Response.End();
                    }
                }
                else if (action == "loadOutstandingInfo")
                {
                    string jsonOnlinepay = GenJsonOnlinepay("loadOutstandingInfo", "", "");
                    Response.Write(jsonOnlinepay);
                    Response.End();
                }
                else if (action == "goready")
                {
                    string paydata = Request.Form["paydata"];
                    string payamount = Request.Form["payamount"];
                    string[] paydatarow = paydata.Split('|');

                    Session["payamount"] = payamount;
                    Session["paydata"] = GenPayList(paydatarow);
                }
            }
        }

        /// <summary>
        /// 生成支付详情List
        /// </summary>
        public List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo> GenPayList(string[] paydatarow)
        {
            try {
                List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo> list = new List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo>();
                if (paydatarow != null && paydatarow.Length > 0)
                {
                    for (int i = 0; i < paydatarow.Length; i++)
                    {
                        string[] row = paydatarow[i].ToString().Split(',');
                        JsonApi.JsonApi_OnlinepayResult_actualpayinfo info = new JsonApi.JsonApi_OnlinepayResult_actualpayinfo();
                        info.transno = row[0].ToString();
                        info.amount = row[1].ToString();
                        info.actualpay = row[2].ToString();
                        info.chargecode = row[3].ToString();
                        info.invoicelinenum = row[4].ToString();
                        list.Add(info);
                    }
                    return list;
                }
                return null;
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// 生成待付款json数据
        /// </summary>
        public string GenJsonOnlinepay(string type, string sortSender, string sortType)
        {
            try
            {
                ApiUtil api = new ApiUtil();
                string json = api.Api_GetOnlinePayInfo(Session["leasenumber"].ToString());
                //string json = "{\r\n  \"result\": \"100\",\r\n  \"message\": \"获取数据成功\",\r\n  \"customername\": \"Chan Tai Man\",\r\n  \"shopname\": \"JD\",\r\n  \"premisename\": \"YHD\",\r\n  \"shoparea\": \"Whampoa Garden Site12 Shop G88\",\r\n  \"totalamount\": \"10360.00\",\r\n  \"payinfonum\": \"2\",\r\n  \"payinfo\": [\r\n    {\r\n      \"rowid\": \"1\",\r\n      \"transno\": \"5097-NL-00010000\",\r\n      \"chargeitem\": \"Extra A/C chg\",\r\n      \"descr\": \"2017-04—2017-05\",\r\n      \"amount\": \"360.00\",\r\n      \"outstanding\": \"360.00\",\r\n      \"duedate\": \"2017-04-31\"\r\n    },\r\n    {\r\n      \"rowid\": \"2\",\r\n      \"transno\": \"5021-NL-00010000\",\r\n      \"chargeitem\": \"Rental\",\r\n      \"descr\": \"2017-03—2017-04\",\r\n      \"amount\": \"5200.00\",\r\n      \"outstanding\": \"5200.00\",\r\n      \"duedate\": \"2017-04-31\"\r\n    }\r\n  ]\r\n}";
            
                if (type == "search")
                {
                    JsonApi.JsonApi_GetOnlinepayInfo_Res jsonDataRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetOnlinepayInfo_Res>(json);
                    List<JsonApi.JsonApi_GetOnlinepayInfo_payinfo> list = new List<JsonApi.JsonApi_GetOnlinepayInfo_payinfo>();
                    foreach (JsonApi.JsonApi_GetOnlinepayInfo_payinfo payinfo in jsonDataRes.payinfo)
                    {
                        list.Add(payinfo);
                    }

                    //设定排序
                    if (sortSender != "" && sortType != "")
                    {
                        if (sortSender == "transno")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.transno).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.transno).ToList();
                            }
                        }
                        else if (sortSender == "chargeitem")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.chargeitem).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.chargeitem).ToList();
                            }
                        }
                        else if (sortSender == "descr")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.descr).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.descr).ToList();
                            }
                        }
                        else if (sortSender == "amount")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.amount).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.amount).ToList();
                            }
                        }
                        else if (sortSender == "outstanding")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.outstanding).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.outstanding).ToList();
                            }
                        }
                        else if (sortSender == "duedate")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.duedate).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.duedate).ToList();
                            }
                        }
                    }
                    else
                    {
                        //默认按时间排序
                        list = list.OrderByDescending(o => o.duedate).ToList();  
                    }
                    
                    JsonApi.JsonApi_Bootgrid_SearchOnlinePay jbs = new JsonApi.JsonApi_Bootgrid_SearchOnlinePay();
                    jbs.current = 1;
                    jbs.rowCount = -1;
                    jbs.rows = list;
                    jbs.total = list.Count;

                    json = JsonConvert.SerializeObject(jbs);
                    
                }
                return json;
            }
            catch
            {
                return GlobalUtil.bootErrorStr;
            }
            
        }
    }
}