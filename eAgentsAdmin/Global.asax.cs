using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using PropertyOneAppWeb.Helper;
namespace PropertyOneAppWeb
{
    public class Global : System.Web.HttpApplication
    {
        System.Timers.Timer t = null;

        protected void Application_Start(object sender, EventArgs e)
        {            
            try
            {
                if (string.IsNullOrEmpty(Helper.Aid.yPath))                
                    Aid.yPath = AppDomain.CurrentDomain.BaseDirectory;

                t = new System.Timers.Timer();
                t.Interval = 3 * 60 * 1000; //
                t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
                t.Start();
                Commom.Log.WriteLog("注册并启动定时任务...");
            }
            catch (Exception)
            { }
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {            
            try
            {
                Pulse();
                /// -- start -- task                
                FeedbackSummaryRpt1 fsr1 = new FeedbackSummaryRpt1();
                fsr1.Execute();

                FeedbackSummaryRpt2 t2 = new FeedbackSummaryRpt2();
                t2.Execute();
                /// --  end  --

                /// -- start -- 这是需要实现的每晚定时运行并发送的邮件格式 pkg_eform_flow.Get_BatchEmail_NoticeEForm
                // 检查公共状态是否为0，true则执行，false则跳过
                if (Aid.Status4EFM_NTC == "0" || string.IsNullOrEmpty(Aid.Status4EFM_NTC))
                {                   
                    InformNtcEfmTask();
                }              
                /// --  end  --                          
            }
            catch (Exception err)
            {                
                Commom.Log.WriteLog("Elapsed task error:" + err.Message + "\r\n" + err.StackTrace);
            }
        }

        private void InformNtcEfmTask()
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {                
                PropertyOneAppWeb.system.EformMailSender email = new system.EformMailSender(tx);
                email.Send("EAgent_BatchInformStaff_NTC_EFM", "G", DateTime.Now);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //Commom.Log.WriteLog("Session start");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Commom.Log.WriteLog("Application_Error");
        }

        protected void Session_End(object sender, EventArgs e)
        {
            //Commom.Log.WriteLog("Session end[" + sender.GetType() + "]");
        }
        
        protected void Application_End(object sender, EventArgs e)
        {
            try
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["vpath"];
                Commom.Log.WriteLog("IIS Application_End Event fired,attempt to restart with url:" + url);

                //这里设置你的web地址，可以随便指向你的任意一个aspx页面甚至不存在的页面，目的是要激发Application_Start               
                System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
            }
            catch (Exception err)
            {
                Commom.Log.WriteLog("Application restart error with " + err.Message);
            }
        }

        private void Pulse()
        {
            try
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["vpath"];

                //这里设置你的web地址，可以随便指向你的任意一个aspx页面甚至不存在的页面，目的是要激发Application_Start               
                System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流    

            }
            catch (Exception err)
            {
                Commom.Log.WriteLog("Pulse error:" + err.Message);
            }
        }
    }

    #region 以下业务逻辑暂时放在Global.asax.cs文件中。
    public abstract class MailFactory
    {
        public static MailFactory Creator(string type)
        {
            switch (type)
            {
                case "ALERT_FEEDBACK_NOREPLAY_1":
                    return new NoRepliesIn7(type);
                case "ALERT_FEEDBACK_REPORT_2":
                    return new FeedbackSummaryRpt2();
                default:
                    return null;
            }
        }

        public abstract void Execute(bool skipcheck = false);
        public abstract bool Check(int id);
        public abstract List<system.SysMailParamsModel> GetParams(string type);

        public static string Css()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<style>");
            sb.Append("body{");
            sb.Append(" font-family: -apple-system,BlinkMacSystemFont,\"Segoe UI\",Roboto,\"Helvetica Neue\",Arial,sans-serif,\"Apple Color Emoji\",\"Segoe UI Emoji\",\"Segoe UI Symbol\";");
            sb.Append(" font-size: 1rem;");
            sb.Append(" font-weight: 400;");
            sb.Append(" line-height: 1.5;");
            sb.Append(" color: #212529;");
            sb.Append(" text-align: left;");
            sb.Append(" }");
            sb.Append(" table,th,td{border: 1px solid black;border-collapse:collapse;}");
            sb.Append(" table thead{background-color:#d9d9d9}");
            sb.Append(" span{ color:blue; }");            
            sb.Append("</style>");
            return sb.ToString();
        }

        public static string Css2()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<style>");
            sb.Append(" body{");
            sb.Append(" font-family: -apple-system,BlinkMacSystemFont,\"Segoe UI\",Roboto,\"Helvetica Neue\",Arial,sans-serif,\"Apple Color Emoji\",\"Segoe UI Emoji\",\"Segoe UI Symbol\";");
            sb.Append(" font-size: 1rem;");
            sb.Append(" font-weight: 400;");
            sb.Append(" line-height: 1.5;");
            sb.Append(" color: #212529;");
            sb.Append(" text-align: left;");
            sb.Append("}");
            sb.Append(" table{border-collapse: collapse;}");
            sb.Append(" table td, table th { border: 1px solid #001122;font-size:0.8rem;}");
            sb.Append(" tr th{ background-color:#d9d9d9} ");
            sb.Append(" table {table-layout:auto}");
            sb.Append("</style>");
            return sb.ToString();
        }

        public string GetMailBodyTable(List<Model.T_FeedBack> etys)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<table>");
            sb.Append("<thead><tr><th style='width:5%'>Property</th><th style='width:10%'>Tenant Name</th><th style='width:15%'>Premise</th><th style='width:5%'>Lease Number</th><th style='width:5%'>Type</th><th style='width:5%'>Report&nbsp;Date</th><th style='width:5%'>Latest Conversation</th><th style='width:15%'>Subject</th><th style='width:35%'>Conversation</th></tr></thead>");

            for (int i = 0; i < etys.Count; i++)
            {
                sb.Append("<tr>");
                // property code
                sb.Append("<td>");
                if (Helper.Aid.DbNull2Str(etys[i].LEASENUMBER) != "")
                {
                    sb.Append(etys[i].LEASENUMBER.Substring(0, 4));
                }
                sb.Append("</td>");
                // customer name
                sb.Append("<td>");
                sb.Append(etys[i].CustomerName);
                sb.Append("</td>");

                // premise
                sb.Append("<td>");
                sb.Append(etys[i].Premise);
                sb.Append("</td>");

                // lease number
                sb.Append("<td>");
                sb.Append(etys[i].LEASENUMBER);
                sb.Append("</td>");
                // type
                sb.Append("<td>");
                if (etys[i].TYPE == 500)
                {
                    sb.Append("Lease");
                }
                else if (etys[i].TYPE == 600)
                {
                    sb.Append("EM"); //EM related
                }
                else if (etys[i].TYPE == 700)
                {
                    sb.Append("Billing"); //
                }
                else if (etys[i].TYPE == 900)
                {
                    sb.Append("other");
                }
                sb.Append("</td>");

                //report date
                sb.Append("<td>");
                sb.Append(Helper.Aid.Date2Str(etys[i].CreateDate, Helper.Aid.DateFmt.Standard));
                sb.Append("</td>");

                // Latest Conversation
                sb.Append("<td>");
                if (etys[i].feedback_res.Count > 0)
                    sb.Append(Helper.Aid.Date2Str(etys[i].feedback_res[etys[i].feedback_res.Count - 1].CreateDate, Helper.Aid.DateFmt.Standard));
                sb.Append("</td>");

                // subject(fbs)
                sb.Append("<td>");
                sb.Append(etys[i].Title);
                sb.Append("</td>");

                // Conversation
                sb.Append("<td>");
                sb.Append("<strong>" + etys[i].CreateBy + " [" + Helper.Aid.Date2Str(etys[i].CreateDate, Helper.Aid.DateFmt.YmdHm) + "]</strong>:<br/>" + etys[i].Detail + "<br/>");
                for (int j = 0; j < etys[i].feedback_res.Count; j++)
                {
                    Model.T_Feedback_Res etyres = etys[i].feedback_res[j];
                    sb.Append("<strong>" + etyres.CreateBy + " [" + Helper.Aid.Date2Str(etyres.CreateDate, Helper.Aid.DateFmt.YmdHm) + "]</strong>:<br/>" + etyres.Detail.Replace("\n", "<br/>") + "<br/>");
                }
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return sb.ToString();
        }

        public string GenMailBodyTable(List<Model.T_FeedBack> etys)
        {
            string debugText = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            IEnumerable<IGrouping<string, Model.T_FeedBack>> propGroup = etys.GroupBy(s => s.LEASENUMBER.Substring(0, 4));
            sb.Append(Css2());
            sb.Append("<table>");
            foreach (IGrouping<string, Model.T_FeedBack> item in propGroup)
            {                
                // 头部1行(property code)
                sb.Append("<thead class='theader'>");
                sb.Append("<tr>");
                sb.Append("<th colspan='8'>");
                sb.Append(item.Key);
                sb.Append("</th>");
                sb.Append("</tr>");               
                // 标题1行
                sb.Append("<tr>");
                sb.Append(" <th style='width:6%'>Report Date</th>");
                sb.Append(" <th style='width:8%'>Tenant Name</th>");
                sb.Append(" <th style='width:18%'>Subject</th>");
                sb.Append(" <th style='width:18%'>Conversation</th>");                
                sb.Append(" <th style='width:10%'>Last Update</th>");
                sb.Append(" <th style='width:8%'>Completion Date</th>");
                sb.Append(" <th style='width:5%'>Status</th>");
                sb.Append(" <th style='width:7%'>Follow By</th>");
                //sb.Append(" <th style='width:5%'>Type</th>");
                //sb.Append(" <th style='width:12%'>Premises</th>");
                //sb.Append(" <th style='width:8%'>Lease number</th>");                
                sb.Append("</tr>");
                sb.Append("</thead>");
                // 内容n行
                foreach (Model.T_FeedBack ety in item)
                {
                    debugText += string.Format("FeedbackId:'{0}',Title:'{1}',CreateDate:'{2}'", ety.FeedbackId.ToString(), ety.Title, ety.CreateDate) + "\r\n\t";
                    if (ety.STATUS != 400)
                    {
                        if (ety.NoReply)
                            sb.Append("<tr class='open_status'>");
                        else
                            sb.Append("<tr>");
                    }
                    
                    //Repoert Date
                    sb.Append("<td>").Append(Helper.Aid.Date2Str(ety.CreateDate, Helper.Aid.DateFmt.Standard)).Append("</td>");
                    //Tenant Name
                    sb.Append("<td>").Append(ety.CustomerName).Append("</td>");
                    //Subject
                    sb.Append("<td>").Append(ety.Title).Append("</td>");
                    // Conversation
                    sb.Append("<td>");
                    sb.Append("<strong>" + ety.CreateBy + " [" + Helper.Aid.Date2Str(ety.CreateDate, Helper.Aid.DateFmt.YmdHm) + "]</strong>:<br/>" + ety.Detail + "<br/>");
                    for (int j = 0; j < ety.feedback_res.Count; j++)
                    {
                        Model.T_Feedback_Res etyres = ety.feedback_res[j];
                        sb.Append("<strong>" + etyres.CreateBy + " [" + Helper.Aid.Date2Str(etyres.CreateDate, Helper.Aid.DateFmt.YmdHm) + "]</strong>:<br/>" + etyres.Detail.Replace("\n", "<br/>") + "<br/>");
                    }                  
                    //Last Update                    
                    sb.Append("<td>").Append(ety.Last_Update == null ? "" : Helper.Aid.Date2Str(ety.Last_Update.Value, Helper.Aid.DateFmt.Standard)).Append("</td>");
                    //Completion Date closed状态的日期
                    sb.Append("<td>").Append(ety.STATUS == 400 ? ety.Last_Update == null ? "" : Helper.Aid.Date2Str(ety.Last_Update.Value, Helper.Aid.DateFmt.Standard) : "").Append("</td>");
                    //Status                   
                    if (ety.STATUS==400)                    
                        sb.Append("<td>Closed</td>");                    
                    else 
                    {
                        if (ety.NoReply)
                            sb.Append("<td>Open</td>");
                        else
                            sb.Append("<td>Replied</td>");    
                    }
                    
                    //Follow By
                    sb.Append("<td>");
                    if (ety.feedback_res.Count > 0 && ety.feedback_res[ety.feedback_res.Count - 1].ReplyType == "S") //如果最后一条是后台人员的回复
                        sb.Append(ety.feedback_res[ety.feedback_res.Count - 1].CreateBy);
                    sb.Append("</td>");
                    //Type
                    //sb.Append("<td>");
                    //if (ety.TYPE == 500)
                    //{
                    //    sb.Append("Lease");
                    //}
                    //else if (ety.TYPE == 600)
                    //{
                    //    sb.Append("EM"); //EM related
                    //}
                    //else if (ety.TYPE == 700)
                    //{
                    //    sb.Append("Billing"); //
                    //}
                    //else if (ety.TYPE == 900)
                    //{
                    //    sb.Append("other");
                    //}
                    //sb.Append("</td>");
                    //Premises                    
                    //sb.Append("<td>").Append(ety.Premise).Append("</td>");
                    //Lease number
                    //sb.Append("<td>").Append(ety.LEASENUMBER).Append("</td>");                   
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table>");
            Commom.Log.WriteLog("-- eMail body:\r\n\t" + debugText);

            return sb.ToString();
        }
    }

    public class FeedbackSummaryRpt2 : MailFactory
    {
        protected int tid = 3;                              //按照目前的设计，这里的taskid必须已经在数据库task表里存在，相当于常量。
        protected string btype = "ALERT_FEEDBACK_REPORT_2"; //按照目前的设计，相当于常量。

        private string param_type = "";
        int interval = 13; // in terms of day. default=13
        List<Model.T_FeedBack> etysClosed = new List<Model.T_FeedBack>();

        public override void Execute(bool skipcheck = false)
        {
            /// ALERT_[sth.]=>业务标题, PARAM_NAME=>部门,PARAM_VALUE=> NOTIFICATION_[sth.]_ADMIN/FIN/EM/MKT
            /// ALERT_[sth.]_PERIOD=>执行周期，PARAM_NAME=>null            
            /// 0.获取interval(period)
            /// 1.获取ALERT_[sth.]业务下所有PARAM_VALUE配置,就是NOTIFICATION_[sth.]信息
            /// 2.准备数据
            /// 3.遍历所有NOTIFICATION_[sth.]，发送邮件内容            
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                BizFeedback bz = new BizFeedback(tx);
                interval = bz.GetInterval(btype);

                bool pass_check = true;
                if (!skipcheck && (!bz.Check(tid, interval)))//如果不跳过检查并且检查触发条件不通过                                    
                {
                    pass_check = false; 
                }

                if (pass_check)
                {
                    //获取配置
                    List<system.SysMailParamsModel> mps1 = bz.GetParams(btype);// this.GetParams(BIZ_TYPE);

                    List<Model.T_FeedBack> etys = bz.GetReplied(interval);  // bz.GetFbsRes(interval);//获取指定日期内回复数据  只取非400（closed）的数据
                    etys.AddRange(bz.GetNoReply(interval)); //获取从未回复的数据               
                    etysClosed = bz.GetClosed(interval);   //获取指定期限内关闭的数据           
                    etys.AddRange(etysClosed);

                    if (etys.Count == 0 && etysClosed.Count == 0)
                    {
                        Commom.Log.WriteLog("abort send feedback summary report,for no record exists.");
                    }
                    else
                    {
                        bz.Rig(etys); //重新组装数据，给每条feedback加上customer_name信息

                        //分别取各配置值
                        for (int i = 0; i < mps1.Count; i++) //第一层循环
                        {
                            this.param_type = mps1[i].PARAM_VALUE;
                            //取部门值                    
                            string dept = Helper.Aid.DbNull2Str(mps1[i].PARAM_NAME); //这里的PARAM_NAME是部门值
                            if (dept != "") // 如果部门值不为空
                            {
                                List<Model.T_FeedBack> outs = new List<Model.T_FeedBack>();
                                if (dept.ToUpper() == "MKT")
                                    outs = etys.FindAll(x => { return x.TYPE == 500 || x.TYPE == 900; });
                                else if (dept.ToUpper() == "EM")
                                    outs = etys.FindAll(x => { return x.TYPE == 600; });
                                else if (dept.ToUpper() == "FIN")
                                    outs = etys.FindAll(x => { return x.TYPE == 700; });
                                else
                                    outs = etys;

                                /// -- start -- 排序
                                outs = outs.OrderBy(x => x.LEASENUMBER.Substring(0, 4)).ThenBy(y => y.TYPE).ThenBy(z => z.CreateDate).ToList();
                                /// --  end  --

                                if (outs.Count > 0)
                                {

                                    //以下过滤properties
                                    Dictionary<string, string> receipiant = new Dictionary<string, string>();
                                    var q = mps1[i].notis.GroupBy(x => x.PARAM_VALUE); // 对收件人分组，生成一个字典列表，key为收件人，value为prop列表，包含default
                                    foreach (IGrouping<string, system.SysMailParamsModel> item in q)
                                    {
                                        receipiant.Add(item.Key, "");
                                        foreach (system.SysMailParamsModel ety in item.ToList())
                                        {
                                            receipiant[item.Key] += ety.PARAM_NAME + ",";
                                        }
                                        receipiant[item.Key] = receipiant[item.Key].Substring(0, receipiant[item.Key].Length - 1);
                                    }

                                    List<Model.T_FeedBack> fbs = new List<Model.T_FeedBack>();
                                    foreach (var item in receipiant)
                                    {
                                        List<string> tos = new List<string>();
                                        if (item.Value.Contains("DEFAULT")) //如果含有default，则所有内容都发布                                        
                                            fbs = outs;
                                        else
                                            fbs = outs.FindAll(x => { return item.Value.Contains(x.LEASENUMBER.Substring(0, 4)); });

                                        if (fbs.Count > 0)
                                        {
                                            Commom.Log.WriteLog("-- start -- feedback summary report with dept '" + dept + "' for " + btype);
                                            tos.Add(item.Key);
                                            SendOut(fbs, tos);
                                            Commom.Log.WriteLog("--  end  -- feedback summary report");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bz.UpdateResult(skipcheck, tid);
                }          
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();     
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }           
        }

        public override bool Check(int id)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<TaskModel> etys = bz.Retrieve<TaskModel>(x => x.Id == id);
                tx.CommitTrans();

                if (etys.Count <= 0)
                    return false;

                if (Helper.Aid.DbNull2Str(etys[0].Last_Run) == "")
                    return true;

                DateTime last = DateTime.Parse(Helper.Aid.Date2Str(DateTime.Parse(etys[0].Last_Run.ToString()), Helper.Aid.DateFmt.Standard));
                //如果当前日期与上次日期加上间隔日相等，则返回true，表示需要触发任务;否则返回false，不触发任务   
                DateTime now = DateTime.Now;
                if (now.Subtract(last).Days >= this.interval && now.Hour >= 8)
                    return true;
                else
                    return false;               
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                return false;
            }
        }

        public override List<system.SysMailParamsModel> GetParams(string type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 实际发送邮件
        /// </summary>
        /// <param name="fbs"></param>
        /// <param name="tos"></param>
        private void SendOut(List<Model.T_FeedBack> fbs, List<string> tos)
        {
            Commom.ExchangeMail em = new Commom.ExchangeMail();
            string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];            
            //string body = GetMailBody(fbs);
            if (fbs.Count>0)
            {
                string body = GetMailBody_v2(fbs);
                if (dev == "0")
                {
                    em.SendExchangeMail(tos, "[HutchisonAgent] Feedback Summary Report" , body);
                }
                else
                {
                    if (this.param_type.ToUpper().Contains("ADMIN"))
                    {
                        tos.Clear();
                        tos.Add("liwei@hwpg.com");
                        em.SendExchangeMail(tos, "[HutchisonAgent] Feedback Summary Report", body);    
                    }                    
                }

                string ftos = "";
                for (int i = 0; i < tos.Count; i++)
                {
                    ftos += tos[i] + ",";
                }
                Commom.Log.WriteLog("Send email of 'Feedback Summary Report' [" + fbs.Count.ToString() + " feedback(s)] to " + ftos);
            }            
        }

        private string GetMailBody_v2(List<Model.T_FeedBack> etys)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //sb.Append(Css());
            sb.Append("Dear All<br/>");
            sb.Append("<br/>");
            sb.Append("Below please find feedback summary of HutchisonAgent for your reference." + "<br/>");
            sb.Append("<br/>");
            etys = etys.OrderBy(x => x.LEASENUMBER.Substring(0, 4)).ThenByDescending(y => y.CreateDate).ToList();
            sb.Append(GenMailBodyTable(etys));
            sb.Append("<br/>"); 

            sb.Append("This e-mail serves as notification only, please do not reply to this email.	<br/>");
            sb.Append("HutchisonAgent System <br/>");
            sb.Append("(" + this.param_type + ")<br/>");
            if (System.Configuration.ConfigurationManager.AppSettings["dev"]=="1")            
                sb.Append("Sent from UAT");            
           
            return sb.ToString();
        }

        /// <summary>
        /// （废弃使用）
        /// </summary>
        /// <param name="etys"></param>
        /// <returns></returns>
        private string GetMailBody(List<Model.T_FeedBack> etys)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Css());
            sb.Append("Dear All<br/>");
            sb.Append("<br/>");
            sb.Append("Below please find feedback summary of HutchisonAgent for your reference." + "<br/>");
            sb.Append("<br/>");
            
            ///检索已回复的数据
            List<Model.T_FeedBack> etysHis = etys.FindAll(x => { return x.NoReply == false && x.STATUS != 400; }); //x.STATUS != 400 表示去除close状态
            if (etysHis.Count > 0)
            {
                sb.Append(string.Format("<span>The following comments have been replied in last {0} days.", interval.ToString()) + "</span><br/>");
                sb.Append(GetMailBodyTable(etysHis) + "<br/>");             
                sb.Append("<br/>");
            }

            List<Model.T_FeedBack> etysNew = etys.FindAll(x => { return x.NoReply == true && x.STATUS != 400; });//x.STATUS != 400 表示去除close状态
            if (etysNew.Count > 0)
            {
                sb.Append("<span>The following comments are pending for reply:</span><br/>");
                sb.Append(GetMailBodyTable(etysNew) + "<br/>");                
                sb.Append("<br/>");
            }

            List<Model.T_FeedBack> etysClosed = etys.FindAll(x => { return x.STATUS == 400; });//x.STATUS == 400 表示只取closed状态            
            if (etysClosed.Count > 0)
            {
                sb.Append(string.Format("<span>The following cases have been closed in last {0} days.", interval.ToString()) + "</span><br/>");
                sb.Append(GetMailBodyTable(etysClosed) + "<br/>");                
                sb.Append("<br/>");
            }

            etys = etys.OrderBy(x => x.LEASENUMBER.Substring(0, 4)).ThenByDescending(y => y.CreateDate).ToList();
            GenMailBodyTable(etys);

            sb.Append("This e-mail serves as notification only, please do not reply to this email.	<br/>");
            sb.Append("HutchisonAgent System <br/>");
            return sb.ToString();
        }
    }

    public class FeedbackSummaryRpt1 : FeedbackSummaryRpt2
    {
        public FeedbackSummaryRpt1() 
            : base()
        {
            this.tid = 1;
            this.btype = "ALERT_FEEDBACK_REPORT_1";
        }
    }

    public class NoRepliesIn7 : MailFactory
    {
        string paramType = "";
        int interval = 0;
        public NoRepliesIn7(string type)
        {
            this.paramType = type;
        }

        public List<Model.T_FeedBack> GetData()
        {
            string sql = "select a.*  from t_feedback a";
            sql += "  inner join (select a.feedbackid  from t_feedback_res a";
            sql += "  inner join (select feedbackId, max(createdate) as maxdate";
            sql += "  from t_feedback_res  where replytype = 'L' group by FeedbackId) aa";
            sql += "  on a.feedbackid = aa.feedbackid";
            sql += "  and a.createdate = aa.maxdate";
            sql += "  where 1 = 1  and trunc(a.createdate) >= trunc(sysdate) - " + this.interval.ToString() + " ) c";
            sql += " on a.feedbackid = c.feedbackid";
            sql += " union select a.*";
            sql += " from t_feedback a";
            sql += " left join t_feedback_res b  on a.feedbackid = b.feedbackid";
            sql += " where 1 = 1 and trunc(a.createdate) >= trunc(sysdate) - " + this.interval.ToString() + "  and b.id is null";

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.T_FeedBack> etys = bz.Retrieve<Model.T_FeedBack>(sql);
                etys.Sort((x, y) =>
                {
                    if (x.FeedbackId > y.FeedbackId)
                        return 1;
                    else if (x.FeedbackId < y.FeedbackId)
                        return -1;
                    else
                        return 0;
                });

                for (int i = 0; i < etys.Count; i++)
                {                    
                    List<Model.T_Feedback_Res> etysd = bz.Retrieve<Model.T_Feedback_Res>(x => x.FeedbackId == etys[i].FeedbackId);
                    etysd.Sort((x, y) =>
                    {
                        if (x.Id > y.Id)
                            return 1;
                        else if (x.Id < y.Id)
                            return -1;
                        else
                            return 0;
                    });
                    etys[i].feedback_res = etysd;
                }
                tx.CommitTrans();
                return etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                throw err;
            }
        }

        public override void Execute(bool skipcheck = false)
        {
            try
            {
                List<system.SysMailParamsModel> paras = this.GetParams(this.paramType);
                if (!skipcheck)//如果不跳过检查                
                    if (!this.Check(2)) return;

                List<Model.T_FeedBack> etys = this.GetData();
                this.AttachCustomer(etys);
                List<Model.T_FeedBack> etysout = new List<Model.T_FeedBack>();
                for (int i = 0; i < paras.Count; i++) //第一层过滤
                {
                    List<string> tos = new List<string>();
                    if (paras[i].PARAM_NAME == "ADMIN")
                    {
                        for (int j = 0; j < paras[i].notis.Count; j++)
                            tos.Add(paras[i].notis[j].PARAM_VALUE);
                        etysout = etys;
                    }
                    else if (paras[i].PARAM_NAME == "MKT")
                    {
                        etysout = etys.FindAll(x => { return x.TYPE == 500 || x.TYPE == 900; });
                        etysout = etysout.FindAll(y => { return y.PROPERTYCODE == paras[i].PARAM_NAME; });
                        for (int k = 0; k < paras[i].notis.Count; k++)
                            tos.Add(paras[i].notis[k].PARAM_VALUE);
                    }
                    else if (paras[i].PARAM_NAME == "EM")
                    {
                        etysout = etys.FindAll(x => { return x.TYPE == 600; });
                        etysout = etysout.FindAll(y => { return y.PROPERTYCODE == paras[i].PARAM_NAME; });
                        for (int k = 0; k < paras[i].notis.Count; k++)
                            tos.Add(paras[i].notis[k].PARAM_VALUE);
                    }
                    else if (paras[i].PARAM_NAME == "FIN")
                    {
                        etysout = etys.FindAll(x => { return x.TYPE == 700; });
                        etysout = etysout.FindAll(y => { return y.PROPERTYCODE == paras[i].PARAM_NAME; });
                        for (int k = 0; k < paras[i].notis.Count; k++)
                            tos.Add(paras[i].notis[k].PARAM_VALUE);
                    }

                    Commom.ExchangeMail em = new Commom.ExchangeMail();
                    string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
                    string body = GetMailBody(etysout);

                    if (dev == "0")
                        em.SendExchangeMail(tos, "[HutchisonAgent] Feedback Summary Report", body);
                    else
                    {
                        em.SendExchangeMail("liwei@hwpg.com", "[HutchisonAgent] Feedback Summary Report", body);
                        Commom.Log.WriteLog("Send email of 'Feedback Summary Report' to liwei@hwpg.com");
                    }
                }
                UpdateResult(skipcheck);
            }
            catch (Exception err)
            {
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
        }

        public override bool Check(int id)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<TaskModel> etys = bz.Retrieve<TaskModel>(x => x.Id == id);
                tx.CommitTrans();

                if (etys.Count <= 0)
                    return false;

                if (etys[0].Last_Run == null)
                    return true;

                DateTime last = DateTime.Parse(etys[0].Last_Run.ToString());
                if (Helper.Aid.Date2Str(DateTime.Now, Helper.Aid.DateFmt.Standard) == Helper.Aid.Date2Str(last.AddDays(this.interval), Helper.Aid.DateFmt.Standard))
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                return false;
            }
        }

        public override List<system.SysMailParamsModel> GetParams(string type)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                string typeStr = type + "_PERIOD";
                List<system.SysMailParamsModel> etys = bz.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == typeStr);
                if (etys.Count == 0)
                {
                    throw new Exception("Param_Type not found for[" + type + "]");
                }
                else
                    interval = Helper.Aid.DbNull2Int(etys[0].PARAM_VALUE);

                //获取所有notification列表及其配置                
                etys = bz.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == type);
                for (int i = 0; i < etys.Count; i++)
                {                    
                    List<system.SysMailParamsModel> etysb = bz.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == etys[i].PARAM_VALUE);
                    etys[i].notis = etysb;
                }
                tx.CommitTrans();

                return etys;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                throw err;
            }
        }

        private string GetMailBody(List<Model.T_FeedBack> etys)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Css());
            sb.Append("Dear All<br/>");
            sb.Append("<br/>");
            sb.Append("please find the below feedback informations of HutchisonAgent for your reference." + "<br/>");
            sb.Append("<br/>");

            sb.Append("The following feedbacks are those which haven't replied in " + interval.ToString() + " days . <br/>");
            sb.Append(GetMailBodyTable(etys));
            sb.Append("<br/>");

            sb.Append("This e-mail serves as notification only, please do not reply to this email.	<br/>");
            sb.Append("HutchisonAgent System <br/>");
            return sb.ToString();
        }

        private List<string> Match(List<system.SysMailParamsModel> etys, string prop)
        {
            List<string> tos = new List<string>();
            for (int m = 0; m < etys.Count; m++) //过滤property
            {
                if (etys[m].PARAM_NAME == "DEFAULT")
                {
                    tos.Add(etys[m].PARAM_VALUE);
                }
                else
                {
                    if (etys[m].PARAM_NAME == prop) //如果第二层匹配到property
                    {
                        tos.Add(etys[m].PARAM_VALUE);
                    }
                }
            }
            return tos;
        }

        private void AttachCustomer(List<Model.T_FeedBack> etys)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                // 赋值customer name
                for (int j = 0; j < etys.Count; j++)
                {
                    Model.T_FeedBack ety = etys[j];                    
                    ety = bz.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == ety.FeedbackId)[0];

                    if (Helper.Aid.DbNull2Str(ety.LEASENUMBER) != "")
                    {                        
                        List<Model.Pone_Lm_Lease> etysLeases = bz.Retrieve<Model.Pone_Lm_Lease>(x => x.LEASE_NUMBER == ety.LEASENUMBER);

                        if (etysLeases.Count > 0)
                        {                            
                            List<Model.Pone_Customer> etysCusts = bz.Retrieve<Model.Pone_Customer>(x => x.CUSTOMER_NUMBER == etysLeases[0].CUSTOMER_NUMBER);
                            if (etysCusts.Count == 0)
                                throw new Exception("customer number not found for feedbackid " + etys[j].FeedbackId.ToString());
                            etys[j].CustomerName = etysCusts[0].CUSTOMER_NAME;

                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name1) != "")
                                etys[j].Premise += etysLeases[0].Premise_Name1;
                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name2) != "")
                                etys[j].Premise += "," + etysLeases[0].Premise_Name2;
                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name3) != "")
                                etys[j].Premise += "," + etysLeases[0].Premise_Name3;
                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name4) != "")
                                etys[j].Premise += "," + etysLeases[0].Premise_Name4;
                        }
                    }
                    else
                    {
                        etys[j].CustomerName = "";
                    }
                    etys[j].LEASENUMBER = ety.LEASENUMBER;
                    etys[j].CreateDate = ety.CreateDate;
                    etys[j].TYPE = ety.TYPE;
                    etys[j].Title = ety.Title;
                    etys[j].Detail = ety.Detail;
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                throw err;
            }
        }

        /// <summary>
        /// 更新任务结果，下次检查时使用
        /// </summary>
        private void UpdateResult(bool skip = false)
        {
            if (skip) //如果是手动执行的话，跳过（不做更新Last_Run的值）
                return;

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<TaskModel> etys2 = bz.Retrieve<TaskModel>(x => x.Id == 2);
                etys2[0].Last_Run = DateTime.Now;
                bz.Update(etys2[0]);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                throw;
            }
        }
    }

    public class BizFeedback : Helper.DbConsole
    {
        public BizFeedback(Helper.DbTx tx) : base(tx) { }

        public int GetInterval(string type)
        {
            try
            {
                string param_type = type + "_PERIOD";               
                List<system.SysMailParamsModel> etys = this.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == param_type);

                if (etys.Count > 0)
                    return Helper.Aid.DbNull2Int(etys[0].PARAM_VALUE);
                else
                    return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
       
        /// <summary>
        /// 获取关闭的回复 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public List<Model.T_FeedBack> GetClosed(int interval)
        {
            try
            {                
                List<Model.T_FeedBack> etys = this.Retrieve<Model.T_FeedBack>(x => x.STATUS == 400);
                List<Model.T_FeedBack> outs = etys.FindAll(x =>
                {
                    if (x.Last_Update != null)
                        return DateTime.Now.Subtract(x.Last_Update.Value).Days <= interval;
                    else
                        return false;
                });
                return outs;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// Get feedbacks which Have been replied
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public List<Model.T_FeedBack> GetReplied(int interval)
        {
            try
            {
                string dstr = Helper.Aid.Date2Str(DateTime.Now.AddDays(-interval), Helper.Aid.DateFmt.Standard);
                string sql = "select a.* ";
                sql += " from t_feedback_res a ";
                sql += " inner join (select feedbackid, max(b.createdate) as maxdate ";
                sql += " from t_feedback_res b ";
                sql += " where 1 = 1 ";
                //sql += " and CreateDate >= to_date('" + dstr + "', 'yyyy-mm-dd') ";
                sql += " and b.approve = 'A'";
                sql += " group by feedbackid) bb ";
                sql += " on a.feedbackid = bb.feedbackid ";
                sql += " and a.createdate = bb.maxdate ";
                sql += " where 1 = 1 ";
                sql += " and a.approve='A' and ReplyType <> 'L' ";

                List<Model.T_Feedback_Res> etys1res = this.Retrieve<Model.T_Feedback_Res>(sql);

                List<Model.T_FeedBack> etys0 = new List<Model.T_FeedBack>();
                for (int i = 0; i < etys1res.Count; i++)
                {
                    //取主表数据                    
                    List<Model.T_FeedBack> etys1 = this.Retrieve<Model.T_FeedBack>(x => x.FeedbackId == etys1res[i].FeedbackId);   //肯定有值，不必做检查。

                    //取子表数据                    
                    List<Model.T_Feedback_Res> etys2res = this.Retrieve<Model.T_Feedback_Res>(x => x.FeedbackId == etys1res[i].FeedbackId); //肯定有值，不必做检查。
                    etys2res.Sort((x, y) => { if (x.Id > y.Id) return 1; else if (x.Id < y.Id) return -1; else return 0; }); //处理排序

                    etys1[0].feedback_res = etys2res;
                    Model.T_Feedback_Res etyRes = etys2res[etys2res.Count - 1];//取最后一条feedback_res;
                    if (etyRes.Approve == "A" && etyRes.ReplyType == "S") //如果是审批了并且是后台人员回复的                    
                        etys1[0].NoReply = false;// 这里标识该feedback是已经回复的
                    etys0.Add(etys1[0]);
                }

                etys0 = etys0.FindAll(x => { return x.STATUS != 400; }); // 只取非400（closed）的数据
                //etys0=etys0.FindAll(x=>x.feedback_res.FindAll(y=>y.up
                return etys0;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// Get feedbacks which Pending for reply
        /// </summary>
        /// <returns></returns>
        public List<Model.T_FeedBack> GetNoReply(int interval = 0)
        {
            try
            {
                string sql = "select 'fbrs', a.feedbackid, a.detail,a.createdate";
                sql += " from t_feedback_res a";
                sql += " inner join (select feedbackid, max(b.createdate) as maxdate  from t_feedback_res b  where 1 = 1 and b.approve='A' group by feedbackid) bb";
                sql += " on a.feedbackid = bb.feedbackid  and a.createdate = bb.maxdate ";
                sql += "  inner join t_feedback aa on a.feedbackid = aa.feedbackid";
                sql += " where 1 = 1 and ReplyType = 'L' and aa.status<>'400'";                                
                sql += " union";
                sql += " select 'fbs', aa.feedbackid, aa.detail,aa.createdate  from t_feedback aa";
                sql += "  left join t_feedback_res bb";
                sql += " on aa.feedbackid = bb.feedbackid";
                sql += " where bb.feedbackid is null and aa.status<>'400'";

                System.Data.DataTable dt = this.Query2dt(sql);
                List<Model.T_FeedBack> etys = new List<Model.T_FeedBack>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Model.T_FeedBack ety = new Model.T_FeedBack();
                    ety.FeedbackId = int.Parse(dt.Rows[i]["FeedbackId"].ToString());
                    ety.NoReply = true;
                    ety.CreateDate = DateTime.Parse(dt.Rows[i]["CreateDate"].ToString());
                    etys.Add(ety);
                }
                //etys = etys.FindAll(x => { return x.STATUS != 400; });      
                etys = etys.FindAll(x => { return DateTime.Now.Subtract(x.CreateDate).TotalDays >= interval; });
                return etys;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// 获取数据库里Mail_Params参数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<system.SysMailParamsModel> GetParams(string type)
        {
            try
            {                
                string typeStr = type + "_PERIOD";
                List<system.SysMailParamsModel> etys = this.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == typeStr);
                if (etys.Count == 0)
                {
                    throw new Exception("Param_Type not found for[" + type + "]");
                }

                //获取所有notification列表及其配置                
                etys = this.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == type);
                for (int i = 0; i < etys.Count; i++)
                {                    
                    List<system.SysMailParamsModel> etysb = this.Retrieve<system.SysMailParamsModel>(x => x.PARAM_TYPE == etys[i].PARAM_VALUE);
                    etys[i].notis = etysb;
                }

                return etys;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// 组装数据（客户信息）
        /// </summary>
        /// <param name="etys"></param>
        public void Rig(List<Model.T_FeedBack> etys)
        {
            try
            {
                // 赋值customer name
                for (int j = 0; j < etys.Count; j++)
                {
                    Model.T_FeedBack ety = etys[j];                    
                    List<Model.T_FeedBack> tetys = this.Retrieve<Model.T_FeedBack>(x=>x.FeedbackId==ety.FeedbackId);
                    if (tetys.Count > 0) //如果存在
                        ety = tetys[0];
                    else
                        continue;

                    if (Helper.Aid.DbNull2Str(ety.LEASENUMBER) != "")
                    {                        
                        List<Model.Pone_Lm_Lease> etysLeases = this.Retrieve<Model.Pone_Lm_Lease>(x => x.LEASE_NUMBER == ety.LEASENUMBER);

                        if (etysLeases.Count > 0)
                        {                            
                            List<Model.Pone_Customer> etysCusts = this.Retrieve<Model.Pone_Customer>(x => x.CUSTOMER_NUMBER == etysLeases[0].CUSTOMER_NUMBER);
                            if (etysCusts.Count == 0)
                                throw new Exception("customer number not found for feedbackid " + etys[j].FeedbackId.ToString());
                            etys[j].CustomerName = etysCusts[0].CUSTOMER_NAME;

                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name1) != "")
                                etys[j].Premise += etysLeases[0].Premise_Name1;
                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name2) != "")
                                etys[j].Premise += "," + etysLeases[0].Premise_Name2;
                            if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name3) != "")
                                etys[j].Premise += "," + etysLeases[0].Premise_Name3;
                            //if (Helper.Aid.DbNull2Str(etysLeases[0].Premise_Name4) != "")
                            //    etys[j].Premise += "," + etysLeases[0].Premise_Name4;

                            if (etys[j].Premise.Length > 1)
                                etys[j].Premise = etys[j].Premise.Substring(0, etys[j].Premise.Length - 1);
                        }
                    }
                    else
                    {
                        etys[j].CustomerName = "";
                    }
                    etys[j].LEASENUMBER = ety.LEASENUMBER;
                    etys[j].CreateDate = ety.CreateDate;
                    etys[j].CreateBy = ety.CreateBy;
                    etys[j].TYPE = ety.TYPE;
                    etys[j].Title = ety.Title;
                    etys[j].Detail = ety.Detail;
                    etys[j].Last_Update = ety.Last_Update;
                    etys[j].STATUS = ety.STATUS;                    
                    List<Model.T_Feedback_Res> etysDtl = this.Retrieve<Model.T_Feedback_Res>(x => x.FeedbackId == etys[j].FeedbackId && x.Approve == "A");
                    etysDtl.Sort((x, y) => x.Id < y.Id ? -1 : 0);
                    etys[j].feedback_res = etysDtl;
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// 更新执行结果
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="taskid"></param>
        public void UpdateResult(bool skip = false, int taskid = 0)
        {
            if (skip) //如果是手动执行的话，跳过（不做更新Last_Run的值）
                return;
            try
            {
                if (Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["dev"]) == "1") //如果是开发环境，不做更新
                    return;
                                
                List<TaskModel> etys2 = this.Retrieve<TaskModel>(x => x.Id == taskid);

                if (etys2.Count > 0)
                {
                    etys2[0].Last_Run = DateTime.Now;
                    this.Update(etys2[0]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="interval"></param>
        /// <returns>返回true表示需要执行任务，反之返回false</returns>
        public bool Check(int id, int interval = 50)
        {
            try
            {               
                List<TaskModel> etys = this.Retrieve<TaskModel>(x => x.Id == id);
                etys = this.Retrieve<TaskModel>(x => x.Id == id);

                if (etys.Count <= 0)
                    return false;

                if (Helper.Aid.DbNull2Str(etys[0].Last_Run) == "")
                    return true;

                DateTime last = DateTime.Parse(Helper.Aid.Date2Str(DateTime.Parse(etys[0].Last_Run.ToString()), Helper.Aid.DateFmt.Standard));
                //如果当前日期与上次日期加上间隔日相等，则返回true，表示需要触发任务;否则返回false，不触发任务   
                DateTime now = DateTime.Now;
                if (now.Subtract(last).Days >= interval && now.Hour >= 8 && Helper.Aid.DbNull2Str(etys[0].Invalid) != "Y")
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
                return false;
            }
        }
    }

    public class TaskModel
    {
        public const string TABLE_NAME = "Task";

        [DbField(true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Start_Date { get; set; }
        public Nullable<DateTime> Last_Run { get; set; }
        public string Invalid { get; set; }
    }

    #endregion    
}