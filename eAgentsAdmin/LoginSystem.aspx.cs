using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Threading;
using System.Globalization;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.system;
namespace PropertyOneAppWeb
{
    /// <summary>
    /// 登录
    /// </summary>
    /// bug fix：
    /// 1.检查逻辑修改，action有值时，避免输出整个前端页面. @2020-06-04
    public partial class LoginSystem : PageBase
    {
        public static Model.Sys_Users_Group GroupInfo = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            string action =Helper.Aid.Null2Str(Request.Form["action"]);

            if (!IsPostBack && action == "")
            {
                try
                {
                    string t = Request.Url.AbsoluteUri;
                    int posi = t.IndexOf("LoginSystem");
                    Helper.Aid.vPath = t.Substring(0, posi);
                    Helper.Aid.yPath = MapPath("~/");
                }
                catch (Exception)
                { }
            }

            if (string.IsNullOrEmpty(Bizhub.Path))
                Bizhub.Path = Request.PhysicalApplicationPath;

            string loginType = System.Configuration.ConfigurationManager.AppSettings["logintype"].ToString();
            if (Session["logintype"] == null)
            {
                Session["logintype"] = loginType;
            }

            if (Helper.Aid.DbNull2Str(Request.QueryString["ReturnUrl"]) != "")//记录登录前的地址，用于登录后重定向，通常是来自邮件的各link的直接访问
            {
                Session["ReturnUrl"] = Request.QueryString["ReturnUrl"];
            }

            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "check")
                    {
                        string userName = Request.Form["username"];
                        string password = Request.Form["password"].Trim();
                        //string lang = Request.Form["lang"];

                        List<Model.SysLoginSystemModel> etys = this.GetSysUser();

                        if (etys.Count == 0)
                            result = string.Format("LoginName [{0}] not found!", userName);
                        else if (etys[0].Password != password && etys[0].Password != GlobalUtil.GenerateMD5(password))
                            result = "Wrong password!";
                        else if (etys.Count == 1)
                        {
                            Session["website"] = System.Configuration.ConfigurationManager.AppSettings["WebsiteAddress"].ToString();
                            //Session["preferredculture"] = lang;
                            Session["loginname"] = etys[0].LoginName;
                            Session["password"] = password;
                            Session["userid"] = etys[0].UserId.ToString(); //當前登錄的userid；
                            System.Web.Security.FormsAuthentication.SetAuthCookie(userName, false);

                            //获取该用户权限列表
                            OracleEnquiry oe = new OracleEnquiry();
                            string jsonAuthList = oe.GetAuthListJson(userName);
                            Session["authlist"] = jsonAuthList;

                            result = "ok";
                            //throw new Exception("interrupt intentional");
                        }
                        else
                        {
                            result = "Login failure!";
                        }

                        // cookie
                        HttpCookie hc = new HttpCookie("login");
                        string who = hc.Values.Get("who");
                        if (Helper.Aid.DbNull2Str(who) == "")
                        {
                            hc.Values.Add("who", userName);
                            hc.Values.Add("guid", Guid.NewGuid().ToString());
                            hc.Expires = DateTime.Now.AddDays(7);
                        }
                        else
                        { }
                        Response.Cookies.Add(hc);         
                    }
                    else if (action == "loadsysuserinfo")
                    {
                        /// -- start --
                        Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                        try
                        {
                            Helper.DbConsole bz = new Helper.DbConsole(tx);                            
                            List<Model.Sys_Users_Group> etys = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == Aid.Null2Int(Request.Form["groupid"]));
                            if (etys.Count > 0)
                            {
                                Session["groupinfo"] = etys[0];
                                Session["groupname"] = etys[0].GroupName;
                                GroupInfo = etys[0];
                            }

                            /// -- 这里变更authlist    
                            // 1.取原来的authlist
                            string originlist = Session["authlist"].ToString();
                            DataModel.ModelUserAuthList ety = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel.ModelUserAuthList>(originlist);

                            // 2.重新构建新authlist
                            ety.authInfo.Clear();                            
                            List<Model.Sys_Users_Group_Authority> etys2 = bz.Retrieve<Model.Sys_Users_Group_Authority>(x => x.GroupId == etys[0].GroupId);

                            for (int i = 0; i < etys2.Count; i++)
                            {
                                DataModel.ModelUserAuthList_AuthInfo ety2 = new DataModel.ModelUserAuthList_AuthInfo();
                                ety2.authId = etys2[i].AuthTypeId.ToString();
                                ety.authInfo.Add(ety2);
                            }
                            Session["authlist"] = Newtonsoft.Json.JsonConvert.SerializeObject(ety);
                            Session["groupid"] = etys[0].GroupId; // 这里保存当前登录用户的groupid至session，用于各界面的property权限过滤。
                            if (etys[0].GroupName.ToUpper().Contains("ADMIN") || etys[0].Dept.ToUpper() == "ADMIN")
                                Session["admingrpid"] = etys[0].GroupId;
                            
                            tx.CommitTrans();
                        }
                        catch (Exception err)
                        {
                            tx.AbortTrans();
                            Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                        }
                        /// --  end  --

                        result = "ok";
                    }
                    else if (action == "logout")
                    {
                        System.Web.Security.FormsAuthentication.SignOut();
                        Session.Clear();
                        Session.Abandon();

                        result = "ok";
                    }
                    else if (action == "getgroup")
                    {
                        JsonRsp rsp = new JsonRsp();
                        Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                        try
                        {
                            Helper.DbConsole bz = new Helper.DbConsole(tx);
                            string sql = "select a.userid,";
                            sql += "    a.loginname,b.groupid,c.groupname";
                            sql += "    from sys_login_system a";
                            sql += "    inner join sys_login_system_group b";
                            sql += "    on a.userid = b.userid";
                            sql += "    inner join sys_users_group c";
                            sql += "    on b.groupid = c.groupid";
                            sql += "    where a.Status <> 'D'";
                            sql += "    and upper(trim(loginname))='" + Request.Form["username"].ToUpper().Trim() + "'";
                            sql += " order by c.groupname";
                            DataTable dt = bz.RetrieveDataTable(sql);
                            tx.CommitTrans();
                            rsp.result = dt;
                        }
                        catch (Exception err)
                        {
                            tx.AbortTrans();
                            rsp.err_code = -1;
                            rsp.err_msg = err.Message;
                        }
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    }
                    else
                    {
                        result = "action not found!";
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    //do nothing
                }
                catch (Exception err)
                {
                    result = err.Message;
                    Log.WriteLog(err.Message + "\r\n" + err.Source + "\r\n" + err.StackTrace);
                }
                Response.Write(result);
                Response.End();
            }
            else
            {
                string lang = Request.QueryString["lang"];
                if (!string.IsNullOrEmpty(lang))
                    Session.Add("lang", lang);
                else
                {
                    lang = Helper.Aid.DbNull2Str(Session["lang"]);
                    if (string.IsNullOrEmpty(lang))
                    {
                        lang = Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]);
                        lang = lang == "" ? "en-US" : lang;
                        Session.Add("lang", lang);
                    }
                }
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

                if (!IsPostBack)
                {
                    System.Web.Security.FormsAuthentication.SignOut();
                    AddSession();
                }
            }
        }

        private List<Model.SysLoginSystemModel> GetSysUser()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false,true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                string username = Request.Form["username"].Trim().ToLower();
                List<Model.SysLoginSystemModel> etys = bz.Retrieve<Model.SysLoginSystemModel>(x => x.LoginName == username);
                tx.CommitTrans();

                return etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                throw new Exception(err.Message + "\r\n" + err.Source + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// 检查是否超过指定月份数没有登录，并发送邮件。
        /// </summary>
        /// <param name="cnt"></param>
        private void CheckMonth(int cnt)
        { }

        //添加session
        private void AddSession()
        {
            /* 登录信息 */
            Session.Add("preferredculture", "");          //设置语言
            //Session.Add("logintype", "");               //用户登录类型  系统用户(system) or 普通用户
            Session.Add("loginname", "");                 //登录名
            Session.Add("password", "");                  //登录密码
            Session.Add("currenturl", "");                //当前页面网址
            Session.Add("authlist", "");                  //权限列表 json类型
            Session.Add("website", "");                   //前台网址，目前是指手机端的url
            Session.Add("statementdateid", "");           //打开Exception List页面用到的id
            Session.Add("statementdate", "");             //打开Exception List页面用到的Statement date
            Session.Add("websiteaddrmobile", "");         //前台手机端站点，如：http://172.21.112.74:92          
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.T_Feedback_Type> etys = bz.Retrieve<Model.T_Feedback_Type>(PredicateBuilder.True<Model.T_Feedback_Type>());
                Session.Add("feedbacktype", Newtonsoft.Json.JsonConvert.SerializeObject(etys));
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Source + "\r\n" + err.Message + "\r\n" + err.StackTrace);
            }

            /// -- start -- 虚拟根路径
            string sv1 = Request.ServerVariables["APPL_MD_PATH"];   //eg. /LM/W3SVC/3/ROOT            
            string sv2 = Request.ServerVariables["HTTP_HOST"];      //eg. 172.21.112.74:91
            string sv3 = Request.ServerVariables["HTTPS"];
            string vpathroot = sv3 == "off" ? "http://" : "https://";
            string[] a = sv1.Split('/');           
            if (a[a.Length - 1] == "ROOT") //如果最后一个是ROOT
            {
                vpathroot += sv2 + "/";
            }
            else 
            {
                string tstr = "";
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] == "ROOT")
                    {
                        tstr = a[i + 1];
                        break;
                    }
                }
                int posi = sv1.IndexOf(tstr);
                tstr = sv1.Substring(posi);
                vpathroot += sv2 + "/" + tstr + "/"; 
            }
            Session.Add("vpathroot", vpathroot);         //虚拟路径根目录
            /// --  end  --
        }        
    }    
}
/*
 * 
 */