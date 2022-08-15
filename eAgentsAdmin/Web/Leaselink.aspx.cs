using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Commom;
using Newtonsoft.Json;

namespace PropertyOneAppWeb.Web
{
    public partial class Leaselink : PageBase
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
                            string json = SearchLease();
                            result = json;
                        }
                        catch (System.Threading.ThreadAbortException)
                        {
                            //do nothing
                        }
                        catch (Exception exc)
                        {
                            result = GlobalUtil.bootErrorStr;
                            Log.WriteLog(exc.Message + exc.StackTrace);
                        }
                    }
                    else if (action == "link")
                    {
                        string loginname = Session["loginname"].ToString();
                        string type = "A";
                        string statementnum = Request.Form["statementnum"];
                        string balance = Request.Form["balance"];
                        string leasenum = Request.Form["leasenum"];
                        string expiredate = Request.Form["expiredate"];
                        string rental = Request.Form["rental"];

                        expiredate = GlobalUtil.ConvertDateType(expiredate, "dd-MM-yyyy", "yyyy-MM-dd");
                        
                        //绑定的租约号不能和登录租约相同
                        if (loginname != leasenum)
                        {
                            ApiUtil api = new ApiUtil();
                            string json = api.Api_LinkLease(loginname, type, statementnum, balance, leasenum, expiredate, rental);
                            JsonApi.JsonApi_LinkLease_Res jsonRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_LinkLease_Res>(json);
                            if (jsonRes.result == "100")
                            {
                                SearchLease();
                                result = "ok";
                            }
                            else
                            {
                                throw new Exception(jsonRes.message);
                            }
                        }
                        else
                        {
                            throw new Exception("Lease number the same as Login name");
                        }
                    }
                    else if (action == "delete")
                    {
                        string loginname = Session["loginname"].ToString();
                        string leasenum = Request.Form["leasenum"];
                        string type = "D";

                        //解除绑定的租约号不能和登录租约相同
                        if (loginname != leasenum)
                        {
                            ApiUtil api = new ApiUtil();
                            string json = api.Api_LinkLease(loginname, type, null, "0", leasenum, null, null);
                            JsonApi.JsonApi_LinkLease_Res jsonRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_LinkLease_Res>(json);
                            if (jsonRes.result == "100")
                            {
                                if (leasenum == Session["leasenumber"].ToString())   //如果删除的租约号是当前选中的租约号
                                {
                                    Session["leasenumber"] = Session["loginname"];
                                }
                                SearchLease();
                                result = "ok";
                            }
                            else
                            {
                                throw new Exception(jsonRes.message);
                            }
                        }
                        else
                        {
                            throw new Exception("Lease number the same as Login name");
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

        /// <summary>
        /// 找出所有关联的Lease Number 
        /// </summary>
        public string SearchLease()
        {
            string userName = Session["loginname"].ToString();
            string password = Session["password"].ToString();
            ApiUtil api = new ApiUtil();
            string json = api.Api_LoginCheck(userName, password, "E");
            JsonApi.JsonApi_LoginCheck_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_LoginCheck_Res>(json);
            if (res.result == "100")  //登录成功
            {
                /******************** 获取用户组 ********************/
                int leaseCount = Int32.Parse(res.leasecount);   //账号对应租约的数量
                if (leaseCount > 0)
                {
                    List<JsonApi.JsonApi_LoginCheck_Leaseinfo> list = res.leaseinfo;
                    List<DataModel.ModelLeaseGroup> leaseGroup = new List<DataModel.ModelLeaseGroup>();

                    DataModel.ModelLeaseGroup mlg = new DataModel.ModelLeaseGroup();
                    mlg.leasenum = userName;
                    mlg.outstanding = api.GetOutstanding(userName);

                    //获取premises
                    string jsonLeaseInfo = api.Api_GetLeaseInfo(userName);
                    JsonApi.JsonApi_GetLeaseInfo_Res getLeaseInfoRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetLeaseInfo_Res>(jsonLeaseInfo);
                    mlg.premises = getLeaseInfoRes.leaseinfo[0].premises;

                    leaseGroup.Add(mlg);

                    for (int i = 0; i < leaseCount; i++)
                    {
                        mlg = new DataModel.ModelLeaseGroup();
                        mlg.leasenum = list[i].leasenumber;
                        mlg.outstanding = api.GetOutstanding(list[i].leasenumber);

                        //获取premises
                        jsonLeaseInfo = api.Api_GetLeaseInfo(mlg.leasenum);
                        getLeaseInfoRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetLeaseInfo_Res>(jsonLeaseInfo);
                        mlg.premises = getLeaseInfoRes.leaseinfo[0].premises;

                        leaseGroup.Add(mlg);
                    }

                    string leasegroupjson = JsonConvert.SerializeObject(leaseGroup);
                    Session["leasegroup"] = leaseGroup;
                    Session["leasegroupjson"] = leasegroupjson.Replace("'", @"\'");
                    //Session["leasenumber"] = userName;
                }
                else if (leaseCount == 0)
                {
                    List<DataModel.ModelLeaseGroup> leaseGroup = new List<DataModel.ModelLeaseGroup>();
                    DataModel.ModelLeaseGroup mlg = new DataModel.ModelLeaseGroup();
                    mlg.leasenum = userName;
                    mlg.outstanding = api.GetOutstanding(userName);

                    //获取premises
                    string jsonLeaseInfo = api.Api_GetLeaseInfo(userName);
                    JsonApi.JsonApi_GetLeaseInfo_Res getLeaseInfoRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetLeaseInfo_Res>(jsonLeaseInfo);
                    mlg.premises = getLeaseInfoRes.leaseinfo[0].premises;

                    leaseGroup.Add(mlg);

                    string leasegroupjson = JsonConvert.SerializeObject(leaseGroup);
                    Session["leasegroup"] = leaseGroup;
                    Session["leasegroupjson"] = leasegroupjson.Replace("'", @"\'");
                    //Session["leasenumber"] = userName;
                }
                else
                {
                    throw new Exception("get linked lease count error: " + leaseCount.ToString());
                }
            }

            List<DataModel.ModelLeaseGroup> leasegroup = Session["leasegroup"] as List<DataModel.ModelLeaseGroup>;
            if (leasegroup.Count >= 1)
            {
                leasegroup.RemoveAt(0);
            }

            JsonApi.JsonApi_Bootgrid_SearchLeaseGroup bootgridLeaseGroup = new JsonApi.JsonApi_Bootgrid_SearchLeaseGroup();
            bootgridLeaseGroup.current = 1;
            bootgridLeaseGroup.rowCount = -1;
            bootgridLeaseGroup.rows = leasegroup;
            bootgridLeaseGroup.total = leasegroup.Count;
            return JsonConvert.SerializeObject(bootgridLeaseGroup);
        }
    }
}