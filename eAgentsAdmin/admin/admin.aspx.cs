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
using Oracle.ManagedDataAccess.Client;

namespace PropertyOneAppWeb.admin
{
    public partial class admin : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {               
                string result = "";

                if (action == "verify")   //验证admin密码
                {
                    try
                    {
                        string psw = Request.Form["psw"];

                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_USER_NAME", "admin");
                        oh.AddParameter("P_PASSWORD", psw);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dt = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_LOGIN_CHECK");

                        int rowcount = dt.Rows.Count;
                        if (rowcount == 1)  //校验成功
                        {
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception("password error");
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
                else if (action == "RunNotification")   //运行 Notification of eStatement Service (Run once only)
                {
                    try
                    {
                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dt = (DataTable)oh.ExecuteStoredProcedure("FUN_GET_LEASE_USERS");
                        int rowcount = dt.Rows.Count;
                        if (rowcount > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                string tenantName = dr["TENANT_TRADE_NAME1"].ToString();
                                string tenantAddress = dr["BILLING_ADDRESS_1"].ToString() + " " + dr["BILLING_ADDRESS_2"].ToString() + " " + dr["BILLING_ADDRESS_3"].ToString();
                                string premises = dr["PREMISE_NAME1"].ToString() + " " + dr["PREMISE_NAME2"].ToString() + " " + dr["PREMISE_NAME3"].ToString();
                                string leaseNum = dr["LEASE_NUMBER"].ToString();

                                ExchangeMail em = new ExchangeMail();
                                string html =
                                        "<html>" +
                                        "<head>" +
                                        "    <title>Notification of eStatement Service</title>" +
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
                                        "                Password: "  +
                                        "            </td>" +
                                        "        </tr>" +
                                        "        <tr class=\"table-blank\">" +
                                        "        </tr>" +
                                        "        <tr>" +
                                        "            <td>" +
                                        "                For security, please be reminded to change your password after your first login." +
                                        "                You may also view all the eStatement under your portfolio by linking up all your" +
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
                                        "                密碼: "  +
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
                                em.SendMailBySmtp("test@hwpg.com", "eStatement password", html);
                            }
                        }
                        else
                        {
                            throw new Exception("no data found");
                        }

                        result = "ok";

                        
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
            this.lblConnection.Text = "数据库连接数：" + Helper.DbTx.ConnectionCount.ToString();
        }

        protected void btnTestConn_Click(object sender, EventArgs e)
        {
            string connStr = this.tbxConnStr.Text;
            if (!string.IsNullOrEmpty(connStr))               
            {
                Helper.DbTx tx = null;
                try
                {
                    tx = new Helper.DbTx(Helper.DbVendor.Oracle, connStr, false, true);
                    if (tx.Version.Length > 0)
                    {
                        lblConnection.Text = "success.";
                    }
                    else
                    {
                        lblConnection.Text = "failed.";
                    }
                    tx.CommitTrans();
                }
                catch (Exception)
                {
                    lblConnection.Text = "exception.";
                    tx.AbortTrans();                    
                }
            }
        }
    }
}