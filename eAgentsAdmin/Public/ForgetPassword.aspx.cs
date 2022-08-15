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
    public partial class ForgetPassword : PageBase
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
                        Session.Add("leasenum", "");
                        Session.Add("vcode", "");
                        Session.Add("vmail", "");
                        Session.Add("oldpsw", "");

                        string leasenum = Request.Form["leasenum"];

                        ApiUtil api = new ApiUtil();
                        string json = api.Api_ForgetPsw(leasenum, "C");
                        JsonApi.JsonApi_ForgetPsw_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_ForgetPsw_Res>(json);
                        if (res.result == "100")
                        {
                            string email = res.email;
                            string vCode = res.checkcode;
                            Session["leasenum"] = leasenum;
                            Session["vcode"] = vCode;
                            Session["vmail"] = email;
                            Session["oldpsw"] = res.oldpsw;

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
                                        "                Dear User," +
                                        "            </td>" +
                                        "        </tr>" +
                                        "        <tr class=\"table-blank\">" +
                                        "        </tr>" +
                                        "        <tr>" +
                                        "            <td>" +
                                        "                Please find below the verification code for password reset in our website. (website:" +
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
                                        "                請使用以下驗證碼重新設定密碼。(網站: <a href=\"https://www.heal.com.hk\" target=\"_blank\">www.heal.com.hk</a>)" +
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
                            em.SendMailBySmtp(email, Resources.Lang.Res_ForgetPassword_Mail_0, html);

                            result = "ok";
                        }
                        else
                        {
                            throw new Exception(res.message);
                        }
                    }
                    else if (action == "changepsw")
                    {
                        string leaseNum = Session["leasenum"].ToString();
                        string vCode = Request.Form["code"];
                        string newpsw = Request.Form["newpsw"];
                        string sVCode = Session["vcode"].ToString();
                        string email = Session["vmail"].ToString();
                        string oldpsw = Session["oldpsw"].ToString();

                        if (vCode == sVCode)
                        {  //如果验证码正确, 开始更改密码
                            ApiUtil api = new ApiUtil();
                            string json = api.Api_ChangePsw(leaseNum, "C", oldpsw, newpsw);
                            JsonApi.JsonApi_ChangePsw_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_ChangePsw_Res>(json);
                            if (res.result == "100")
                            {
                                result = "ok";
                            }
                            else
                            {
                                throw new Exception(res.message);
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
    }
}