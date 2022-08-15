using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;

namespace PropertyOneAppWeb.Public
{
    public partial class register : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try 
                {
                    if (action == "register")  //注册
                    {
                        string statementnum = Request.Form["statementnum"];
                        string balance = Request.Form["balance"];
                        string leasenum = Request.Form["leasenum"];
                        string expiredate = Request.Form["expiredate"];
                        string rental = Request.Form["rental"];
                        string email = Request.Form["email"];
                        string name = Request.Form["name"];
                        string phone = Request.Form["phone"];

                        expiredate = GlobalUtil.ConvertDateType(expiredate, "dd-MM-yyyy", "yyyy-MM-dd");

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_Register(statementnum, balance, leasenum, expiredate, email, rental, name, phone);

                        JsonApi.JsonApi_Register_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_Register_Res>(json);
                        if (res.result == "100")  //注册成功
                        {
                            result = "ok";

                            ExchangeMail em = new ExchangeMail();
                            string html =
                                    "<html>" +
                                    "<head>" +
                                    "    <title>eStatement password</title>" +
                                    "    <style type=\"text/css\">" +
                                    "        body" +
                                    "        {" +
                                    "            height: auto;" +
                                    "            background-color: #fbfbfb;" +
                                    "        }" +
                                    "        div" +
                                    "        {" +
                                    "            margin-left: auto;" +
                                    "            margin-right: auto;" +
                                    "            background: #fff;" +
                                    "            border: 1px solid #eee;" +
                                    "            border-radius: 4px;" +
                                    "            box-shadow: 0 1px 10px rgba(0,0,0,.04);" +
                                    "            width: 800px;" +
                                    "            padding: 20px 20px 20px 20px;" +
                                    "        }" +
                                    "        table" +
                                    "        {" +
                                    "            font-family: \"Segoe UI\" , \"Lucida Grande\" , Helvetica, Arial, \"Microsoft YaHei\" , FreeSans, Arimo, \"Droid Sans\" , \"wenquanyi micro hei\" , \"Hiragino Sans GB\" , \"Hiragino Sans GB W3\" , \"FontAwesome\" , sans-serif;" +
                                    "            font-weight: normal;" +
                                    "            line-height: 1.2;" +
                                    "            color: #333333;" +
                                    "            font-size: 1.2rem;" +
                                    "        }" +
                                    "        .table-blank" +
                                    "        {" +
                                    "            height: 20px;" +
                                    "        }" +
                                    "    </style>" +
                                    "</head>" +
                                    "<body>" +
                                    "    <div>" +
                                    "    <table>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Dear Tenant," +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Thank you for choosing our eStatement service. Please find below your password for" +
                                    "                your first login to our website." +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Password: " + res.temppsw +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                For security, please be reminded to change your password after your first login." +
                                    "                You may also view the eStatement under your portfolio by linking up all your" +
                                    "                leases via the website." +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Yours faithfully," +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Hutchison Estate Agents Limited" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr style=\"height: 120px;\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                親愛的租戶 :" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                感謝您選用我們的電子帳單服務。請使用下列的密碼作首次登入。" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                密碼: " + res.temppsw +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                為了安全起見, 請於首次登錄後更改密碼。您也可以將您的租務組合中的所有租約連結起來，以便日後查閱帳單。" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                和記地產代理有限公司 謹啟" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "    </table>" +
                                    "    </div>" +
                                    "</body>" +
                                    "</html>";
                            em.SendMailBySmtp(email, "eStatement password", html);
                        }
                        else
                        {
                            throw new Exception(res.message);
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
    }
}