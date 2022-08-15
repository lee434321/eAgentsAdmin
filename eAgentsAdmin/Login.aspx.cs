using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Commom;
using System.Collections;
using System.Threading;
using System.Globalization;

namespace PropertyOneAppWeb
{
    public partial class Login : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string loginType = System.Configuration.ConfigurationManager.AppSettings["logintype"].ToString();
            if (Session["logintype"] == null)
            {
                Session["logintype"] = loginType;
            }
            
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "check")
                    {
                        string userName = Request.Form["username"];
                        string password = Request.Form["password"];
                        string lang = Request.Form["lang"];
                        loginType = Request.Form["logintype"];

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_LoginCheck(userName, password, loginType);
                        JsonApi.JsonApi_LoginCheck_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_LoginCheck_Res>(json);
                        if (res.result == "100")  //登录成功
                        {
                            Session.Add("preferredculture", lang);
                            Session["userid"] = res.userid;
                            Session["loginname"] = userName;
                            Session["password"] = password;
                            Session["logintype"] = loginType;
                            if (!String.IsNullOrEmpty(res.lastlogin))
                            {
                                Session["lastlogin"] = res.lastlogin.ToString() + " HKT";
                            }
                            else
                            {
                                Session["lastlogin"] = "First time login";
                            }

                            if (loginType == "C")        //如果是账号登录
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
                                    leaseGroup.Add(mlg);

                                    for (int i = 0; i < leaseCount; i++)
                                    {
                                        mlg = new DataModel.ModelLeaseGroup();
                                        mlg.leasenum = list[i].leasenumber;
                                        mlg.outstanding = api.GetOutstanding(list[i].leasenumber);
                                        leaseGroup.Add(mlg);
                                    }

                                    string leasegroupjson = JsonConvert.SerializeObject(leaseGroup);
                                    Session["leasegroup"] = leaseGroup;
                                    Session["leasegroupjson"] = leasegroupjson.Replace("'", @"\'");
                                    Session["leasenumber"] = userName;
                                }
                                else if (leaseCount == 0)
                                {
                                    List<DataModel.ModelLeaseGroup> leaseGroup = new List<DataModel.ModelLeaseGroup>();
                                    DataModel.ModelLeaseGroup mlg = new DataModel.ModelLeaseGroup();
                                    mlg.leasenum = userName;
                                    mlg.outstanding = api.GetOutstanding(userName);
                                    leaseGroup.Add(mlg);

                                    string leasegroupjson = JsonConvert.SerializeObject(leaseGroup);
                                    Session["leasegroup"] = leaseGroup;
                                    Session["leasegroupjson"] = leasegroupjson.Replace("'", @"\'");
                                    Session["leasenumber"] = userName;
                                }
                                else
                                {
                                    throw new Exception("get linked lease count error: " + leaseCount.ToString());
                                }
                            }

                            string status = res.status;
                            if (status == "A")  //正常登录
                            {
                                System.Web.Security.FormsAuthentication.SetAuthCookie(userName, false);
                                result = "ok";
                            }
                            else if (status == "F")  //首次登录
                            {
                                result = "first";
                            }
                            else
                            {
                                throw new Exception("Login status error: " + status);
                            }
                        }
                        else
                        {
                            throw new Exception("Login error: " + res.message);
                        }
                    }
                    else if (action == "loadUserInfo")
                    {
                        string leasenum = Session["leasenumber"].ToString();
                        ApiUtil api = new ApiUtil();
                        if (leasenum.Contains("F")||leasenum.Contains("M"))
                        {
                            leasenum = leasenum.Substring(0, leasenum.Length - 1);
                        }
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
                    else if (action == "logout")
                    {
                        System.Web.Security.FormsAuthentication.SignOut();
                        Session.Clear();
                        Session.Abandon();

                        result = "ok";
                    }
                    else if (action == "changepsw")
                    {
                        string newpsw = Request.Form["newpsw"];

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_ChangePsw(Session["loginname"].ToString(), Session["logintype"].ToString(), Session["password"].ToString(), newpsw);

                        JsonApi.JsonApi_ChangePsw_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_ChangePsw_Res>(json);
                        if (res.result == "100")
                        {
                            System.Web.Security.FormsAuthentication.SetAuthCookie(Session["loginname"].ToString(), false);
                            Session["password"] = newpsw;
                            result = "ok";
                        }
                        else
                        {
                            result = res.message;
                        }
                    }
                    else if (action == "changelang")  //切换语言
                    {
                        string lang = Request.Form["lang"];
                        Session.Add("preferredculture", lang);

                        if (Session["preferredculture"] == null)
                        {
                            Session.Add("preferredculture", "en-US");
                        }
                        string UserCulture = Session["preferredculture"].ToString();
                        if (UserCulture != "")
                        {
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo(UserCulture);
                        }

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
            
            if (!IsPostBack)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                AddSession();
            }
        }

        //添加session
        private void AddSession()
        {
            /* 登录信息 */
            Session.Add("logintype", "");                 //用户登录类型  系统用户(system) or 普通用户

            /* 待支付信息 */
            Session.Add("paydata", "");                   //待支付信息
            Session.Add("payamount", "");                 //用户期望支付总额
            Session.Add("actualpaytype", "");             //实际支付类型100-PPS 200-支付宝 300-银联 400-微信支付 900-其他支付方式
            Session.Add("receivecompanycode", "");        //收款公司
            Session.Add("currencycode", "");              //币别
            Session.Add("bankaccount", "");               //银行账号
            Session.Add("actualpaydate", "");             //实际支付时间

            /* 对账单信息 */
            Session.Add("pdffilename", "");               //对账单文件名
            Session.Add("statementnum", "");              //对账单号

            /* 存储用户信息 */
            Session.Add("leasenumber", "");               //租约号
            Session.Add("userid", "");                    //登录名对应的userid
            Session.Add("loginname", "");                 //登录名
            Session.Add("password", "");                  //登录密码
            Session.Add("leasegroup", "");                //账户登录对应的租约组List
            Session.Add("leasegroupjson", "");            //账户登录对应的租约组json
            Session.Add("premises", "");                  //premises
            Session.Add("shoparea", "");                  //商铺地址
            Session.Add("custname", "");                  //客户名字
            Session.Add("custnumber", "");                //客户号
            Session.Add("lastlogin", "");                 //最后登录日期
            Session.Add("accountcustname", "");           //账号登录的用户姓名
            Session.Add("homepageoutstanding", "");       //主页上Outstanding显示的数值

            /* pps信息 */
            Session.Add("ppsreferenceno", "");            //如果是PPS支付，表示16位DO唯一标识

            /* 系统信息 */
            Session.Add("feedbacktype", "");              //Feedback类型
            Session.Add("currenturl", "");                //当前页面网址
        }
    }
}