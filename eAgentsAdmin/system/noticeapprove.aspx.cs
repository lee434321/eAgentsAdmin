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
    public partial class noticeapprove : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "12"))
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "search")
                {
                    try
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);
                        string searchPhrase = Request.Form["searchPhrase"];
                        
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_LOGINNAME", Session["loginname"].ToString());
                        oa.AddParameter("P_APPROVE", 'I');
                        if (searchPhrase != null && searchPhrase != "")
                        {
                            oa.AddParameter("P_SEARCH_PHRASE", searchPhrase);
                        }
                        else
                        {
                            oa.AddParameter("P_SEARCH_PHRASE", "");
                        }
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelectNotice = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_NOTICE_BY_LOGINNAME_AP");

                        dtSelectNotice = GetNotices();
                        /// -- start -- 根据当前登录用户的groupid来过滤property。                        
                        //Clause c=new Clause();
                        //c.Add("GroupId",int.Parse(Session["groupid"].ToString()));
                        //DataTable dt2 = Bizhub.FilterDatatable<Model.Sys_Users_Group_Property>(c, dtSelectNotice, "PropertyCode", "PropertyCode");
                        /// --  end  --
                        
                        if (dtSelectNotice != null)
                        {
                            //if (Session["lang"].ToString() == "en-US")
                            //{
                            //    for (int i = 0; i < dtSelectNotice.Rows.Count; i++)
                            //    {
                            //        dtSelectNotice.Rows[i]["startdate"] = Helper.Aid.Date2Str(DateTime.Parse(dtSelectNotice.Rows[i]["startdate"].ToString()), Helper.Aid.DateFmt.Dmy);
                            //        dtSelectNotice.Rows[i]["enddate"] = Helper.Aid.Date2Str(DateTime.Parse(dtSelectNotice.Rows[i]["enddate"].ToString()), Helper.Aid.DateFmt.Dmy);
                            //    }
                            //}
                            //else
                            //{
                            //    for (int j = 0; j < dtSelectNotice.Rows.Count; j++)
                            //    {
                            //        dtSelectNotice.Rows[j]["startdate"] = Helper.Aid.Date2Str(DateTime.Parse(dtSelectNotice.Rows[j]["startdate"].ToString()), Helper.Aid.DateFmt.Standard);
                            //        dtSelectNotice.Rows[j]["enddate"] = Helper.Aid.Date2Str(DateTime.Parse(dtSelectNotice.Rows[j]["enddate"].ToString()), Helper.Aid.DateFmt.Standard);
                            //    }
                            //}

                            string json = GlobalUtil.GenBootGridSystem(dtSelectNotice, current, rowCount);
                            Response.Write(json);
                            Response.End();
                        }
                        else
                        {
                            Response.Write(GlobalUtil.bootErrorStr);
                            Response.End();
                        }
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
                else if (action == "approve")
                {
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);                       
                        // 找到具有指定property的所有前台用户
                        string sql = "select distinct loginname,registration_id from sys_login_account where userid in";
                        sql += " (select userid from sys_users_group_lease a";
                        sql += " left join pone_lm_lease b   on a.leasenumber = b.lease_number";
                        sql += " where substr(leasenumber, 1, 4) in {0} and create_src = '0' and nvl(b.lease_term_to,sysdate-1) > add_months(sysdate, -2))";
                        List<string> crc = new List<string>();
                        string url = System.Configuration.ConfigurationManager.AppSettings["transUrl"] + "?act=pushnoti";
                        //string url = System.Configuration.ConfigurationManager.AppSettings["PushUrl"];//172.21.111.108:836
                        
                        string sendStr = Request.Form["sendStr"];
                        string[] arraySendStr = sendStr.Split('|');
                        string approveList = arraySendStr[1].ToString();

                        if (approveList != null && approveList != "")
                        {
                            string[] arrayApprove = approveList.Split(',');
                            foreach (string i in arrayApprove)//遍历所有选中的通知
                            {
                                List<Model.Sys_Login_Account> tos = new List<Sys_Login_Account>(); //模板消息的接受用户(后期逻辑要调整)

                                Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                                oa.AddParameter("P_NOTICEID", Int32.Parse(i));
                                oa.AddParameter("P_APPROVEBY", Session["loginname"]);
                                oa.AddParameter("P_APPROVEDATE", DateTime.Now);
                                oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                                string ret = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.UPDATE_NOTICE_APPROVE").ToString();

                                /// -- start -- 如果含有附件 则要发送邮件及推送通知
                                /// 1.根据id找到notice
                                /// 2.取出property
                                /// 3.取得具有该property权限的前台用户
                                ///   select distinct email from sys_login_account where userid in
                                ///     (select userid from sys_users_group_lease a
                                ///         left join pone_lm_lease b   on a.leasenumber = b.lease_number
                                ///       where substr(leasenumber, 1, 4) = 'CKCT'  and create_src = '0' and b.lease_term_to > add_months(sysdate, -2))
                                ///       
                                List<string> props = new List<string>();
                                string result="";
                                //bz.WhereClauses.Add("NoticeId", new Helper.Evaluation(Helper.SqlOperator.EQ, Int32.Parse(i)));
                                List<T_Notice> etys = bz.Retrieve<T_Notice>(x => x.NoticeId == Int32.Parse(i)); //tested
                                if (etys.Count>0)
                                {
                                    if (!props.Contains(etys[0].PropertyCode.Trim()))                                    
                                        props.Add(etys[0].PropertyCode.Trim());
                                    
                                    string propsArr = "(";
                                    for (int j = 0; j < props.Count; j++)
                                        propsArr += "'" + props[j] + "',";
                                    propsArr = propsArr.Substring(0, propsArr.Length - 1);
                                    propsArr += ")";

                                    string t = "";
                                    DataTable dt = tx.ExecuteDataTable(string.Format(sql, propsArr)); //获取该通知所属的前台用户
                                    for (int k = 0; k < dt.Rows.Count; k++)
                                    {
                                        string regid = dt.Rows[k]["Registration_Id"].ToString();
                                        if (regid.Trim()!="")
                                        {
                                            if (crc.Contains(regid))
                                            {
                                                continue;
                                            }
                                            string nt = etys[0].Type;
                                            t = nt == "S" ? "Notice" : nt == "P" ? "Event" : nt == "C" ? "Carpark Info" : nt == "V" ? "Service" : "";
                                            result = HttpUtil.PushNotification(url, "Notice Update", "New " + t + ":" + etys[0].Title, dt.Rows[k]["Registration_Id"].ToString());
                                            Log.WriteLog("推送结果(接口返回" + result + ") " + regid + ":" + etys[0].Title);
                                            crc.Add(regid);
                                        }

                                        /// -- start -- 收集前台用户                                        
                                        List<Model.Sys_Login_Account> etysls = bz.Retrieve<Model.Sys_Login_Account>(x => x.LoginName == dt.Rows[k]["loginname"].ToString()); //tested
                                        if (etysls.Count > 0)
                                            tos.Add(etysls[0]);
                                        /// --  end  --
                                    }
                                    result= this.SendTemplateMessage(etys[0], "Notice Update", "New " + t + ":" + etys[0].Title, tos);
                                    Log.WriteLog("发送模板消息(接口返回" + result + ") :" + etys[0].Title);
                                }
                                /// --  end  --
                            }
                            
                            Response.Write("ok");
                            Response.End();
                        }

                        tx.CommitTrans();
                        Response.Write("fail");
                        Response.End();
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        tx.AbortTrans();
                    }
                    catch
                    {
                        tx.AbortTrans();
                        Response.Write("fail");
                        Response.End();
                    }
                }
            }
        }

        private string SendTemplateMessage(T_Notice ety, string messag,string title,List<Model.Sys_Login_Account> tos)
        {
            if (tos.Count <= 0)
                return "";
            try
            {
                WxTemplateMessage msg = new WxTemplateMessage();
                WxTemplageMsgToUser user = new WxTemplageMsgToUser();

                for (int i = 0; i < tos.Count; i++)
                {                    
                    user.username = tos[i].LoginName;
                    user.password = tos[i].Password;
                    user.openId = tos[i].Wx_OpenId;
                    msg.toUsers.Add(user);
                }
                msg.msgId = ety.NoticeId;
                msg.msgType = "N";
                msg.status = ety.Status;               
                msg.title = ety.Title;
                msg.detail = ety.Detail;
                msg.date = Helper.Aid.Date2Str(ety.CreateDate, Helper.Aid.DateFmt.Standard);

                string data = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                EncryptUtil encry = new EncryptUtil();
                data = encry.Encrypt(data);
                string param = HttpUtility.UrlEncode("data") + "=" + HttpUtility.UrlEncode(data);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(param);
                string url = System.Configuration.ConfigurationManager.AppSettings["transUrl"] + "?act=sendtemplatemsg"; //推送
                Log.WriteLog("prepare send template message to " + url);
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                System.IO.Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();

                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s = response.GetResponseStream();
                string result = Helper.Aid.Stream2Str(s);
                s.Close();
                Log.WriteLog("send out template message result:" + result);
                return result;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        private DataTable GetNotices()
        {
            Model.Sys_Users_Group g = Session["groupinfo"] as Model.Sys_Users_Group;
            string searchPhrase = Request.Form["searchPhrase"];

            string sql = "select t4.*,t5.property_name from t_notice t4 left join pone_pd_property t5 on t4.propertycode = t5.property_code";
            sql += " where propertycode in (select t3.propertycode";
            sql += " from sys_login_system         t,";
            sql += " sys_login_system_group   t2,";
            sql += " sys_users_group          t6,";
            sql += " sys_users_group_property t3";
            sql += " where 1 = 1";
            sql += " and t.userid = t2.userid";
            sql += " and t2.groupid = t6.groupid";
            sql += " and t2.groupid = t3.groupid";
            sql += " and t6.groupid =" + g.GroupId.ToString(); //根据当前登录用户的groupid来过滤property。     
            sql += " and t.loginname = '" + Session["loginname"].ToString() + "'";
            if (g.Dept != "")
                sql += " and t6.dept = '" + g.Dept + "'";
            sql += " and t6.approver = '1')"; //这里需要是审批人。
            sql += " and t4.status = 'A'";
            sql += " and t4.approve = 'I'";
            if (searchPhrase != "")
            {
                sql += " and (t4.propertycode like '" + searchPhrase.ToUpper() + "%' or t4.title like '" + searchPhrase + "%' or t4.detail like '" + searchPhrase + "%')";
            }
            
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole biz = new Helper.DbConsole(tx);
                DataTable dt = biz.RetrieveDataTable(sql);
                tx.CommitTrans();

                return dt;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                return new DataTable();
            }            
        }
    }
}