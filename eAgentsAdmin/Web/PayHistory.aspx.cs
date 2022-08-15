using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Services;
using Newtonsoft.Json;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Commom;
using System.Collections;

namespace PropertyOneAppWeb.Web
{
    public partial class PayHistory : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "search")
                {
                    ArrayList arraySort = GlobalUtil.GetSortSenderFromBootgrid(Request);
                    int current = Int32.Parse(Request.Form["current"]);
                    int rowCount = Int32.Parse(Request.Form["rowCount"]);
                    string startdate = Request.Form["startdate"];
                    string enddate = Request.Form["enddate"];
                    startdate = GlobalUtil.ConvertDateType(startdate, "dd-MM-yyyy", "yyyy-MM-dd");
                    enddate = GlobalUtil.ConvertDateType(enddate, "dd-MM-yyyy", "yyyy-MM-dd");
                    string json;
                    if (arraySort != null)
                    {
                        json = SearchHistory(startdate, enddate, current, rowCount, arraySort[0].ToString(), arraySort[1].ToString());
                    }
                    else
                    {
                        json = SearchHistory(startdate, enddate, current, rowCount, "", "");
                    }
                    Response.Write(json);
                    Response.End();
                }
            }
        }

        public string SearchHistory(string startdate, string enddate, int current, int rowCount, string sortSender, string sortType)
        {
            try
            {
                ApiUtil api = new ApiUtil();
                string json = api.Api_GetPayHistory(Session["leasenumber"].ToString(), startdate, enddate);
                JsonApi.JsonApi_OnlinepayHistory_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_OnlinepayHistory_Res>(json);
                List<JsonApi.JsonApi_OnlinepayHistory_payinfo> list = new List<JsonApi.JsonApi_OnlinepayHistory_payinfo>();
                foreach (JsonApi.JsonApi_OnlinepayHistory_payinfo info in res.payinfo)
                {
                    list.Add(info);
                }

                //设定排序
                if (sortSender != "" && sortType != "")
                {
                    if (sortSender == "paydate")
                    {
                        if (sortType == "asc")
                        {
                            list = list.OrderBy(o => o.paydate).ToList();
                        }
                        else if (sortType == "desc")
                        {
                            list = list.OrderByDescending(o => o.paydate).ToList();
                        }
                    }
                    else if (sortSender == "payamount")
                    {
                        if (sortType == "asc")
                        {
                            list = list.OrderBy(o => o.payamount).ToList();
                        }
                        else if (sortType == "desc")
                        {
                            list = list.OrderByDescending(o => o.payamount).ToList();
                        }
                    }
                }
                else
                {
                    //默认按时间排序
                    list = list.OrderByDescending(o => o.paydate).ToList();
                }

                //设置翻页
                list = GlobalUtil.PageList<JsonApi.JsonApi_OnlinepayHistory_payinfo>(list, current, rowCount);

                JsonApi.JsonApi_Bootgrid_SearchPayHistory boot = new JsonApi.JsonApi_Bootgrid_SearchPayHistory();
                boot.current = current;
                boot.rowCount = rowCount;
                boot.rows = list;
                boot.total = res.payinfo.Count;

                json = JsonConvert.SerializeObject(boot);
                return json;
            }
            catch 
            {
                return GlobalUtil.bootErrorStr;
            }
        }
    }
}