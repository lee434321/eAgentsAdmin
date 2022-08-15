using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using System.Collections;

namespace PropertyOneAppWeb.Web
{
    public partial class Statement : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "search")
                    {
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);
                            ArrayList arraySort = GlobalUtil.GetSortSenderFromBootgrid(Request);  //获取排序关键字
                            string json;
                            if (arraySort != null)
                            {
                                json = GenJsonStatement("", current, rowCount, arraySort[0].ToString(), arraySort[1].ToString());
                            }
                            else
                            {
                                json = GenJsonStatement("", current, rowCount, "", "");
                            }
                            result = json;
                        }
                        catch (Exception exc)
                        {
                            result = GlobalUtil.bootErrorStr;
                            Log.WriteLog(exc.Message + exc.StackTrace);
                        }
                    }
                    else if (action == "loadpdf")
                    {
                        Session["pdffilename"] = Request.Form["filename"];
                        Session["statementnum"] = Request.Form["statementnum"];

                        result = "ok";
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

        /// <summary>
        /// 生成对账单json数据
        /// </summary>
        public string GenJsonStatement(string statementnum, int current, int rowCount, string sortSender, string sortType)
        {
            try
            {
                ApiUtil api = new ApiUtil();
                string startdate = DateTime.Now.AddDays(-183).ToString("yyyy-MM-dd");

                string json = api.Api_GetStatement(Session["leasenumber"].ToString(), startdate, "", statementnum);
                JsonApi.JsonApi_GetStatement_Res jsonDataRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetStatement_Res>(json);
                if (jsonDataRes.result == "100")  //如果成功获取账单
                {
                    List<JsonApi.JsonApi_GetStatement_StatementInfo> list = new List<JsonApi.JsonApi_GetStatement_StatementInfo>();
                    foreach (JsonApi.JsonApi_GetStatement_StatementInfo statementinfo in jsonDataRes.statementinfo)
                    {
                        list.Add(statementinfo);
                    }

                    //设定排序
                    if (sortSender != "" && sortType != "")
                    {
                        if (sortSender == "statementnum")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.statementnum).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.statementnum).ToList();
                            }
                        }
                        else if (sortSender == "statementdate")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.statementdate).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.statementdate).ToList();
                            }
                        }
                        else if (sortSender == "paymentduedate")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.paymentduedate).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.paymentduedate).ToList();
                            }
                        }
                        else if (sortSender == "statementamount")
                        {
                            if (sortType == "asc")
                            {
                                list = list.OrderBy(o => o.statementamount).ToList();
                            }
                            else if (sortType == "desc")
                            {
                                list = list.OrderByDescending(o => o.statementamount).ToList();
                            }
                        }
                    }
                    else
                    {
                        //默认按时间排序
                        list = list.OrderByDescending(o => o.statementdate).ToList();
                    }

                    //设置翻页
                    list = GlobalUtil.PageList<Model.JsonApi.JsonApi_GetStatement_StatementInfo>(list, current, rowCount);
                    JsonApi.JsonApi_Bootgrid_SearchStatement jbs = new JsonApi.JsonApi_Bootgrid_SearchStatement();
                    jbs.current = current;
                    jbs.rowCount = rowCount;
                    jbs.rows = list;
                    jbs.total = jsonDataRes.statementinfo.Length;

                    json = JsonConvert.SerializeObject(jbs);
                    return json;
                }
                else
                {
                    throw new Exception(jsonDataRes.message + " Statement number: " + statementnum);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}