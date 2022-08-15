using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;

namespace PropertyOneAppWeb.Web
{
    public partial class changemail : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "change")  //变更邮箱
                    {
                        string userid = Session["loginname"].ToString();
                        string email = Request.Form["email"];
                        string contactname = Request.Form["contactname"];
                        string phone = Request.Form["phone"];
                        string code = Request.Form["code"];

                        if (code != Session["code"].ToString())
                        {
                            throw new Exception("Verification code error");
                        }

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_UpdateEmail(userid, email, contactname, phone);

                        JsonApi.JsonApi_UpdateEmail_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_UpdateEmail_Res>(json);
                        if (res.result == "100")
                        {
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception(res.message);
                        }
                    }
                    if (action == "check")
                    {
                        string statementnum = Request.Form["statementnum"];
                        string balance = Request.Form["balance"];
                        string expiredate = Request.Form["expiredate"];
                        string rental = Request.Form["rental"];
                        string leasenum = Session["loginname"].ToString();
                        expiredate = GlobalUtil.ConvertDateType(expiredate, "dd-MM-yyyy", "yyyy-MM-dd");

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_VerifyUser(statementnum, balance, leasenum, expiredate, rental);
                        JsonApi.JsonApi_VerifyUser_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_VerifyUser_Res>(json);
                        if (res.result == "100")  //查询成功
                        {
                            result = res.check;
                        }
                        else
                        {
                            throw new Exception(res.message);
                        }
                    }
                    else if (action == "getcode")
                    {
                        string leasenum = Session["loginname"].ToString();
                        string email = Request.Form["newemail"];
                        Random rnd = new Random();
                        string vCode = GlobalUtil.MakePassword(6, rnd);
                        Session.Add("code", vCode);

                        ExchangeMail em = new ExchangeMail();
                        string html =
                                    "<html>" +
                                    "<head>" +
                                    "    <title>eStatement Verification code</title>" +
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
                                    "                Please find below the verification code for update email address in our website. (website:" +
                                    "                <a href=\"https://www.heal.com.hk\" target=\"_blank\">www.heal.com.hk</a>)" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                Verification code: " + vCode +
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
                                    "                請使用以下驗證碼重新設定郵箱地址。(網站: <a href=\"https://www.heal.com.hk\" target=\"_blank\">www.heal.com.hk</a>)" +
                                    "            </td>" +
                                    "        </tr>" +
                                    "        <tr class=\"table-blank\">" +
                                    "        </tr>" +
                                    "        <tr>" +
                                    "            <td>" +
                                    "                驗證碼: " + vCode +
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
                        em.SendMailBySmtp(email, Resources.Lang.Res_ChangeEmail, html);
                        result = "ok";
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