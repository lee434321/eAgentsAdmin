using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Helper;

using System.Reflection;
namespace PropertyOneAppWeb.system
{
    public class eAgentsDb : DbConsoleExam
    {
        public eAgentsDb() : base(Commom.GlobalUtilSystem.sdb_local) { }

        public DbSet<Model.Sys_Login_Account> SysLoginAccounts { get; set; }
    }

    public partial class FeedbackMaintenance : PageBase
    {
        string webRootPath = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            eAgentsDb db = new eAgentsDb();

            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            if (!string.IsNullOrEmpty(Request.QueryString["fid"]))
            {
                JsonRsp rsp = new JsonRsp();
                DbTx tx = new DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                try
                {
                    int fid = int.Parse(Request.QueryString["fid"]);
                    FeedbackDataSet fds = new FeedbackDataSet();

                    DbConsole bz = new DbConsole(tx);                    
                    List<T_FeedBack> etys = bz.Retrieve<T_FeedBack>(x => x.FeedbackId == fid);
                    if (etys.Count > 0)
                    {
                        fds.feedback = etys[0];                       
                        List<T_Feedback_Res> etys2 = bz.Retrieve<T_Feedback_Res>(x => x.FeedbackId == fid);
                        etys2.Sort((x, y) => x.Id < y.Id ? -1 : 0);
                        fds.feedbackres = etys2;

                        for (int j = 0; j < etys2.Count; j++)
                        {                            
                            List<Sys_Login_Account> etys3 = bz.Retrieve<Sys_Login_Account>(x => x.LoginName == etys2[j].CreateBy);
                            if (etys3.Count > 0 && etys2[j].ReplyType=="L") // "L"表示前台用户
                                etys2[j].Src = "front";
                            else
                                etys2[j].Src = "rear";
                        }
                    }
                    else
                        rsp.result = null;
                    tx.CommitTrans();
                    rsp.result = fds;
                }
                catch (Exception err)
                {
                    tx.AbortTrans();
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    Log.WriteLog("vue get feedback_res error :" + err.Message);
                }
                Response.Write(JsonConvert.SerializeObject(rsp));
                Response.End();
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["grpid"]))  //grpid
            {
                JsonRsp rsp = new JsonRsp();
                Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                try
                {
                    int grpid = int.Parse(Request.QueryString["grpid"]);
                    Helper.DbConsole bz = new Helper.DbConsole(tx);
                    List<Sys_Users_Group> etys = bz.Retrieve<Sys_Users_Group>(x => x.GroupId == grpid);
                    if (etys.Count > 0)
                        rsp.result = etys[0];
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "Group not exists.";
                    }
                    tx.CommitTrans();
                }
                catch (Exception err)
                {
                    tx.AbortTrans();
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    Log.WriteLog("vue get grpid error :" + err.Message);
                }
                Response.Write(JsonConvert.SerializeObject(rsp));
                Response.End();
            }

            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "search")
                {
                    string startdate = Request.Form["startdate"];
                    string type = Request.Form["type"];
                    string reply = Request.Form["reply"];
                    int current = Int32.Parse(Request.Form["current"]);
                    int rowCount = Int32.Parse(Request.Form["rowCount"]);

                    string json = GenJsonFeedback(startdate, type, reply, current, rowCount);
                    Response.Write(json);
                    Response.End();
                }
                else if (action=="vue-req2close")
                {
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        int fid = Helper.Aid.Null2Int(Request.Form["fbid"]);

                        if (fid > 0)
                        {                           
                            List<T_FeedBack> etys = bz.Retrieve<T_FeedBack>(x => x.FeedbackId == fid); //tested
                            if (etys.Count > 0)
                            {
                                if (etys[0].STATUS == 400)
                                {
                                    rsp = new JsonRsp(-1, "You can not request to close, for the status is closed");
                                }
                                else
                                {
                                    etys[0].STATUS = 300;   //300表示pending close状态
                                    etys[0].Update_By = Session["loginname"].ToString();
                                    etys[0].Last_Update = DateTime.Now;
                                    bz.Update(etys[0]);
                                }
                            }
                            else 
                                rsp.err_msg = "feedback not found with id '" + fid.ToString() + "'";                            
                        }
                        else {
                            rsp.err_code = -1;
                            rsp.err_msg = "feedbackid is required";
                        }

                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                        rsp.result = null;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                }
                else if (action=="vue-aprv2close")
                {
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        int fid = Helper.Aid.Null2Int(Request.Form["fbid"]);

                        if (fid > 0)
                        {
                            List<T_FeedBack> etys = bz.Retrieve<T_FeedBack>(x => x.FeedbackId == fid);  //tested
                            if (etys.Count > 0)
                            {
                                if (etys[0].STATUS == 300)
                                {
                                    etys[0].STATUS = 400;   //400表示closed状态
                                    etys[0].Last_Update = DateTime.Now;
                                    etys[0].Update_By = Session["loginname"].ToString();
                                    bz.Update(etys[0]);
                                }
                                else
                                {
                                    rsp = new JsonRsp(-1, "You can not approve to close, for the status is not pending");
                                }                                
                            }
                            else
                                rsp.err_msg = "feedback not found with id '" + fid.ToString() + "'";
                        }
                        else
                        {
                            rsp.err_code = -1;
                            rsp.err_msg = "feedbackid is required";
                        }

                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                        rsp.result = null;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                    
                }
                else if (action == "vue-approve") //审批
                {
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        string[] idstr = Request.Form["ids"].ToString().Split(',');

                        List<int> ids = new List<int>();
                        for (int i = 0; i < idstr.Length; i++)
                            ids.Add(int.Parse(idstr[i]));

                        List<T_Feedback_Res> etys = bz.Retrieve<T_Feedback_Res>(x => x.Id.Sql_In(ids));  //tested
                        for (int j = 0; j < etys.Count; j++)
                        {
                            etys[j].Approve = "A";
                            etys[j].ApproveBy = Session["loginname"].ToString();
                            etys[j].ApproveDate = DateTime.Now;
                            int c = bz.Update(etys[j]);

                            if (c > 0)
                            {
                                List<Model.T_FeedBack> etys2 = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == etys[j].FeedbackId);  //tested
                                if (etys2.Count > 0)
                                {
                                    etys2[0].STATUS = 200; //置为200，表示已经回复。
                                    etys2[0].Last_Update = DateTime.Now;
                                    etys2[0].Update_By = Session["loginname"].ToString();
                                    bz.Update(etys2[0]);
                                    /// -- start --

                                    webRootPath = Request.MapPath(".");
                                    System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(delegate(object d)
                                    {
                                        Log.WriteLog("-- Start send mail & push noti thread --");
                                        T_Feedback_Res ety = d as T_Feedback_Res;
                                        SendMail(ety);
                                        PushNoti(ety);
                                        SendTemplateMessage(ety);
                                        Log.WriteLog("-- End thread --");
                                    });
                                    System.Threading.Thread t = new System.Threading.Thread(pts);
                                    t.Start(etys[0]);
                                    /// --  end  --
                                    //SendMail(etys[j]);
                                    //PushNoti(etys[j]);
                                }
                            }
                        }
                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                }
                else if (action == "vue-del")
                {
                    //只能删除当前用户自己提交的reply ,实际保存时要再次检查是否审批
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        int id = int.Parse(Request.Form["rid"].ToString());                        
                        List<T_Feedback_Res> etys = bz.Retrieve<T_Feedback_Res>(x => x.Id == id);
                        if (etys.Count > 0)
                        {
                            if (etys[0].Approve == "U")
                            {
                                int c = bz.Delete(etys[0]);
                                if (c > 0)
                                    rsp.result = true;
                                else
                                    rsp.result = false;
                            }
                            else
                            {
                                rsp.err_code = -1;
                                rsp.err_msg = "It 's already approved just now!";
                            }
                        }
                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                }
                else if (action == "vue-save")
                {
                    //只能修改当前用户自己提交的reply ,实际保存时要再次检查是否审批
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        int id = int.Parse(Request.Form["rid"].ToString());
                        string content = Request.Form["content"].ToString();                        
                        List<T_Feedback_Res> etys = bz.Retrieve<T_Feedback_Res>(x => x.Id == id);

                        if (etys.Count > 0)
                        {
                            if (etys[0].Approve == "U")
                            {                                
                                List<Model.T_FeedBack> etysH = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == etys[0].FeedbackId);
                                if (etysH.Count>0)
                                {
                                    etysH[0].STATUS = 200;
                                    etysH[0].Last_Update = DateTime.Now;
                                    etysH[0].Update_By = Session["loginname"].ToString();
                                    bz.Update(etysH[0]);
                                }

                                etys[0].Detail = content;
                                int c = bz.Update(etys[0]);
                                if (c > 0)
                                {
                                    rsp.result = true;
                                    /// 向审批者发送邮件（提醒审批人员）                    
                                    SendMail2Approver(etys[0]);
                                }
                                else
                                    rsp.result = false;
                            }
                        }
                        else
                        {
                            rsp.err_code = -1;
                            rsp.err_msg = "It 's already approved.";
                        }
                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                }
                else if (action == "vue-reply")
                {
                    JsonRsp rsp = new JsonRsp();
                    Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                    try
                    {
                        Helper.DbConsole bz = new Helper.DbConsole(tx);
                        Model.T_Feedback_Res ety = new T_Feedback_Res();
                        ety.CreateBy = Session["loginname"].ToString();
                        ety.CreateDate = DateTime.Now;
                        ety.Detail = Request.Form["content"].ToString();
                        ety.FeedbackId = int.Parse(Request.Form["fid"].ToString());
                        ety.Leasenum = Request.Form["leasenum"].Trim();
                        ety.ReplyType = "S";//“S”表示是后台人员回复。
                        ety.ReplyPerson = Session["loginname"].ToString();
                        ety.Status = "A";
                        ety.Approve = "U"; //这里要先设置为未审批。                        
                        ety.Id = Helper.Aid.DbNull2Int(tx.ExecuteScalar("select max(id) from " + Model.T_Feedback_Res.TABLE_NAME).ToString()) + 1;
                        int c = bz.Create(ety);                        
                        if (c > 0)
                        {
                            rsp.result = true;
                            /// 向审批者发送邮件（提醒审批人员）                    
                            SendMail2Approver(ety);
                        }
                        else
                            rsp.result = false;
                        
                        List<Model.T_FeedBack> etysH = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);
                        if (etysH.Count>0)
                        {
                            etysH[0].STATUS = 200;//200表示已回复（早期定义，实际界面不再使用该字段）
                            etysH[0].Last_Update = DateTime.Now;
                            etysH[0].Update_By = Session["loginname"].ToString();
                            bz.Update(etysH[0]);
                        }
                        tx.CommitTrans();
                    }
                    catch (Exception err)
                    {
                        tx.AbortTrans();
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                        rsp.result = false;
                    }
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                    Response.Write(json);
                    Response.End();
                }
                else if (action == "authapprove") //检查apprrove授权，返回true时按钮可用，否则不可用。
                {
                    string result = this.AuthApprove();
                    Response.Write(result);
                    Response.End();
                }
                else if (action == "sendrpt")
                {
                    JsonRsp rsp = new JsonRsp();
                    try
                    {                       
                        FeedbackSummaryRpt2 fsr2 = new FeedbackSummaryRpt2();
                        fsr2.Execute(true);

                        FeedbackSummaryRpt1 fsr1 = new FeedbackSummaryRpt1(); //新尝试
                        fsr1.Execute(true);

                        rsp.result = true;                              
                    }
                    catch (Exception err)
                    {
                        rsp.err_code = -99;
                        rsp.result = null;
                        rsp.err_msg = err.Message;
                    }
                    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                    Response.End();
                }
            }
        }

        private string AuthApprove()
        {
            JsonRsp rsp = new JsonRsp();
            //检查权限
            try
            {
                bool auth = false;
                string originlist = Session["authlist"].ToString();
                DataModel.ModelUserAuthList ety = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel.ModelUserAuthList>(originlist);
                for (int i = 0; i < ety.authInfo.Count; i++)
                {
                    if (ety.authInfo[i].authId == "91")
                    {
                        auth = true;
                        break;
                    }
                }

                rsp.err_code = 0;
                rsp.result = auth;
            }
            catch (Exception err)
            {
                rsp.err_code = -1;
                rsp.err_msg = err.Message;
                rsp.result = null;
                Log.WriteLog("AuthApprove error:" + err.Message);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }     

        /// <summary>
        /// 推送通知
        /// </summary>
        private void PushNoti(Model.T_Feedback_Res ety)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.T_FeedBack> etys = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);                
                List<Model.Sys_Login_Account> etys2 = bz.Retrieve<Model.Sys_Login_Account>(x => x.LoginName == etys[0].CreateBy);
                tx.CommitTrans();

                string url = System.Configuration.ConfigurationManager.AppSettings["PushUrl"];
                url = System.Configuration.ConfigurationManager.AppSettings["transUrl"] + "?act=pushnoti";
                Log.WriteLog("Push service url:" + url);
                string result = HttpUtil.PushNotification(url, etys[0].Title, ety.Detail, etys2[0].Registration_Id);

                Log.WriteLog("Push notice result :" + result);
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Log.WriteLog("Push notice error :" + err.Message);
                throw err;
            }
        }

        /// <summary>
        /// 发送邮件给前台用户
        /// </summary>
        /// <param name="ety"></param>
        private void SendMail(Model.T_Feedback_Res ety)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new DbConsole(tx);

                /// 取feedback信息                
                List<Model.T_FeedBack> etysH = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);
                if (etysH.Count == 0) { throw new Exception("Feedback not found!"); }

                /// 取此条feedback创建人的邮箱                
                List<Model.Sys_Login_Account> etysAcc = bz.Retrieve<Model.Sys_Login_Account>(x => x.LoginName == etysH[0].CreateBy);
                if (etysAcc.Count == 0) { throw new Exception("LoginName not found!"); }
                tx.CommitTrans();

                /// 发送               
                string email = etysAcc[0].Email;
                string str = "{\"username\":\"" + etysAcc[0].LoginName + "\",\"password\":\"" + etysAcc[0].Password + "\",\"lang\":\"en-US\"}";
                string encryptStr = new EncryptUtil().Encrypt(str);
                string qs = "&txt=" + encryptStr;
                string tenantlink = System.Configuration.ConfigurationManager.AppSettings["TenantMailUrl"];
                string link = string.Format("<a href='{0}'>link</a>", tenantlink + "/Contact/Reply?feedbackId=" + ety.FeedbackId + qs);
                ExchangeMail em = new ExchangeMail();
                string html = this.GenEmailBody(link);
                if (html == "")
                    html = this.GenMailBody(link);

                if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                    em.SendExchangeMail(email, "新的回复讯息", html);
                else
                    em.SendExchangeMail(email, "新的回覆訊息 New replied message of HutchisonAgent", html);
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Log.WriteLog("Send mail error:" + err.Message);
            }
        }       

        private void SendMail2Approver(Model.T_Feedback_Res ety)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Model.Sys_Users_Group grp = Session["groupinfo"] as Model.Sys_Users_Group;

                Helper.DbConsole biz = new Helper.DbConsole(tx);
                string sql = "select a.*  from sys_users_group_authority a";
                sql += " inner join sys_users_group b";
                sql += " on a.groupid = b.groupid";
                sql += " where 1 = 1";
                sql += " and a.groupid in";
                sql += " (select groupid";
                sql += " from sys_users_group_property";
                sql += " where propertyCode in (select propertycode";
                sql += " from sys_users_group_property";
                sql += " where groupid = " + grp.GroupId.ToString() + ")";
                sql += " and groupid <> " + grp.GroupId.ToString() + ")";
                sql += " and authtypeid = 91";
                if (grp.Dept != "")
                    sql += " and b.dept ='" + grp.Dept + "'";
                sql += " and b.status = 'A'";
                List<Model.Sys_Users_Group_Authority> etys0 = biz.Retrieve<Model.Sys_Users_Group_Authority>(sql);
                if (etys0.Count == 0) return;

                string ingrps = "(";
                for (int i = 0; i < etys0.Count; i++)
                    ingrps += etys0[i].GroupId.ToString() + ",";
                ingrps = ingrps.Substring(0, ingrps.Length - 1);
                ingrps += ")";

                sql = "select * from sys_login_system where userid in";
                sql += " (select userid from sys_login_system_group where groupid in " + ingrps + ")";

                List<Model.Sys_Login_System> etys4 = biz.Retrieve<Model.Sys_Login_System>(sql);
                if (etys4.Count == 0) return;

                #region obsolete

                //// 获取所有审批权限组(authtypeid=91)
                //biz.Clauses.Add("authtypeid", new Helper.Evaluation(Helper.SqlOperator.EQ, 91));
                //List<Model.Sys_Users_Group_Authority> etys0 = biz.Retrieve<Model.Sys_Users_Group_Authority>(); biz.Clauses.Clear();                
                //if (etys0.Count == 0) return;

                //// 获取当前登录所属property
                //biz.Clauses.Add("groupid", new Helper.Evaluation(Helper.SqlOperator.EQ, grpid));
                //List<Model.Sys_Users_Group_Property> etys1 = biz.Retrieve<Model.Sys_Users_Group_Property>(); biz.Clauses.Clear();
                //if (etys1.Count == 0) return;

                //// 取property中有审批权限的分组
                //object[] props = new object[etys1.Count];
                //for (int i = 0; i < etys1.Count; i++)
                //{
                //    props[i] = etys1[i].PropertyCode;
                //}
                //object[] grps = new object[etys0.Count];
                //for (int i = 0; i < etys0.Count; i++)
                //{
                //    grps[i] = etys0[i].GroupId;
                //}
                //biz.Clauses.Add("propertycode", new Helper.Evaluation(Helper.SqlOperator.IN, props));
                //biz.Clauses.Add("groupid", new Helper.Evaluation(Helper.SqlOperator.IN, grps));
                //List<Model.Sys_Users_Group_Property> etys2 = biz.Retrieve<Model.Sys_Users_Group_Property>(); biz.Clauses.Clear();
                //if (etys2.Count == 0) return;

                //// 再次取分组
                //grps = new object[etys2.Count];
                //for (int i = 0; i < etys2.Count; i++)
                //{
                //    grps[i] = etys2[i].GroupId;
                //}
                //biz.Clauses.Add("groupid", new Helper.Evaluation(Helper.SqlOperator.IN, grps));
                //List<Model.Sys_Login_System_Group> etys3 = biz.Retrieve<Model.Sys_Login_System_Group>(); biz.Clauses.Clear();
                //if (etys3.Count == 0) return;

                //// 取sys_user
                //object[] users = new object[etys3.Count];
                //for (int i = 0; i <etys3.Count; i++)
                //{
                //    users[i] = etys3[i].UserId;
                //}
                //biz.Clauses.Add("UserId", new Helper.Evaluation(Helper.SqlOperator.IN, users));
                //List<Model.Sys_Login_System> etys4 = biz.Retrieve<Model.Sys_Login_System>(); biz.Clauses.Clear();
                //if (etys4.Count == 0) return;
                #endregion

                // 发邮件
                ExchangeMail em = new ExchangeMail();
                HashSet<string> sents = new HashSet<string>();
                string link = string.Format("<a href='{0}'>link</a>", Request.Url + "?feedbackid=" + ety.FeedbackId.ToString());
                for (int i = 0; i < etys4.Count; i++)
                {
                    if (sents.Contains(etys4[i].Email))
                        continue;

                    if (etys4[i].LoginName != Session["loginname"].ToString())
                    {
                        em.SendExchangeMail(etys4[i].Email, "Approval request of Comment / Feedback on HutchisonAgent", this.GenMailBody4Approver(ety, link));
                        sents.Add(etys4[i].Email);
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
            }
        }

        /// <summary>
        /// 生成发送给审批者的邮件内容
        /// </summary>
        /// <param name="ety"></param>
        /// <param name="link">带有a标签的html字符串</param>
        /// <returns></returns>
        private string GenMailBody4Approver(Model.T_Feedback_Res ety, string link)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.T_FeedBack> etys = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);
                
                List<Model.Sys_Login_Account> etys2 = bz.Retrieve<Model.Sys_Login_Account>(x => x.LoginName == etys[0].CreateBy);
                if (etys2.Count == 0)
                    throw new Exception("Can not send feedback mail to approver for 'Login Name' not found!");
                
                List<Model.Sys_Users_Group_Lease> etys3 = bz.Retrieve<Model.Sys_Users_Group_Lease>(x => x.USERID == etys2[0].UserId && x.CREATE_SRC == "0");
                if (etys3.Count == 0)
                    throw new Exception("Can not send feedback mail to approver for lease number not found!");
                
                List<Pone_Lm_Lease> etys4 = bz.Retrieve<Pone_Lm_Lease>(x => x.LEASE_NUMBER == etys3[0].LEASENUMBER.Trim());
                if (etys4.Count == 0)
                    throw new Exception("Can not send feedback mail to approver for lease number not found(pone_lm_lease)!");

                string body = "<html><body>";

                string body_en = "You have an approval request of Comment / Feedback on HutchisonAgent." + "<br/>";
                body_en += "<br/>";
                body_en += "The following are the info of Comment / Feedback:" + "<br/>";
                body_en += string.Format("Lease Number：{0}", etys3[0].LEASENUMBER) + "<br/>";
                body_en += string.Format("Premises：{0}", etys4[0].Premise_Name1 + " " + etys4[0].Premise_Name2) + "<br/>";  //Premise_Name1 sql += ",l.premise_name1||' '||l.premise_name2 as premises";
                body_en += string.Format("User : {0}", etys[0].CreateBy) + "<br/>";
                body_en += string.Format("Subject : [{0}]", etys[0].Title) + "<br/>";
                body_en += string.Format("Content : [{0}]", ety.Detail) + "<br/>";
                body_en += string.Format("Created by : [{0}]", ety.CreateBy) + "<br/>";
                body_en += "<br/>";
                body_en += string.Format("Please check it by the following [{0}]", link) + "" + "<br/>";
                body_en += "<br/>";
                body_en += "This e-mail serves as notification only, please do not reply to this email." + "<br/>";
                body_en += "<br/>";
                body_en += "HutchisonAgent System <br/>";
                body += body_en;
                tx.CommitTrans();

                return body;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                throw err;
            }
        }

        /// <summary>
        /// 生成发送给用户的邮件内容
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string GenMailBody(string link)
        {
            string body = "<html><body>";

            string body_en = "Dear Tenant," + "<br/>";
            body_en += "<br/>";
            body_en += "You have a new message on HutchisonAgent." + "<br/>";
            body_en += "Please view the message via following link:" + "<br/>";
            body_en += link + "<br/>";
            body_en += "<br/>";
            body_en += "This e-mail serves as notification only, please do not reply to this email. If you change your email address, please notify us immediately by changing it through the website." + "<br/>";

            body_en += "Thank you for using our HutchisonAgent. <br/>";
            body_en += "<br/>";
            body_en += "Yours faithfully" + "<br/>";
            body_en += "Hutchison Estate Agents Limited" + "<br/>";
            body_en += "<br/><br/>";
            body += body_en;

            body += "親愛的租戶:" + "<br/>";
            body += "<br/>";
            body += "您有一條新的HutchisonAgent訊息。" + "<br/>";
            body += "請透過以下連結檢視訊息: " + "<br/>";
            body += link + "<br/>";
            body += "<br/>";
            body += "這只是通告電郵，請不要回覆此電郵。如果您更改您的電子郵件地址，請即時透過我們的網站更改，以便知悉。" + "<br/>";
            body += "<br/>";
            body += "感謝您使用我們的HutchisonAgent。" + "<br/>";
            body += "<br/>";

            body += "和記地產代理有限公司 謹啟" + "<br/>";
            body += "</body></html>";
            return body;
        }

        private string GenEmailBody(string link)
        {
            try
            {
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                string rpath = webRootPath;    //当前目录 
                xd.Load(rpath + "\\email.xml");         //加载配置文件
                System.Xml.XmlElement root = xd.DocumentElement; //获取根节点

                // 1.识别区域(hk,prc)及业务,获取模板
                System.Xml.XmlNode node = null;
                if (Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]) == "zh-CHS")
                    node = root.SelectSingleNode("//template[@id='3']/content[@type='zh']");
                else
                    node = root.SelectSingleNode("//template[@id='3']/content[@type='hk']");

                // 2.根据具体业务替换模板中变量生成邮件内容
                string body = node.InnerText.Replace("{link}", link);
                return body;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string GenJsonFeedback(string startdate, string type, string reply, int current, int rowCount)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                var predicate = PredicateBuilder.True<T_FeedBack>();
                if (reply == "all")
                {
                    //bz.Clauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, reply == "notreplay" ? 100 : 200));
                }
                else if (reply == "notreplay")
                {
                    //bz.WhereClauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, 100));
                    predicate = predicate.And(p => p.STATUS == 100);
                }
                else if (reply == "replied")
                {
                    //bz.WhereClauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.GTE, 200));
                    predicate = predicate.And(p => p.STATUS == 200);
                }
                else if (reply == "pending2close")
                {
                    //bz.WhereClauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, 300));
                    predicate = predicate.And(p => p.STATUS == 300);
                }
                else if (reply == "closed")
                {
                    //bz.WhereClauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, 400));
                    predicate = predicate.And(p => p.STATUS == 400);
                }

                List<Model.T_FeedBack> etyfbs = bz.Retrieve<Model.T_FeedBack>(predicate);
                JsonApi.JsonApi_GetFeedback_Res jsonDataRes = new JsonApi.JsonApi_GetFeedback_Res(etyfbs); //取feedback主表
                for (int i = 0; i < jsonDataRes.feedbackinfo.Count; i++)
                {
                    string fbid = jsonDataRes.feedbackinfo[i].feedbackid;
                    List<T_Feedback_Res> etys2 = bz.Retrieve<T_Feedback_Res>("select * from t_feedback_res where feedbackid=" + fbid + " order by id desc ");
                    if (etys2.Count > 0)
                    {
                        if (etys2.FindAll(x => { return x.Approve == "U"; }).Count > 0)
                            jsonDataRes.feedbackinfo[i].approve = "W";
                        else
                            jsonDataRes.feedbackinfo[i].approve = etys2[0].Approve;
                        jsonDataRes.feedbackinfo[i].id = etys2[0].Id;                       

                        /// -- start --                        
                        List<T_Feedback_Res> etysfbres = bz.Retrieve<T_Feedback_Res>(x => x.FeedbackId == int.Parse(fbid));
                        if (etysfbres.Count > 0)
                        {
                            if (etysfbres.FindAll(x => { return x.Approve == "A"; }).Count > 0) //如後台用戶回覆訊息, 而approver也審批了該訊息後, “Feedback status” 變成 “Replied”                            
                                jsonDataRes.feedbackinfo[i].feedbackStatus = "Replied";
                            else
                                jsonDataRes.feedbackinfo[i].feedbackStatus = "Not Reply";

                            if (etysfbres.FindAll(x => { return x.Approve == "U"; }).Count > 0) //如feedback內有任何回覆訊息是未經審批,  “Approval status” 變成 “Waiting for approval” .                            
                                jsonDataRes.feedbackinfo[i].approveStatus = "Waiting for approval";
                            else
                                jsonDataRes.feedbackinfo[i].approveStatus = "Approved";
                        }
                        else
                        {
                            jsonDataRes.feedbackinfo[i].feedbackStatus = "Not Reply";
                            jsonDataRes.feedbackinfo[i].approveStatus = "N/A";
                        }
                        /// --  end  --
                    }
                    else
                    {
                        jsonDataRes.feedbackinfo[i].approve = "W";//etys2.count 为空表示"等待回复"（wait for reply）  
                        jsonDataRes.feedbackinfo[i].feedbackStatus = "Not Reply";
                        jsonDataRes.feedbackinfo[i].approveStatus = "N/A";
                    }

                    if (jsonDataRes.feedbackinfo[i].status == "300")
                    {
                        jsonDataRes.feedbackinfo[i].feedbackStatus = "Pending to close";
                        jsonDataRes.feedbackinfo[i].approveStatus = "Waiting for approval";
                    }
                    else if (jsonDataRes.feedbackinfo[i].status == "400")
                    {
                        jsonDataRes.feedbackinfo[i].feedbackStatus = "Closed";
                        jsonDataRes.feedbackinfo[i].approveStatus = "N/A";
                    }
                }
                /// --  end  --

                /// -- start -- 这里要过滤当前登录用户可以查看的feedback
                List<Model.Sys_Users_Group_Property> etys = Bizhub.GetProperties(int.Parse(Session["groupid"].ToString()));
                List<JsonApi.JsonApi_GetFeedback_feedbackinfo> fbs = new List<JsonApi.JsonApi_GetFeedback_feedbackinfo>();

                // filter 1
                var result = from x in jsonDataRes.feedbackinfo
                             from y in etys
                             where x.leasenum.Substring(0, 4) == y.PropertyCode.Trim()
                             select x;

                foreach (var item in result)
                {
                    //再次过滤当前用户是mkt或em可查看feedback类型的情况
                    //em 除了500都可以看 （Estate Management related）
                    //mkt 除了600都可以看（lease related）
                    //others 都可以看 900。

                    // filter 2
                    Model.Sys_Users_Group g = Session["groupinfo"] as Model.Sys_Users_Group;
                    if (g.Dept != "" ? g.Dept == "EM" : g.GroupName.ToUpper().Contains("EM")) //如果是em登录
                    {
                        if (item.type == "600" && item.type != "700")
                        {
                            fbs.Add(item);
                        }
                    }
                    else if (g.Dept != "" ? g.Dept == "MKT" : g.GroupName.ToUpper().Contains("MKT")) // 如果是mkt登录
                    {
                        if (item.type != "600" && item.type != "700")
                        {
                            fbs.Add(item);
                        }
                    }
                    else if (g.Dept != "" ? g.Dept == "FIN" : g.GroupName.ToUpper().Contains("FIN")) // 如果是fin登录
                    {
                        if (item.type == "700")
                        {
                            fbs.Add(item);
                        }
                    }
                    else  //如果是其他组用户登录
                    {
                        fbs.Add(item);
                    }
                }
                /// --  end  --

                List<JsonApi.JsonApi_GetFeedback_feedbackinfo> list = new List<JsonApi.JsonApi_GetFeedback_feedbackinfo>();
                foreach (JsonApi.JsonApi_GetFeedback_feedbackinfo feedbackinfo in fbs)
                {
                    if (feedbackinfo.approve == "U")
                        feedbackinfo.sort = 1;
                    else if (feedbackinfo.approve == "W")
                        feedbackinfo.sort = 2;
                    else
                        feedbackinfo.sort = 9;
                    list.Add(feedbackinfo);
                }

                /// -- start --
                JsonRsp obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonRsp>(AuthApprove());
                if (Convert.ToBoolean(obj.result))
                    list = list.OrderBy(o => o.sort).ThenByDescending(o => o.date).ToList(); //For approver, please sort by those pending for approval, pending for replied                
                else
                {    //按时间排序
                    list = list.OrderByDescending(o => o.date).ThenBy(o => o.type).ToList();
                }
                /// --  end  --
                
                //设置翻页
                list = GlobalUtil.PageList<JsonApi.JsonApi_GetFeedback_feedbackinfo>(list, current, rowCount);
                //创建bootgrid的json数据
                JsonApi.JsonApi_Bootgrid_SearchFeedback jbs = new JsonApi.JsonApi_Bootgrid_SearchFeedback();
                jbs.current = current;
                jbs.rowCount = rowCount;
                jbs.rows = list;
                jbs.total = fbs.Count;

                tx.CommitTrans();
                string json = JsonConvert.SerializeObject(jbs);
                return json;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                return GlobalUtil.bootErrorStr;
            }
        }

        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="ety"></param>
        /// <returns></returns>
        ///  关于微信模板消息的问题，需要后台程序实现以下功能：
        ///  1.	Logincheck再增加一个参数openid,保存到用户数据表
        ///  2.	前台站点新增发送微信模板消息的接口，http://172.21.111.108:837/WebService.asmx/sendTemplateMessage
        ///  参数data是json格式的数据，加密方式和接口pushNotification一致
        ///  data的json格式如下：
        ///  {"openIds":[""]," msgType ":"","msgId":"","title":"","detail":"","date":"","status":"","remark":""}
        ///  openIds多个值以逗号分隔，msgType表示消息类型（N-表示Notice,S-表示Service，C-表示Carpark，F-表示Feedback）
        ///  msgId表示Notice或Feedback的ID
        private static string SendTemplateMessage(Model.T_Feedback_Res ety)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {                
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                WxTemplateMessage msg = new WxTemplateMessage();                                            
                WxTemplageMsgToUser user = new WxTemplageMsgToUser();                                
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Sys_Login_Account>(x => x.LoginName == ety.CreateBy);                
                List<Model.T_FeedBack> etys2fb = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);                
                List<Model.Sys_Login_Account> etysAccs = bz.Retrieve<Model.Sys_Login_Account>(x=>x.LoginName==etys2fb[0].CreateBy);
                
                if (etysAccs.Count > 0)
                {
                    user.username = etysAccs[0].LoginName;
                    user.password = etysAccs[0].Password;
                    user.openId = etysAccs[0].Wx_OpenId;
                }
                msg.toUsers.Add(user);
                msg.msgId = ety.FeedbackId;
                msg.msgType = "F";
                msg.status = ety.Status;                
                List<Model.T_FeedBack> etysh = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId);
                msg.title = etysh[0].Title;
                msg.detail = ety.Detail;
                msg.date = Helper.Aid.Date2Str(ety.CreateDate, Helper.Aid.DateFmt.Standard);
                tx.CommitTrans();

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
                tx.AbortTrans();
                return err.Message;
            }
        }
    }

    public class WxTemplateMessage
    {
        public List<WxTemplageMsgToUser> toUsers = new List<WxTemplageMsgToUser>();
        //{"openIds":[""]," msgType ":"","msgId":"","title":"","detail":"","date":"","status":"","remark":""}
        public List<string> openIds = new List<string>();   //多个值以逗号分隔
        public string msgType { get; set; }         //表示消息类型（N-表示Notice,S-表示Service，C-表示Carpark，F-表示Feedback）
        public int msgId { get; set; }              //表示Notice或Feedback的ID
        public string title { get; set; }
        public string detail { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string remark { get; set; }
    }

    public class WxTemplageMsgToUser
    {
        public string openId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string lang { get; set; }
    }

    public class RspFeedback
    {
        public int result { get; set; }
        public string err_msg { get; set; }
        public Model.T_FeedBack head = new T_FeedBack();
        public List<Model.T_Feedback_Res> body = new List<T_Feedback_Res>();
    }

    public class FeedbackDataSet
    {
        public T_FeedBack feedback { get; set; }
        public List<T_Feedback_Res> feedbackres = new List<T_Feedback_Res>();
    }

    public class SysMailParamsModel
    {
        public const string TABLE_NAME = "Sys_Mail_Params";

        [DbField(true)]
        public int Id { get; set; }

        public string PARAM_TYPE { get; set; }
        public string PARAM_NAME { get; set; }
        public string PARAM_DESC { get; set; }
        public string PARAM_VALUE { get; set; }

        [DbField(false, false, true)]
        public List<SysMailParamsModel> notis = new List<SysMailParamsModel>();
    }
}
