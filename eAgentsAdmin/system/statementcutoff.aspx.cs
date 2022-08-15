using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
namespace PropertyOneAppWeb.system
{
    /// <summary>
    ///  Statement Date Setting页面
    /// </summary>
    /// 
    /// Change log:    
    /// 1.新增表“T_Statement_Date_Property”
    /// create table T_Statement_Date_Property(
    ///   Id integer primary key,
    ///   Statement_Date_Id integer,
    ///   Property_Code varchar2(10),
    ///   Last_Update date
    /// )
    public partial class statementcutoff : PageBase
    {
        static int sid = 0;// 静态变量，用来向子线程传递数据
        static string path = "";//静态变量，保存请求路径
        string rpath = "";//当前请求物理路径

        protected void Page_Load(object sender, EventArgs e)
        {
            rpath = Request.MapPath(".");
            path = Request.MapPath("~/");
            string action = Request.Form["action"];
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (Session["loginname"] == null)
            {
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
            else if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "23")) // 如果没有权限
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "enquirystatementdate")   //查找Statement date
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.Output);
                        DataTable dt = (DataTable)oa.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_SELECT_STATEMENT_DATE");
                        /// -- start -- 注意：执行“PROC_SELECT_STATEMENT_DATE”的结果集中日期是经过处理的（dd--mm-yyyy）格式，
                        /// 这种处理方式不通用，所以要变为标准格式。历史原因，不再做优化了。
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string t = dt.Rows[i]["statement_date"].ToString();
                            string[] a = t.Split('-');
                            dt.Rows[i]["statement_date"] = a[2] + '-' + a[1] + "-" + a[0];
                        }
                        /// --  end  --
                        result = GlobalUtil.GenBootGridSystem(dt, current, rowCount);
                    }
                    else if (action == "newstatementdate")  //新增Statement date
                    {
                        DateTime statementDate = GlobalUtil.ConvertStrToDateTime(Request.Form["statementdate"].ToString(), "dd-MM-yyyy");
                        string isSendMail = Request.Form["issendmail"].ToString();

                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_STATEMENT_DATE", statementDate);
                        oh.AddParameter("P_MAIL", isSendMail);
                        oh.AddParameter("P_CREATEBY", Session["loginname"].ToString());
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                        result = oh.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_ADD_STATEMENT_DATE").ToString();
                        if (result != "ok")
                        {
                            throw new Exception(result);
                        }
                    }
                    else if (action == "delstatementdate")  //删除Statement date
                    {
                        int statementDateId = Int32.Parse(Request.Form["id"]);

                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_ID", statementDateId);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                        result = oh.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_DEL_STATEMENT_DATE").ToString();
                        if (result != "ok")
                        {
                            throw new Exception(result);
                        }
                    }
                    else if (action == "updatestatementdate")  //更新 Statement date
                    {
                        int statementDateId = Int32.Parse(Request.Form["id"]);
                        string isMail = Request.Form["ismail"]; //Y,N

                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_ID", statementDateId);
                        oh.AddParameter("P_MAIL", isMail);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                        result = oh.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_UPDATE_STATEMENT_DATE").ToString();
                        if (result != "ok")
                        {
                            throw new Exception(result);
                        }

                        if (isMail == "Y") //如果需要发送邮件
                        {
                            this.StartEmailThread();
                            sid = statementDateId;
                        }
                    }
                    else if (action == "openexceptionlist")  //打开Exception List页面
                    {
                        string id = Request.Form["id"];
                        string date = Request.Form["date"];
                        Session["statementdateid"] = id;
                        Session["statementdate"] = date;
                        result = "ok";
                    }
                    else if (action == "loadproperties")
                    {
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);

                            Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                            oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                            DataTable dtSelect = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_PROPERTY");
                            result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                        }
                        catch (System.Threading.ThreadAbortException)
                        {
                            //do nothing
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(ex.Message + ex.StackTrace);
                            throw new Exception(GlobalUtil.bootErrorStr);
                        }
                    }
                    else if (action == "getproperties")
                    {
                        result = JsonConvert.SerializeObject(this.GetPropsBySid(int.Parse(Request.Form["sid"])));
                    }
                    else if (action == "saveproperties")
                    {
                        string[] props = Request.Form["saveinfo"].Split(',');
                        result = this.SaveProps(int.Parse(Request.Form["sid"]), props);
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
        /// 获取指定statementid的记录
        /// </summary>
        private JsonRsp GetPropsBySid(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.T_Statement_Date_Property> etys = bz.Retrieve<Model.T_Statement_Date_Property>(x => x.Statement_Date_Id == id); //tested
                rsp.result = etys;

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }
            return rsp;
        }

        private string SaveProps(int sid, string[] props)
        {
            string result = "";
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.T_Statement_Date_Property> etys = bz.Retrieve<Model.T_Statement_Date_Property>(x => x.Statement_Date_Id == sid);//tested
                for (int i = 0; i < etys.Count; i++)
                    bz.Delete(etys[i]);

                for (int j = 0; j < props.Length; j++)
                {
                    Model.T_Statement_Date_Property ety = new T_Statement_Date_Property();
                    ety.PROPERTY_CODE = props[j];
                    ety.Statement_Date_Id = sid;
                    bz.Create(ety);
                }
                result = "ok";

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                result = err.Message;
            }
            return result;
        }

        private void SendEmail()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<T_Statement_Date_Property> etys = bz.Retrieve<T_Statement_Date_Property>(x => x.Statement_Date_Id == sid);//tested
                string props = "(";
                if (etys.Count > 0)
                {
                    for (int i = 0; i < etys.Count; i++)
                        props += "'" + etys[i].PROPERTY_CODE.Trim() + "',";
                    props = props.Substring(0, props.Length - 1);
                }
                else
                    props += "''";
                props += ")";

                string sql = "select distinct email  from sys_login_account a, sys_users_group_lease b";
                sql += " where a.userid = b.userid";
                sql += " and trim(b.leasenumber) not in";
                sql += " (select trim(lease_number) from t_statement_date_exception)";
                sql += " and a.role in ('F','MF','EF','X')";  //限定只有财务角色可以收到邮件。
                sql += " and b.leasenumber not in (select lease_number from pone_lm_lease pll where nvl(pll.lease_term_to,sysdate+1)<=add_months(sysdate,-2))";
                sql += " and a.status='A' ";
                sql += " and substr(b.leasenumber,1,4) in " + props;

                DataTable dt = tx.ExecuteDataTable(sql);

                string siteUrl = System.Configuration.ConfigurationManager.AppSettings["TenantMailUrl"];
                string link = string.Format("<a href={0}>{1}</a>", "'" + siteUrl + "'", "HutchisonAgent");

                string body = "";
                body = GenEmailBody(link);
                if (body == "")
                    body = GenEmailBodyRaw(link);

                ExchangeMail em = new ExchangeMail();
                string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
                int userid = int.Parse(Session["userid"].ToString());
                string ap = path + @"Image\footerimage.jpg";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    System.Threading.Thread.Sleep(1500);
                    string to = dt.Rows[i]["email"].ToString();
                    if (dev == "0")
                    {
                        if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                            em.SendExchangeMail(dt.Rows[i]["email"].ToString(), "电子月结单提示", body, "", ap);
                        else
                            em.SendExchangeMail(dt.Rows[i]["email"].ToString(), "電子月結單提示 E-statement Alert of HutchisonAgent", body, "", ap);
                        Log.WriteLog("statementcutoff业务" + "向" + to + "发送了邮件！（实际发送）");
                    }
                    else
                    {
                        if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                            em.SendExchangeMail("liwei@hwpg.com", "电子月结单提示", body, "", ap);
                        else
                            em.SendExchangeMail("liwei@hwpg.com", "電子月結單提示 E-statement Alert of HutchisonAgent", body, "", ap);
                        Log.WriteLog("statementcutoff业务" + "向测试邮箱发送了邮件！（实际发送）");
                    }

                    DataModel.SysLogModel ety = new DataModel.SysLogModel();
                    ety.CreateDate = DateTime.Now;
                    ety.UserId = userid;
                    ety.Msg = "statementcutoff业务" + "向" + to + "发送了邮件！";
                    ety.Type = 23;
                    ety.LogId = Convert.ToInt32(tx.ExecuteScalar("select max(logid) from sys_log")) + 1;
                    bz.Create(ety);
                }

                for (int j = 0; j < etys.Count; j++)
                {
                    etys[j].Email_Sent = "Y";
                    bz.Update(etys[j]);
                }
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
                System.Threading.Thread.CurrentThread.Abort();
            }
        }

        private string GenEmailBody(string link)
        {
            try
            {
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                xd.Load(rpath + "\\email.xml");         //加载配置文件
                System.Xml.XmlElement root = xd.DocumentElement; //获取根节点
                // 1.识别区域(hk,prc)及业务,获取模板
                System.Xml.XmlNode node = null;
                if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                    node = root.SelectSingleNode("//template[@id='6']/content[@type='zh']");
                else
                    node = root.SelectSingleNode("//template[@id='6']/content[@type='hk']");
                // 2.根据具体业务替换模板中变量生成邮件内容
                string body = node.InnerText.Replace("{link}", link);
                body = body.Replace("{phone1}", node.Attributes["phone1"].Value);
                return body;
            }
            catch (Exception err)
            {
                Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                return "";
            }
        }

        private string GenEmailBodyRaw(string link)
        {
            string body = "<html><body>";
            body += "Dear Tenant," + "<br/>";
            body += "<br/>";
            body += "The latest eStatement for your account is now available." + "<br/>";

            body += "Please logon to " + link + " to view your eStatement for future reference." + "<br/>";
            body += "Please check your eStatement carefully and report to us if there are any errors or discrepancies." + "<br/>";
            body += "<br/>";
            body += "If you haven’t finished the user registration of our electronic services, please complete it at the earliest." + "<br/>";
            body += "<br/>";
            body += "This e-mail serves as notification only, please do not reply to this email. If you change your email address, please notify us immediately by changing it through the website. If you have any enquiries on managing eStatement, please call us at  2128 0066 during office hours." + "<br/>";
            body += "<br/>";
            body += "Thank you for using our eStatement Service." + "<br/>";
            body += "<br/>";
            body += "Yours faithfully,";
            body += "<br/>";
            body += "Hutchison Estate Agents Limited" + "<br/>";
            body += "<br/>";
            body += "<br/>";

            body += "親愛的租戶:" + "<br/>";
            body += "<br/>";
            body += "您的最新電子月結單已經上載至我們的網站。請即登錄 " + link + " 查看。" + "<br/>";
            body += "請仔細檢查您的電子月結單，如果有任何錯誤或差異，請告知我們。" + "<br/>";
            body += "<br/>";
            body += "如果您尚未完成登記電子月結單服務，請儘快完成。" + "<br/>";
            body += "<br/>";
            body += "這只是通告電郵，請不要回覆此電郵。如果您更改您的電子郵件地址，請即時透過我們的網站更改，以便知悉。如您有任何疑問，請在辦公時間致電2128 0066給我們。" + "<br/>";
            body += "<br/>";
            body += "感謝您使用我們的電子月結單服務。" + "<br/>";
            body += "<br/>";

            body += "和記地產代理有限公司 謹啟" + "<br/>";
            body += "</body></html>";
            return body;
        }

        private void StartEmailThread()
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(SendEmail));
            t.Name = "StartEmailThread";
            t.Start();
        }
    }
}