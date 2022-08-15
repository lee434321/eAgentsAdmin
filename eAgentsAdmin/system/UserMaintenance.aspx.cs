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

namespace PropertyOneAppWeb.system
{
    public partial class UserMaintenance : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "21"))
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                if (action == "search")
                {
                    string result = "";
                    try
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);
                        string searchPhrase = Request.Form["searchPhrase"];
                        
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_SEARCH_PHRASE", searchPhrase);                        
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER");
                        result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch (Exception ex)
                    {
                        result = GlobalUtil.bootErrorStr;
                        Log.WriteLog(ex.Message + ex.StackTrace);
                    }
                    finally
                    {
                        Response.Write(result);
                        Response.End();
                    }
                }
                else if (action == "searchgroup")
                {
                    string result = "";
                    try
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);
                        
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);                        
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_GROUP");
                        result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch (Exception ex)
                    {
                        result = GlobalUtil.bootErrorStr;
                        Log.WriteLog(ex.Message + ex.StackTrace);
                    }
                    finally
                    {
                        Response.Write(result);
                        Response.End();
                    }
                }
                else if (action == "savenewuser")  //保存新的user 
                {
                    string result = "";
                    try
                    {
                        string saveinfo = Request.Form["saveinfo"];
                        string[] arraySaveInfo = saveinfo.Split('|');

                        string loginname = arraySaveInfo[0];
                        string status = arraySaveInfo[1];
                        string email = arraySaveInfo[2];
                        string phone = arraySaveInfo[3];
                        string arrayGroup = arraySaveInfo[4];

                        //生成8位随机密码
                        Random rnd = new Random();
                        string psw = GlobalUtil.MakePassword(8, rnd);

                        //新增User Group
                        //OracleHelper oa = new OracleHelper(GlobalUtilSystem.sdb_local);
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_LOGINNAME", loginname.ToLower()); //用户名写入数据库时都是小写。
                        oa.AddParameter("P_PASSWORD", psw);
                        oa.AddParameter("P_STATUS", status);
                        oa.AddParameter("P_EMAIL", email);
                        oa.AddParameter("P_PHONE", phone);
                        oa.AddParameter("P_GROUP", arrayGroup);
                        oa.AddParameter("P_CREATEBY", Session["loginname"].ToString());
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string ret = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER").ToString();

                        if (ret == "ok")
                        {                            
                            //发送密码给收件人
                            string mailBody = "<html><body>Your login name: " + loginname + "<br />  Your password: " + psw + "</body></html>";
                            ExchangeMail em = new ExchangeMail();                            
                            em.SendExchangeMail(email, "[HutchisonAgent] Internal User Account Registration", mailBody);
                            result = "ok";
                        }
                        else
                        {
                            if (ret.Contains("ORA-00001"))
                            {
                                throw new Exception("User name has been registered, please try another");
                            }
                            else
                            {
                                throw new Exception(ret);
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
                else if (action == "saveedituser")  //保存更新后的user 
                {
                    string result = "";
                    try
                    {
                        string saveinfo = Request.Form["saveinfo"];
                        string[] arraySaveInfo = saveinfo.Split('|');

                        string userid = arraySaveInfo[0];
                        string loginname = arraySaveInfo[1];
                        string status = arraySaveInfo[2];
                        string email = arraySaveInfo[3];
                        string phone = arraySaveInfo[4];
                        string arrayGroup = arraySaveInfo[5];

                        //更新User
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_USERID", userid);
                        oa.AddParameter("P_LOGINNAME", loginname.ToLower());
                        oa.AddParameter("P_STATUS", status);
                        oa.AddParameter("P_EMAIL", email);
                        oa.AddParameter("P_PHONE", phone);
                        oa.AddParameter("P_GROUP", arrayGroup);
                        oa.AddParameter("P_UPDATEBY", Session["loginname"].ToString());
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string ret = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.UPDATE_USER").ToString();

                        if (ret == "ok")
                        {
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception(ret);
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
                else if (action == "deluser")
                {
                    string result = "";
                    try
                    {
                        string userid = Request.Form["userid"];
                        
                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_USERID", userid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string ret = oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.DEL_USER").ToString();
                        if (ret == "ok")
                        {
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception(ret);
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
                else if (action == "searchoneuser")
                {
                    string result = "";
                    try
                    {
                        string userid = Request.Form["userid"];

                        DataModel.ModelUserInfo ModelUserInfo = new DataModel.ModelUserInfo();
                        ModelUserInfo.userid = userid;

                        //查询该userid对应的login name, email, phone, status
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_USERID", userid);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        
                        DataTable dtName = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_BY_USERID");

                        ModelUserInfo.loginname = dtName.Rows[0]["LOGINNAME"].ToString();
                        ModelUserInfo.email = dtName.Rows[0]["EMAIL"].ToString();
                        ModelUserInfo.phone = dtName.Rows[0]["PHONE"].ToString();
                        ModelUserInfo.status = dtName.Rows[0]["STATUS"].ToString();

                        //查询该userid对应的所有groupid
                        oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_USERID", userid);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtGroup = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_GROUP_BY_USERID");

                        List<DataModel.ModelUserInfo_GroupInfo> listGroupInfo = new List<DataModel.ModelUserInfo_GroupInfo>();
                        foreach (DataRow dr in dtGroup.Rows)
                        {
                            DataModel.ModelUserInfo_GroupInfo ModelUserInfo_GroupInfo = new DataModel.ModelUserInfo_GroupInfo();
                            ModelUserInfo_GroupInfo.groupid = Int32.Parse(dr["GROUPID"].ToString());
                            listGroupInfo.Add(ModelUserInfo_GroupInfo);
                        }
                        ModelUserInfo.groupinfo = listGroupInfo;

                        //Step4:生成json
                        result = JsonConvert.SerializeObject(ModelUserInfo);
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
}