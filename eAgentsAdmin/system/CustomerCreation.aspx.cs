using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Helper;
using Newtonsoft.Json;
using System.Data;
using System.Linq.Expressions;
namespace PropertyOneAppWeb.system
{
    public partial class CustomerCreation : PageBase
    {
        static string path = "";//静态变量，保存请求路径
        protected void Page_Load(object sender, EventArgs e)
        {
            path = Request.MapPath("~");// 返回应用程序的虚拟目录（路径）
            string action = Request.Form["action"];

            string result = "";
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (Session["loginname"] == null)
            {
                Log.WriteLog("CustomerCreation timeout");
                if (string.IsNullOrEmpty(action))
                {
                    Response.Redirect("../LoginSystem.aspx");
                }
                else
                {
                    Response.Write("timeout");
                    Response.End();
                }
                return;
            }
            else if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "20"))
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            if (!string.IsNullOrEmpty(action))
            {
                if (action == "batchSendmail")
                {
                    result = BatchSendEmail();
                }
                if (action == "search")
                {
                    result = this.Search();
                }
                else if (action == "deluser")
                {
                    result = this.DeleteCustUser();
                }
                else if (action == "savenewcustuser")
                {
                    result = this.CreateCustUser();
                }
                else if (action == "searchonecustuser") ///用于编辑
                {
                    result = this.SearchOneCustUser();
                }
                else if (action == "saveeditcustuser")
                {
                    result = this.SaveEditCustUser();
                }
                else if (action == "sendemail2custuser")
                {
                    //result = this.SendEmail2CustUser();
                    result = this.SendEmail2CustUser2();
                }
                else if (action == "searchproperty")//界面隐藏，不再使用
                { }
                else if (action == "searchcustgroup") //界面隐藏，不再使用
                { }
                else if (action == "searchpropertyfromgroup")//界面隐藏，不再使用
                { }
                else if (action == "getcustname") // 根据租约号获取客户名称
                {
                    result = GetCustNameFormLease();
                }
                else if (action == "searchgrouplease") // 检索绑定的租约
                {
                    result = this.SearchGroupLease();
                }
                else if (action == "checkisadmin") //检查是否可以创建mkt和fin
                {
                    JsonRsp rsp = new JsonRsp();
                    Model.Sys_Users_Group gr = Session["groupinfo"] as Model.Sys_Users_Group;
                    if (gr.Dept.ToUpper() == "ADMIN")
                        rsp.result = true;
                    else
                        rsp.result = false;
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                }
                Response.Write(result);
                Response.End();
            }
            else
            { }
        }

        private string BatchSendEmail()
        {
            bool result = true;
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            List<Model.Sys_Login_Account> etys = new List<Sys_Login_Account>();

            //获取所有目标用户
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                etys = bz.Retrieve<Model.Sys_Login_Account>(x => x.CU_Status == "0" && x.Status == "A"); //使用lambda表达式查询
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                result = false;
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }

            //发送邮件
            if (result)
            {
                string error = "";
                int cnt = 0;
                bool check = true;
                string sysUserEmail = "";
                string roleNm = "";
                string website = System.Configuration.ConfigurationManager.AppSettings["TenantMailUrl"];
                string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
                ExchangeMail em = new ExchangeMail();
                for (int i = 0; i < etys.Count; i++)
                {
                    tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);

                        check = true;

                        // 取创建人email                       
                        string createdby = etys[i].CreateBy;
                        List<Model.Sys_Login_System> etys2 = bz.Retrieve<Model.Sys_Login_System>(x => x.LoginName == createdby);
                        if (etys2.Count <= 0)
                        {
                            check = false;
                            throw new Exception("Creator not found!");
                        }
                        sysUserEmail = etys2[0].Email;

                        // 取创建人角色                        
                        int userid = etys2[0].UserId;
                        List<Model.Sys_Login_System_Group> etys3 = bz.Retrieve<Model.Sys_Login_System_Group>(x => x.UserId == userid);
                        if (etys3.Count <= 0)
                        {
                            check = false;
                            throw new Exception("Creator userid not found!");
                        }

                        // 取角色信息(部门)               
                        int grpid = etys3[0].GroupId;
                        List<Model.Sys_Users_Group> etys4 = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == grpid);
                        if (etys4.Count <= 0)
                        {
                            check = false;
                            throw new Exception("Group not found");
                        }
                        roleNm = etys4[0].Dept;

                        // 检查邮件格式(regex)            
                        System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$");
                        if (!reg.IsMatch(etys[i].Email))
                        {
                            throw new Exception("User '" + etys[i].LoginName + "' email error [" + etys[i].Email + "]");
                        }

                        if (check)
                        {
                            string subject = "";
                            if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                                subject = "电子服务注册";
                            else
                                subject = "電子服務註冊 Registration of HutchisonAgent";

                            // 邮件正文
                            string mailbody = GenEmailBody2(website, sysUserEmail, roleNm, etys[i].LoginName);
                            if (dev == "0")
                            {
                                em.SendExchangeMail(etys[i].Email.Trim(), subject, mailbody);
                                // CU_Status变更
                                etys[i].CU_Status = Commom.CONST.CU_STATUS_SENTMAIL;
                                etys[i].UpdateDate = DateTime.Now;
                                bz.Update(etys[i]);
                            }
                            cnt++;
                        }
                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        error += err.Message + "\r\n";
                    }
                }

                rsp.result = "Total:" + etys.Count.ToString() + "; Sent:" + cnt.ToString() + (error.Length > 0 ? "\r\nError:" + error : "");
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }

        private string SearchGroupLease()
        {
            string result = "";
            int current = Int32.Parse(Request.Form["current"]);
            int rowCount = Int32.Parse(Request.Form["rowCount"]);

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                int userid = Int32.Parse(Request.Form["userid"]);
                DataTable dt = bz.RetrieveDatatable<Sys_Users_Group_Lease>(x => x.USERID == userid);
                result = GlobalUtil.GenBootGridSystem(dt, current, rowCount);
                tx.CommitTrans();
            }
            catch (Exception ex)
            {
                tx.AbortTrans();
                result = ex.Message;
            }
            return result;
        }

        private string SendEmail2CustUser2()
        {
            string result = "";
            /// 1. loginname就是leasenum
            /// 2. 生成邮件链接 from=<email 创建的系统用户>&role=<role>&leasenum=<leasenum>
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                int userid = int.Parse(Request.Form["userid"].ToString() == "" ? "0" : Request.Form["userid"].ToString());
                string loginname = Helper.Aid.DbNull2Str(Request.Form["loginName"]);

                Helper.DbConsole bz = new Helper.DbConsole(tx);

                var predicate = PredicateBuilder.True<Sys_Login_Account>();//初始化一个where字句，解析后会返回"1=1"
                if (loginname.Length == 0)  
                    throw new Exception("LoginName can not be blank.");
                else
                    predicate = predicate.And(p => p.LoginName == loginname);   //向已经初始化的predicate拼接一个“And”子句
                if (userid != 0)
                    predicate = predicate.And(p => p.UserId == userid);         //继续拼接一个“And”子句

                //predicate = predicate.And(p => p.Status == "A");
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>(predicate);
                if (etys.Count <= 0)
                    throw new Exception("Login Account not exists.");

                string leasenum = etys[0].LoginName;// 取customer user
                string email = etys[0].Email;

                // 邮件正文            
                string website = System.Configuration.ConfigurationManager.AppSettings["TenantMailUrl"];
                string mailbody = GenEmailBody2(website, "", "", leasenum);

                string subject = "";
                if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                    subject = "电子服务注册";
                else
                    subject = "電子服務註冊 Registration of HutchisonAgent";
                // 发送邮件       
                ExchangeMail em = new ExchangeMail();
                string bccs = System.Configuration.ConfigurationManager.AppSettings["bcc"].ToString();
                if (path.Substring(path.Length - 1, 1) != "\\")
                    path += "\\";
                string ap = path + @"Image\footerimage.jpg";
                if (System.IO.File.Exists(ap))
                    em.SendExchangeMail(email, subject, mailbody, bccs, ap);
                else
                    em.SendExchangeMail(email, subject, mailbody, bccs);
                result = "success";

                // CU_Status变更
                etys[0].CU_Status = Commom.CONST.CU_STATUS_SENTMAIL;
                etys[0].UpdateDate = DateTime.Now;
                bz.Update(etys[0]);

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                result = err.Message;
            }
            return result;
        }

        private string GenEmailBody2(string website, string sysUserEmail, string roleNm, string leasenum)
        {
            string str = "{\"userName\":" + "\"" + leasenum + "\",";
            str += "\"nonce\":" + "\"" + System.Guid.NewGuid().ToString().Substring(0, 7) + "\",";
            str += "\"expiredTime\":" + "1800" + ",";
            str += "\"lang\":\"en-US\"";
            str += "}";

            string encryptStr = new EncryptUtil().Encrypt(str);
            string qs_en = "&param=" + encryptStr;
            string encryptStr_cn = new EncryptUtil().Encrypt(str.Replace("en-US", "zh-CN"));
            string qs_cn = "&param=" + encryptStr_cn;

            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            string rpath = Request.MapPath(".");    //当前目录 
            xd.Load(rpath + "\\email.xml");         //加载配置文件
            System.Xml.XmlElement root = xd.DocumentElement; //获取根节点
            // 1.识别区域(hk,prc)及业务,获取模板
            System.Xml.XmlNode node = null;
            if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                node = root.SelectSingleNode("//template[@id='1']/content[@type='zh']");
            else
                node = root.SelectSingleNode("//template[@id='1']/content[@type='hk']");
            // 2.根据具体业务替换模板中变量生成邮件内容
            string body = node.InnerText.Replace("{website}", website).Replace("{leasenum}", leasenum);
            body = body.Replace("{qs_en}", qs_en).Replace("{qs_cn}", qs_cn);
            body = body.Replace("{phone1}", node.Attributes["phone1"].Value).Replace("{phone2}", node.Attributes["phone2"].Value);
            return body;
        }

        private string SaveEditCustUser()
        {
            string result = "";
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                string saveinfo = Request.Form["saveinfo"];
                string[] arraySaveInfo = saveinfo.Split('|');

                string userid = arraySaveInfo[0];
                string loginname = arraySaveInfo[1];
                string status = arraySaveInfo[2];
                string email = arraySaveInfo[3];
                string properties = arraySaveInfo[4];
                string groups = arraySaveInfo[5];
                string role = arraySaveInfo[6];
                string contactname = arraySaveInfo[7];
                string contactnbr = arraySaveInfo[8];
                string custname = arraySaveInfo[9];
                string position = arraySaveInfo[10];
                string isprimary = arraySaveInfo[11];
                
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>(x => x.UserId == Aid.Null2Int(userid));
                if (etys.Count > 0)
                {
                    if (etys[0].Status == "I")
                    {
                        //throw new Exception("Can't save , for the customer status is 'I'");
                    }
                    etys[0].LoginName = loginname;
                    etys[0].Status = status;
                    etys[0].Email = email;
                    etys[0].UpdateBy = Session["loginname"].ToString();
                    etys[0].UpdateDate = DateTime.Now;
                    etys[0].Role = role;
                    etys[0].Contact_Name = contactname;
                    etys[0].Contact_Number = contactnbr;
                    etys[0].CustName = custname;
                    etys[0].Position = position;
                    etys[0].IsPrimary = isprimary == "true" ? "Y" : "N";
                    int cnt = bz.Update(etys[0]);

                    if (cnt > 0)
                    {
                        result = "success";
                    }
                    else
                        result = "update failure!";
                }
                else
                {
                    result = "Customer user not found!";
                }
                tx.CommitTrans();
            }
            catch (Exception ex)
            {
                tx.AbortTrans();
                result = ex.Message;
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            return result;
        }

        private string Search()
        {
            string result = "";
            try
            {
                int current = Int32.Parse(Request.Form["current"]);
                int rowCount = Int32.Parse(Request.Form["rowCount"]);
                string searchPhrase = Request.Form["searchPhrase"];


                Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                oa.AddParameter("P_SEARCH_PHRASE", searchPhrase);
                oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_SYS_LOGIN_ACCOUNT");
                //过滤是mkt角色还是fin角色    
                Model.Sys_Users_Group gr = Session["groupinfo"] as Model.Sys_Users_Group;
                string p_role = "";
                switch (gr.Dept.ToUpper())
                {
                    case "EM":
                        p_role = "('E','EF','ME','X')"; // role in ('E','EF','ME','X')
                        break;
                    case "FIN":
                        p_role = "('F','MF','EF','X')"; // role in ('F','MF','EF','X')
                        break;
                    case "MKT":
                        p_role = "('M','MF','ME','X')"; // role in ('M','MF','ME','X')
                        break;
                }
                DataTable dt2 = null;
                if (gr.Dept.ToUpper() == "ADMIN")
                {
                    dt2 = dtSelect;
                }
                else
                {
                    // 取当前用户的property权限列表
                    List<Model.Sys_Users_Group_Property> etys = Bizhub.GetProperties(int.Parse(Session["groupid"].ToString()));
                    string props = "";
                    for (int i = 0; i < etys.Count; i++)
                    {
                        if ((i + 1) == etys.Count)
                            props += "'" + etys[i].PropertyCode.Trim() + "'";
                        else
                            props += "'" + etys[i].PropertyCode.Trim() + "',";
                    }
                    DataRow[] drs = dtSelect.Select("Property_Code in (" + props + ")" + " and Role in " + p_role);
                    dt2 = dtSelect.Clone();
                    for (int j = 0; j < drs.Length; j++)
                    {
                        dt2.ImportRow(drs[j]);
                    }
                }
                result = GlobalUtil.GenBootGridSystem(dt2, current, rowCount);
            }
            catch (System.Threading.ThreadAbortException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                result = GlobalUtil.bootErrorStr;
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            return result;
        }

        private string DeleteCustUser()
        {
            string result = "";
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                int userid = int.Parse(Request.Form["userid"].ToString());
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>(x => x.UserId == userid);
                List<Model.Sys_Users_Group_Lease> etys2 = bz.Retrieve<Model.Sys_Users_Group_Lease>(x => x.USERID == userid);
                int cnt = 0;
                if (etys.Count > 0)
                {
                    etys[0].Status = "I"; //"Inactive"
                    cnt = bz.Update(etys[0]);
                }

                if (cnt > 0)
                {
                    result = "success";
                }
                else
                    result = "failure";

                tx.CommitTrans();
            }
            catch (Exception ex)
            {
                tx.AbortTrans();
                result = ex.Message;
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            return result;
        }

        private string CreateCustUser()
        {
            string result = "";
            Model.Sys_Users_Group eg = Session["groupinfo"] as Model.Sys_Users_Group; //当前登录用户所属的组
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                string saveinfo = Request.Form["saveinfo"];
                string[] arraySaveInfo = saveinfo.Split('|');
                string leasenum = arraySaveInfo[0];
                string status = arraySaveInfo[1];
                string email = arraySaveInfo[2];
                string loginname = arraySaveInfo[5];
                string role = arraySaveInfo[6];
                string contactname = arraySaveInfo[7];
                string contactnbr = arraySaveInfo[8];
                string custname = arraySaveInfo[9];
                string position = arraySaveInfo[10];
                string isprimary = arraySaveInfo[11];

                /// 取当前登录的系统用户信息               
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>(x => x.LoginName == Session["loginname"].ToString());
                if (etys.Count <= 0)
                    throw new Exception("sys_login_system not found!");

                /// 写入lease_login_account表
                Model.Sys_Login_Account ety = new Sys_Login_Account();
                ety.UserId = Helper.Aid.DbNull2Int(tx.ExecuteScalar("select max(UserId) from sys_login_account")) + 1;

                /// -- start -- 这里检查当前登录的后台用户是否超过了可创建的客户数量
                string tmpr = eg.GroupName.ToUpper().Contains("FIN") ? "FIN_MAX" : eg.GroupName.ToUpper().Contains("MKT") ? "MKT_MAX" : eg.GroupName.ToUpper().Contains("EM") ? "EM_MAX" : "";

                if (tmpr == "") //这里的值是早期根据组名取得的角色(部门)信息，如果为空，则使用新的角色(部门)取值策略。
                    tmpr = eg.Dept == "FIN" ? "FIN_MAX" : eg.Dept == "MKT" ? "MKT_MAX" : eg.Dept == "EM" ? "EM_MAX" : "";

                if (!Bizhub.CheckProfile4CustCreation(Session["loginname"].ToString(), tmpr, tx, leasenum))
                    throw new Exception("You can not create customer user any more,for count limit reached!");
                /// --  end  --

                // 以下判断是否来自mkt还是fin             
                if (role != "") //如果角色值不为空，则使用之。
                {
                    ety.LoginName = loginname.Length > 0 ? loginname : leasenum + role;
                    ety.Role = role;
                }
                else //否则就判断
                {
                    Model.Sys_Users_Group gr = Session["groupinfo"] as Model.Sys_Users_Group;                    
                    List<Model.Sys_Users_Group> etys8 = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == gr.GroupId);
                    if (etys8.Count <= 0)
                        throw new Exception("sys_users_group not found!");
                    else
                    {
                        if (etys8[0].Dept != "") //如果有值，则使用新的判断角色（部门）赋值策略。
                        {
                            string tmprole = etys8[0].Dept == "MKT" ? Commom.CONST.CUSTOMER_USER_MKT_SUFFIX : etys8[0].Dept == "FIN" ? Commom.CONST.CUSTOMER_USER_FIN_SUFFIX : "";
                            if (tmprole.Length == 0)
                                throw new Exception("You can not create customer user for authority limit!");
                            ety.LoginName = loginname.Length > 0 ? loginname : leasenum + tmprole;
                            ety.Role = tmprole;
                        }
                        else
                        {
                            if (etys8[0].GroupName.ToUpper().Contains("MKT"))
                            {
                                ety.LoginName = loginname.Length > 0 ? loginname : leasenum + Commom.CONST.CUSTOMER_USER_MKT_SUFFIX; // 这里要区别是来自mkt还是fin,loginname要检查是否已经生成过
                                ety.Role = CONST.CUSTOMER_USER_MKT_SUFFIX;
                            }
                            else if (etys8[0].GroupName.ToUpper().Contains("FIN"))
                            {
                                ety.LoginName = loginname.Length > 0 ? loginname : leasenum + Commom.CONST.CUSTOMER_USER_FIN_SUFFIX; // 这里要区别是来自mkt还是fin,loginname要检查是否已经生成过
                                ety.Role = CONST.CUSTOMER_USER_FIN_SUFFIX;
                            }
                            else
                                throw new Exception("You can not create customer user for authority limit!");
                        }
                    }
                }
                // contact name 必填
                if (contactname.Length <= 0)
                {
                    throw new Exception("Contact name can not be blank!");
                }
                // 检查新增的customer loginname是否已经存在                
                List<Model.Sys_Login_Account> etys4 = bz.Retrieve<Model.Sys_Login_Account>(x=>x.LoginName==ety.LoginName);
                if (etys4.Count > 0)
                    throw new Exception("LoginName[" + ety.LoginName + "] is exists, can not create again !");

                // 检查当前用户是否可以创建指定leasenum的customer user（leasenum前4位必须与当前用户所属的property匹配）
                List<Model.Sys_Users_Group_Property> etysgp = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == eg.GroupId);
                var r = from x in etysgp
                        where x.PropertyCode.Trim() == leasenum.Substring(0, 4)
                        select x;
                if (r.Count<Model.Sys_Users_Group_Property>() <= 0)
                    throw new Exception("You can not create customer user, for the lease not match properties !");

                // 检查相同角色下的email是否存在
                List<Model.Sys_Login_Account> etys6 = bz.Retrieve<Model.Sys_Login_Account>(x => x.Role == ety.Role && x.Email == email); // 查询邮件对应的userid                                          
                if (etys6.Count > 0)
                {  
                    List<Model.Sys_Users_Group_Lease> etys7 = bz.Retrieve<Model.Sys_Users_Group_Lease>(x => x.USERID == etys6[0].UserId); //查询userid 对应的租约号 
                    if (etys7.Count > 0)
                    {
                        if (etys7[0].LEASENUMBER == leasenum) //如果与即将创建的租约号相同，则报错。（邮箱+租约必须唯一才可以创建customer user）
                        {
                            throw new Exception("Email[" + email + "] is exists, can not create again !");
                        }
                    }
                }

                //检查租约是否存在和过期
                var etys9 = bz.Retrieve<Model.Pone_Lm_Lease>(x => x.LEASE_NUMBER == leasenum);               
                if (etys9.Count > 0)
                {
                    if (etys9[0].Lease_Term_To.Year < 1900) //如果lease_term_to为空（日期类型为空时系统默认为0001-01-01）
                    { }
                    else
                    {
                        if (DateTime.Now >= etys9[0].Lease_Term_To)
                            throw new Exception("Lease_Number(" + leasenum + ") expired,can not create customer !");
                    }
                }
                else if (etys9.Count == 0)
                {
                    throw new Exception("Lease number not exists.");
                }

                //生成8位随机密码
                Random rnd = new Random();
                string psw = GlobalUtil.MakePassword(8, rnd);           
                ety.Password = psw;
                ety.Temppsw = psw;//这里使用temppsw字段保存临时密码，在用户收到邮件并注册时判断该字段，如果为空则不允许注册了修改密码了，不为空则修改密码，并置为空。
                ety.CreateBy = Session["loginname"].ToString();
                ety.Email = email;
                ety.Status = status;
                ety.CreateDate = DateTime.Now;
                ety.UpdateBy = Session["loginname"].ToString();
                ety.UpdateDate = DateTime.Now;
                ety.CU_Status = Commom.CONST.CU_STATUS_PENDING;
                ety.Contact_Name = contactname;
                ety.Contact_Number = contactnbr;
                ety.CustName = custname;
                ety.Position = position;
                ety.IsPrimary = isprimary == "true" ? "Y" : "N";
                int cnt = bz.Create<Model.Sys_Login_Account>(ety);
                if (cnt > 0)
                {                     
                    Model.Sys_Users_Group_Lease ety2 = new Sys_Users_Group_Lease();
                    ety2.USERID = ety.UserId;
                    ety2.LEASENUMBER = leasenum;
                    ety2.CREATEDATE = DateTime.Now;
                    ety2.CREATEBY = Session["loginname"].ToString();
                    ety2.CREATE_SRC = "0";
                    cnt = bz.Create(ety2);
                }
                else
                    result = "failure!";

                if (cnt > 0)
                {
                    result = "success";
                }
                else
                    result = "failure";

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                result = err.Message;
            }
            return result;
        }

        private string SearchOneCustUser()
        {
            JsonRsp rsp = new JsonRsp();
            string result = "";
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                string userid = Request.Form["userid"];
                Helper.DbConsole db = new Helper.DbConsole(tx);                
                List<Model.Sys_Login_Account> etys = db.Retrieve<Model.Sys_Login_Account>(x => x.UserId == Aid.Null2Int(userid));

                if (etys.Count > 0)
                {
                    List<Model.Sys_Users_Group_Lease> etys2 = db.Retrieve<Model.Sys_Users_Group_Lease>(x => x.CREATE_SRC == "0" && x.USERID == Aid.Null2Int(userid)); //MethodCallExpressionN
                    if (etys2.Count > 0)
                    {
                        etys[0].Create_From_Lease = etys2[0].LEASENUMBER;
                    }
                    result = JsonConvert.SerializeObject(etys[0]);
                }
                else
                {
                    result = "";
                }
                tx.CommitTrans();
            }
            catch (Exception ex)
            {
                tx.AbortTrans();
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            return result;
        }

        private string GetCustNameFormLease()
        {
            JsonRsp rsp = new JsonRsp();
            string lease_num = Request.Form["lease_num"];

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.Pone_Lm_Lease> etys = bz.Retrieve<Model.Pone_Lm_Lease>(x => x.LEASE_NUMBER == lease_num.Trim()); //InstanceMethodCallExpressionN

                if (etys.Count > 0)
                {
                    List<Model.Pone_Customer> etys2 = bz.Retrieve<Model.Pone_Customer>(x => x.CUSTOMER_NUMBER == etys[0].CUSTOMER_NUMBER); //PropertyExpression
                    if (etys2.Count > 0)
                    {
                        rsp.result = etys2[0];
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "N/A";
                    }
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "lease not found!";
                }

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();

                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }
    }    
}