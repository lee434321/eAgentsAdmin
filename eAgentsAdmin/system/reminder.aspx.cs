using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
using System.Data;
namespace PropertyOneAppWeb.system
{
    public partial class reminder : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Helper.Aid.DbNull2Str(Request.QueryString["act"]);
            if (Session["loginname"] == null)
            {
                if (action == "")
                {
                    Response.Redirect("../LoginSystem.aspx");
                    Response.End();
                }
                else
                {
                    action = "timeout";
                }
            }

            if (!string.IsNullOrEmpty(action))
            {
                int id;
                JsonRsp rsp = new JsonRsp();
                string result = "";
                switch (action)
                {
                    case "del":
                        id = int.Parse(Request.Form["id"].ToString());
                        result = this.Delete(id);
                        break;
                    case "search":
                        string sp = Request.Form["searchParse"].ToString();
                        int pidx = int.Parse(Request.Form["pageIdx"].ToString());
                        int size = int.Parse(Request.Form["pageSize"]);                       
                        result = this.Search(sp, pidx, size);
                        break;
                    case "save":
                        result = this.Save();
                        break;
                    case "getGroup":
                        result = this.GetGroup();
                        break;
                    default:
                        break;
                }
                Response.Write(result);
                Response.End();
            }
        }

        private string Delete(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<ReminderModel> etys = bz.Retrieve<ReminderModel>(x => x.Id == id); //tested
                if (etys.Count > 0) 
                {
                    bz.Delete(etys[0]);
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = string.Format("id '{0}' not found.", id.ToString());
                    rsp.result = false;
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

        private string Search(string criteria, int pidx, int pageSize)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {                
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<ReminderModel> etys = bz.Retrieve<ReminderModel>(x => true); //tested
                rsp.result = this.SetPager(etys, pidx, pageSize);
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

        private string Save()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                ReminderModel ety = new ReminderModel();
                int cnt = 0;
                string act = Request.Form["action"];

                string[] props = Request.Form["Props"].Split(',');
                if (act == "new")
                {
                    ety.Create_By = Session["loginname"].ToString();
                    ety.Create_Date = DateTime.Now;
                    ety.Is_Email = Convert.ToBoolean(Request.Form["Is_Email"]) ? "Y" : "N";
                    ety.Properties = Request.Form["Props"];
                    ety.Rmd_Date = DateTime.Parse(Request.Form["Date"].ToString());
                    cnt = bz.Create(ety);
                    rsp.result = true;
                }
                else
                {
                    List<ReminderModel> etys = bz.Retrieve<ReminderModel>(x => x.Id == int.Parse(Request.Form["Id"])); //tested
                    if (etys.Count > 0)
                    {
                        ety = etys[0];
                        ety.Update_By = Session["loginname"].ToString();
                        ety.Update_Date = DateTime.Now;
                        ety.Is_Email = Convert.ToBoolean(Request.Form["Is_Email"]) ? "Y" : "N";
                        ety.Properties = Request.Form["Props"];
                        ety.Rmd_Date = DateTime.Parse(Request.Form["Date"].ToString());
                        cnt = bz.Update(ety);
                        rsp.result = true;                        
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "reminder not found.";
                        rsp.result = false;                        
                    }
                }
                tx.CommitTrans();

                if (ety.Is_Email=="Y")
                {
                    this.SendEmail(ety);
                }
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = false;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private DataTable PrepareReminder(ReminderModel ety)
        {
            string sql = "select a.reminder_number, a.reminder_date, sum(a.outstanding_amount) as total_reminder_amount";
            sql += ",b.customer_number, b.site_number, b.lease_number";
            sql += " from ar_reminder_letter a, ar_invoice b";
            sql += " where a.invoice_number = b.invoice_number";
            sql += " and substr(a.reminder_number, 1, 2) = 'R1'";
            sql += " and a.active = 'A'";
            sql += " and to_char(a.reminder_date, 'YYYY-MM-DD') = '" + Helper.Aid.Date2Str(ety.Rmd_Date, Helper.Aid.DateFmt.Standard) + "'"; //yyyy/mm/dd
            sql += " and substr(b.lease_number,1,4) in " + "('" + ety.Properties.Replace(",", "','") + "')";
            sql += " group by a.reminder_number, a.reminder_date, b.customer_number, b.site_number, b.lease_number";

            string poneConnStr = System.Configuration.ConfigurationManager.AppSettings["oracleConnString"];
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, poneConnStr, false, true);
            try
            {
                DataTable dt = tx.ExecuteDataTable(sql);
                tx.CommitTrans();
                return dt;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                throw;
            }
        }

        private void SendEmail(ReminderModel ety)
        {
            /*
        	    查询是否有催款信
                select distinct reminder_date from ar_reminder_letter
                where substr(reminder_number, 1, 2) = 'R1'
                and active = 'A'
                and to_char(reminder_date, 'YYYY/MM') = '2020/01'
             */

            string sql = "select a.email from sys_login_account a";
            sql += " where userid in (select userid";
            sql += "             from sys_users_group_lease b";
            sql += "      where leasenumber in ({0})";          //条件 'CKCT00349','WG0200358'
            sql += "  and create_src = '0'";
            sql += " and substr(b.leasenumber,1,4) in ({1})";   //条件 'CKCT','WG02'
            sql += " ) ";
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                
                DataTable dt = PrepareReminder(ety);
                string leases = "";
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["lease_number"].ToString().Length>0)
                        {
                            leases += "'" + dt.Rows[i]["lease_number"].ToString() + "',";    
                        }
                    }
                    if (leases.Length>0)
                    {
                        leases = leases.Substring(0, leases.Length - 1);    
                    }
                }                

                string props = "'" + ety.Properties.Replace(",", "','") + "'";

                DataTable etys = tx.ExecuteDataTable(string.Format(sql, leases, props));

                /// -- start -- 开始发送邮件
                List<string> tos = new List<string>();
                for (int j = 0; j < etys.Rows.Count; j++)
                {
                    if (tos.Contains(etys.Rows[j]["Email"].ToString()))
                    {
                        continue;
                    }
                    tos.Add(etys.Rows[j]["Email"].ToString());
                }

                ExchangeMail em = new ExchangeMail();
                string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
               
                string body = GenEmailBody();
                if (dev == "0")
                {
                    em.SendExchangeMail(tos, "電子催繳通知提示  eReminder Alert of HutchisonAgent", body);
                    Log.WriteLog("Send eMail to " + FlatArray(tos.ToArray()));
                }
                else
                {
                    em.SendExchangeMail("liwei@hwpg.com", "電子催繳通知提示  eReminder Alert of HutchisonAgent" , body);
                    Log.WriteLog("Send eMail to liwei@hwpg.com");
                }
                /// --  end  --
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
            }

        }

        private string FlatArray(string[] arr)
        {
            if (arr.Length > 0)
            {
                string result = "";
                for (int i = 0; i < arr.Length; i++)
                {
                    result += "'" + arr[i].Trim() + "',";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
            return "";
        }

        private string GenEmailBody()
        {
            string body = "Dear Tenant," + "<br/>";
            body += "<br/>";
            body += "The latest eReminder for your account is now available." + "<br/>";
            body += "Please logon to <a href='www.hutchisoneagents.com'>www.hutchisoneagents.com</a> to view your eReminder. Please check your eReminder carefully and report to us if there are any errors or discrepancies." + "<br/>";
            body += "<br/>";
            body += "We would request your settlement of the outstanding balance immediately by return. Balance overdue will be subjected to an interest charge." + "<br/>";
            body += "<br/>";
            body += "Please disregard this eReminder if the above sum has been paid in full." + "<br/>";
            body += "<br/>";
            body += "This e-mail services as notification only. Please do not reply to this email. If you change your email address, please notify us immediately by changing it through the application, If you have any enquiries on managing eReminder, please call us at 2128 0066 during office hours.";
            body += "<br/>";
            body += "Thank you for using eReminder Services." + "<br/>";
            body += "<br/>";
            body += "Yours faithfully," + "<br/>";
            body += "Hutchison Estate Agents Limited " + "<br/>";
            body += "<br/>";
            body += "親愛的租戶:" + "<br/>";
            body += "您的電子催繳通知已經上載至我們的網站。請即登錄<a href='www.hutchisoneagents.com'>www.hutchisoneagents.com</a>查看。請仔細檢查您的電子催繳通知，如果有任何錯誤或差異，請向我們報告。" + "<br/>";
            body += "<br/>";
            body += "本公司特此通知 貴公司儘快繳交欠款，逾期付款將需繳付利息。" + "<br/>";
            body += "<br/>";
            body += "如貴公司已全數付清欠款，則無須理會此通知。" + "<br/>";
            body += "<br/>";
            body += "這只是通告電郵，請不要回覆此電郵。如果您更改您的電子郵件地址，請即時透過我們的系統更改，以便知悉。如您有任何疑問，請在辦公室時間致電 2128 0066 給我們。" + "<br/>";
            body += "<br/>";
            body += "感謝您使用我們的電子催繳通知服務。" + "<br/>";
            body += "<br/>";
            body += "和記地產代理有限公司 謹啟" + "<br/>";
            return body;
        }

        private string GetGroup()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                int grpid = int.Parse(Session["groupid"].ToString());
                List<Model.Sys_Users_Group> etys = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == grpid); //tested
                if (etys.Count > 0)
                {
                    List<Model.Sys_Users_Group_Property> etys2 = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == grpid); //tested
                    GroupInfo grp = new GroupInfo();
                    grp.group = etys[0];
                    grp.props = etys2;
                    rsp.result = grp;
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = string.Format("Group id '{0}' not found.", grpid.ToString());
                    rsp.result = null;
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

    public class ReminderModel
    {
        public const string TABLE_NAME = "T_Reminder";

        [DbField(true,true)]
        public int Id { get; set; }
        public DateTime Rmd_Date { get; set; }
        public string Is_Email { get; set; }
        public string Create_By { get; set; }
        public DateTime Create_Date { get; set; }
        public string Update_By { get; set; }
        public DateTime Update_Date { get; set; }
        public string Properties { get; set; }
    }
}