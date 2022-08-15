using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace PropertyOneAppWeb.Commom
{
    public class ExchangeMail
    {
        /// <summary>
        /// 通过Exchange发送邮件 
        /// </summary>
        public void SendExchangeMail(string to, string subject, string body, string bcc = "", string embedImage = "")
        {
            string demo = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["demo"]);
            MailMessage mm = new MailMessage();
            try
            {
                string from = System.Configuration.ConfigurationManager.AppSettings["mail_from"].ToString();
                string host = System.Configuration.ConfigurationManager.AppSettings["mail_host"].ToString();
                string username = System.Configuration.ConfigurationManager.AppSettings["mail_username"].ToString();
                string psw = System.Configuration.ConfigurationManager.AppSettings["mail_psw"].ToString();

                mm.From = new MailAddress(from);
                mm.To.Add(new MailAddress(to));
                mm.BodyEncoding = System.Text.Encoding.UTF8;
                mm.Subject = (demo == "1" ? subject + "<UAT>" : "");
                mm.Body = body;
                mm.IsBodyHtml = true;
                if (bcc != "")
                    mm.Bcc.Add(bcc);
                if (embedImage != "")
                {
                    AlternateView html = AlternateView.CreateAlternateViewFromString(body + "<br/><img src=\"cid:webd\">", null, "text/html");
                    LinkedResource lrImage = new LinkedResource(embedImage, "image/jpg");
                    lrImage.ContentId = "webd";
                    html.LinkedResources.Add(lrImage);
                    mm.AlternateViews.Add(html);
                }

                SmtpClient sc = new SmtpClient();
                sc.Host = host;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                sc.UseDefaultCredentials = true;
                sc.Credentials = new System.Net.NetworkCredential(username, psw);

                sc.Send(mm);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.StackTrace);
            }
            finally
            {
                mm.Dispose();
            }
        }

        /// <summary>
        /// 通过IIS的smtp发送邮件
        /// </summary>
        public void SendMailBySmtp(string to, string subject, string body)
        {
            MailMessage mm = new MailMessage();
            try
            {
                string from = System.Configuration.ConfigurationManager.AppSettings["mail_from"].ToString();
                string host = System.Configuration.ConfigurationManager.AppSettings["mail_host"].ToString();

                mm.From = new MailAddress(from);
                mm.To.Add(new MailAddress(to));
                mm.Bcc.Add(new MailAddress(from));
                mm.BodyEncoding = System.Text.Encoding.UTF8;
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = true;

                SmtpClient sc = new SmtpClient();
                sc.Host = host;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                sc.Send(mm);

                Log.WriteLog("Send mail success: " + "| From: " + from + "| To: " + to + "| Body: " + body);
            }
            catch
            {
                throw;
            }
            finally
            {
                mm.Dispose();
            }
        }

        public void SendExchangeMail(List<string> tos, string subject, string body, string bcc = "", string attachFilePath = "")
        {
            string demo = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["demo"]);
            MailMessage mm = new MailMessage();
            try
            {
                string from = System.Configuration.ConfigurationManager.AppSettings["mail_from"].ToString();
                string host = System.Configuration.ConfigurationManager.AppSettings["mail_host"].ToString();
                string username = System.Configuration.ConfigurationManager.AppSettings["mail_username"].ToString();
                string psw = System.Configuration.ConfigurationManager.AppSettings["mail_psw"].ToString();

                mm.From = new MailAddress(from);
                for (int i = 0; i < tos.Count; i++)
                    mm.To.Add(tos[i]);

                if (bcc != "")
                    mm.Bcc.Add(bcc);
                if (attachFilePath != "")
                    mm.Attachments.Add(new Attachment(attachFilePath));

                mm.BodyEncoding = System.Text.Encoding.UTF8;
                mm.Subject = (demo == "1" ? subject + "<UAT>" : "");
                mm.Body = body;
                mm.IsBodyHtml = true;

                SmtpClient sc = new SmtpClient();
                sc.Host = host;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                sc.UseDefaultCredentials = true;
                sc.Credentials = new System.Net.NetworkCredential(username, psw);

                sc.Send(mm);
            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }
}