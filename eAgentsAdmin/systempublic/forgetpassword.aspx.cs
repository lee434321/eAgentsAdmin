using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
using System.Data;

namespace PropertyOneAppWeb.systempublic
{
    public partial class forgetpassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "getcode")
                    {
                        Session.Add("loginname", "");
                        Session.Add("vcode", "");
                        Session.Add("vmail", "");
                        Session.Add("oldpsw", "");

                        string loginName = Request.Form["loginname"];

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_LOGINNAME", loginName);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_BY_LOGINNAME");

                        if (dtSelect != null && dtSelect.Rows.Count == 1)
                        {
                            //生成6位随机验证码
                            Random rnd = new Random();
                            string vCode = GlobalUtil.MakePassword(6, rnd);
                            string email = dtSelect.Rows[0]["EMAIL"].ToString();

                            Session["loginname"] = loginName;
                            Session["vcode"] = vCode;
                            Session["vmail"] = email;

                            ExchangeMail em = new ExchangeMail();
                            //string html = GetMailBody(vCode);
                            string html = GenEmailBody(vCode, Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["WebsiteAddress"]));
                            if (html=="")                            
                                html = GetMailBody(vCode);
                            
                            if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")                            
                                em.SendExchangeMail(email, "重置密码", html);                            
                            else
                                em.SendExchangeMail(email, "重置密碼 Reset Password of HutchisonAgent", html);
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception("Lease number not found");
                        }
                    }
                    else if (action == "changepsw")
                    {
                        string loginName = Session["loginname"].ToString();
                        string vCode = Helper.Aid.DbNull2Str(Request.Form["code"]);
                        string newpsw = Request.Form["newpsw"];
                        string sVCode = Session["vcode"].ToString();
                        string email = Session["vmail"].ToString();

                        if (vCode == sVCode)
                        {
                            //如果验证码正确, 开始更改密码
                            Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                            oa.AddParameter("P_LOGINNAME", loginName);
                            oa.AddParameter("P_PSW", GlobalUtil.GenerateMD5(newpsw.Trim()));
                            oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                            string ret = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.UPDATE_PSW_BY_LOGINNAME").ToString();

                            if (ret == "ok")
                            {
                                result = "ok";
                            }
                            else
                            {
                                throw new Exception(ret);
                            }
                        }
                        else
                        {
                            throw new Exception("Verification code error");
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

        private string GetMailBody(string vCode)
        {
            string body = "<html><body>";
            body += "Dear User," + "<br/>";
            body += "<br/>";
            body += "Please find below the verification code for password reset in our application:www.hutchisoneagents.com" + "<br/>";
            body += "<br/>";
            body += "Verification code:" + vCode + "<br/>";
            body += "<br/>";
            body += "Yours faithfully," + "<br/>";
            body += "<br/>";
            body += "Hutchison Estate Agents Limited" + "<br/>";
            body += "<br/>";
            body += "<br/>";
            body += "親愛的租戶:" + "<br/>";
            body += "<br/>";
            body += "請在系統使用以下驗證碼重新設定密碼:www.hutchisoneagents.com" + "<br/>";
            body += "<br/>";
            body += "驗證碼:" + vCode + "<br/>";
            body += "<br/>";
            body += "和記地產代理有限公司謹啟" + "<br/>";
            body += "<br/>";
            body += "</body></html>";            
            return body;
        }

        private string GenEmailBody(string vCode, string website)
        {
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            string rpath = Request.MapPath("..");    //当前目录 

            if (!System.IO.File.Exists(rpath + "\\system" + "\\email.xml"))
                return "";
                        
            xd.Load(rpath+"\\system" + "\\email.xml");         //加载配置文件
            System.Xml.XmlElement root = xd.DocumentElement; //获取根节点
            // 1.识别区域(hk,prc)及业务,获取模板
            System.Xml.XmlNode node = null;
            if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                node = root.SelectSingleNode("//template[@id='2']/content[@type='zh']");
            else
                node = root.SelectSingleNode("//template[@id='2']/content[@type='hk']");
            string link = string.Format("<a href='{0}'>link</a>", website);
            // 2.根据具体业务替换模板中变量生成邮件内容Genbody( paras....)
            string body = node.InnerText.Replace("{vCode}", vCode).Replace("{website}", link);
            return body;
        }
    }
}