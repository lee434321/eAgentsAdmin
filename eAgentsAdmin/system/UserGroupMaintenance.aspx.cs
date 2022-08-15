using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
using System.Data;

namespace PropertyOneAppWeb.system
{
    public partial class UserGroupMaintenance : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
                Response.Redirect("../LoginSystem.aspx");

            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "22"))
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "search") //查找所有User Group
                    {
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);
                            string searchPhrase = Request.Form["searchPhrase"].ToString().Trim();

                            Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                            oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);

                            DataTable dtSelect = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_GROUP");
                            /// -- start --
                            DataTable dt2 = dtSelect.Clone();
                            if (!string.IsNullOrEmpty(searchPhrase))
                            {
                                var r = dtSelect.AsEnumerable().Where(t => t.Field<string>("GroupName").ToUpper().Contains(searchPhrase.ToUpper()) ||
                                    t.Field<string>("Dept").ToUpper().Contains(searchPhrase.ToUpper())).Select(d => d);
                                foreach (DataRow dr in r)
                                {
                                    dt2.ImportRow(dr);
                                }
                                dtSelect = dt2;
                            }
                            /// --  end  --                                                       
                            
                            result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                        }
                        catch (System.Threading.ThreadAbortException)
                        {
                            //do nothing
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(ex.Message + ex.StackTrace);
                            throw new Exception(GlobalUtil.bootErrorStr);
                        }
                    }
                    else if (action=="getusersingrp")
                    {
                        JsonRsp rsp = new JsonRsp();
                        Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);
                            string sp = Request.Form["searchPhrase"];
                            Helper.DbConsole bz = new Helper.DbConsole(tx);
                            int grpid = Aid.Null2Int(sp);
                            List<Sys_Login_System_Group> etys2 = bz.Retrieve<Sys_Login_System_Group>(x => x.GroupId == grpid); // tested 使用lambda表达式查询
                            if (etys2.Count==0) { }
                            /// -- start -- 
                            List<int> ins2 = new List<int>();
                            for (int k = 0; k < etys2.Count; k++)                            
                                ins2.Add(etys2[k].UserId);
                            List<Sys_Login_System> etys3 = bz.Retrieve<Sys_Login_System>(x => x.UserId.Sql_In(ins2)); //tested 使用lambda表达式查询
                            /// --  end  --
                            rsp.result = etys3;
                            tx.CommitTrans();
                            result = GlobalUtil.GenBootGridSystemFromList(etys3, current, rowCount);
                        }
                        catch (Exception err)
                        {
                            tx.AbortTrans();
                            rsp.err_code = -99;
                            rsp.err_msg = err.Message; 
                            result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                        }                        
                    }
                    else if (action == "searchauth") //查找所有Auth
                    {
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);

                            Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                            oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                            DataTable dtSelect = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTHORITY");
                            result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                        }
                        catch (System.Threading.ThreadAbortException)
                        {
                            //do nothing
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(ex.Message + ex.StackTrace);
                            throw new Exception(GlobalUtil.bootErrorStr);
                        }
                    }
                    else if (action == "searchproperty") //查找所有property
                    {
                        try
                        {
                            int current = Int32.Parse(Request.Form["current"]);
                            int rowCount = Int32.Parse(Request.Form["rowCount"]);

                            Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                            oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                            DataTable dtSelect = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_PROPERTY");
                            result = GlobalUtil.GenBootGridSystem(dtSelect, current, rowCount);
                        }
                        catch (System.Threading.ThreadAbortException)
                        {
                            //do nothing
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(ex.Message + ex.StackTrace);
                            throw new Exception(GlobalUtil.bootErrorStr);
                        }
                    }
                    else if (action == "savenewgroup")  //保存新的group 
                    {
                        Commom.Log.WriteLog("-- start -- save new group");
                        string saveinfo = Request.Form["saveinfo"];
                        string[] arraySaveInfo = saveinfo.Split('|');
                        
                        string groupname = arraySaveInfo[0];
                        string status = arraySaveInfo[1];
                        string authId = arraySaveInfo[2];
                        string propertyCode = arraySaveInfo[3];
                        string dept = arraySaveInfo[4];
                        string approver = arraySaveInfo[5] == "true" ? "1" : "0";

                        //Step1:新增User Group
                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_GROUPNAME", groupname);
                        oh.AddParameter("P_STATUS", status);
                        oh.AddParameter("P_CREATEBY", Session["loginname"].ToString());
                        /// -- start -- #19493 新增字段
                        oh.AddParameter("P_DEPT", dept);
                        oh.AddParameter("P_APPROVER", approver);
                        /// --  end  --
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue, 10);
                        string groupId = oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER_GROUP").ToString();
                        Commom.Log.WriteLog("   新增User Group... done!");
                        //Step2:新增User Group Authority
                        string[] arrayAuthId = authId.Split(',');
                        foreach (string subAuthId in arrayAuthId)
                        {
                            if (subAuthId != "")
                            {
                                oh.ClearParameter();
                                oh.AddParameter("P_GROUPID", groupId);
                                oh.AddParameter("P_AUTHTYPEID", subAuthId.Trim());
                                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                                oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER_GROUP_AUTHORITY").ToString();
                            }

                            Commom.Log.WriteLog("   新增User Group Authority ... with " + subAuthId.ToString());
                        }

                        //Step3:新增User Group Property
                        string[] arrayPropertyCode = propertyCode.Split(',');
                        foreach (string subPropertyCode in arrayPropertyCode)
                        {
                            if (subPropertyCode != "")
                            {
                                oh.ClearParameter();
                                oh.AddParameter("P_GROUPID", groupId);
                                oh.AddParameter("P_PROPERTYCODE", subPropertyCode.Trim());
                                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                                oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER_GROUP_PROPERTY").ToString();
                            }

                            Commom.Log.WriteLog("   新增User Group Property ... with " + subPropertyCode.ToString());
                        }
                        result = "ok";

                        Commom.Log.WriteLog("--  end  -- save new group");
                    }
                    else if (action == "saveeditgroup")   //保存编辑过的group
                    {
                        Commom.Log.WriteLog("-- start -- save edit group");
                        string saveinfo = Request.Form["saveinfo"];
                        string[] arraySaveInfo = saveinfo.Split('|');

                        string groupid = arraySaveInfo[0];
                        string groupname = arraySaveInfo[1];
                        string status = arraySaveInfo[2];
                        string authId = arraySaveInfo[3];
                        string propertyCode = arraySaveInfo[4];
                        /// -- start -- #19493 新增字段
                        string dept = arraySaveInfo[5];
                        string approver = arraySaveInfo[6] == "true" ? "1" : "0";
                        /// --  end  --

                        //Step1:更新User Group
                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_GROUPNAME", groupname);
                        oh.AddParameter("P_STATUS", status);
                        oh.AddParameter("P_UPDATEBY", Session["loginname"].ToString());

                        /// -- start -- #19493 新增字段
                        oh.AddParameter("P_DEPT", dept);
                        oh.AddParameter("P_APPROVER", approver);
                        /// --  end  --

                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                        oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.UPDATE_USER_GROUP").ToString();

                        Commom.Log.WriteLog("   更新User Group... done");
                        //Step2:更新User Group Authority
                        oh.ClearParameter();
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                        oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.DEL_USER_GROUP_AUTHORITY").ToString();
                        Commom.Log.WriteLog("   更新User Group Authority... done");

                        string[] arrayAuthId = authId.Split(',');
                        foreach (string subAuthId in arrayAuthId)
                        {
                            if (subAuthId != "")
                            {
                                oh.ClearParameter();
                                oh.AddParameter("P_GROUPID", groupid);
                                oh.AddParameter("P_AUTHTYPEID", subAuthId.Trim());
                                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                                oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER_GROUP_AUTHORITY").ToString();
                            }

                            Commom.Log.WriteLog("   更新arrayAuthId... with " + subAuthId);
                        }

                        //Step3:更新User Group Property
                        oh.ClearParameter();
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                        oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.DEL_USER_GROUP_PROPERTY").ToString();
                        Commom.Log.WriteLog("   更新User Group Property... done");

                        string[] arrayPropertyCode = propertyCode.Split(',');
                        foreach (string subPropertyCode in arrayPropertyCode)
                        {
                            if (subPropertyCode != "")
                            {
                                oh.ClearParameter();
                                oh.AddParameter("P_GROUPID", groupid);
                                oh.AddParameter("P_PROPERTYCODE", subPropertyCode.Trim());
                                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Int32, ParameterDirection.ReturnValue);
                                oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_USER_GROUP_PROPERTY").ToString();
                            }

                            Commom.Log.WriteLog("   更新subPropertyCode... with " + subPropertyCode);
                        }

                        result = "ok";
                        Commom.Log.WriteLog("--  end  --");
                    }
                    else if (action == "searchonegroup")  //编辑一条group
                    {
                        string groupid = Request.Form["groupid"];

                        DataModel.ModelUserGroupInfo ModelUserGroupInfo = new DataModel.ModelUserGroupInfo();
                        ModelUserGroupInfo.groupid = groupid;

                        //Step1:查询该groupid对应的group name和status
                        Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtName = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_USER_GROUP_BY_GROUPID");

                        ModelUserGroupInfo.groupname = dtName.Rows[0]["GROUPNAME"].ToString();
                        ModelUserGroupInfo.status = dtName.Rows[0]["STATUS"].ToString();
                        ModelUserGroupInfo.dept = dtName.Rows[0]["DEPT"].ToString();
                        ModelUserGroupInfo.approver = dtName.Rows[0]["APPROVER"].ToString();
                        //Step2:查询该groupid对应的所有authid
                        oh.ClearParameter();
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtAuth = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTHORITY_BY_GROUPID");

                        List<DataModel.ModelUserGroupInfo_AuthInfo> listAuthInfo = new List<DataModel.ModelUserGroupInfo_AuthInfo>();
                        foreach (DataRow dr in dtAuth.Rows)
                        {
                            DataModel.ModelUserGroupInfo_AuthInfo ModelUserGroupInfo_AuthInfo = new DataModel.ModelUserGroupInfo_AuthInfo();
                            ModelUserGroupInfo_AuthInfo.authid = Int32.Parse(dr["AUTHTYPEID"].ToString());
                            listAuthInfo.Add(ModelUserGroupInfo_AuthInfo);
                        }
                        ModelUserGroupInfo.authinfo = listAuthInfo;

                        //Step3:查询该groupid对应的所有property code
                        oh.ClearParameter();
                        oh.AddParameter("P_GROUPID", groupid);
                        oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtProperty = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_PROPERTY_BY_GROUPID");

                        List<DataModel.ModelUserGroupInfo_PropertyInfo> listPropertyInfo = new List<DataModel.ModelUserGroupInfo_PropertyInfo>();
                        foreach (DataRow dr in dtProperty.Rows)
                        {
                            DataModel.ModelUserGroupInfo_PropertyInfo ModelUserGroupInfo_PropertyInfo = new DataModel.ModelUserGroupInfo_PropertyInfo();
                            ModelUserGroupInfo_PropertyInfo.propertycode = dr["PROPERTYCODE"].ToString();
                            listPropertyInfo.Add(ModelUserGroupInfo_PropertyInfo);
                        }
                        ModelUserGroupInfo.propertyinfo = listPropertyInfo;

                        //Step4:生成json
                        result = JsonConvert.SerializeObject(ModelUserGroupInfo);
                    }
                    else if (action == "checkgroup")
                    {
                        JsonRsp rsp = new JsonRsp();                     
                        Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                        try
                        {
                            Helper.DbConsole bz = new Helper.DbConsole(tx);
                            int groupid = Aid.Null2Int(Request.Form["groupid"]);
                            List<Model.Sys_Login_System_Group> etys = bz.Retrieve<Model.Sys_Login_System_Group>(x => x.GroupId == groupid); //tested

                            if (etys.Count > 0)
                            {
                                rsp.err_code = -1;
                                rsp.err_msg = "Group can not be delete for user exist in it !";
                            }
                            else
                            {
                                rsp.err_code = 0;
                                rsp.result = "ok";
                            }
                            tx.CommitTrans();                            
                        }
                        catch (Exception err)
                        {
                            tx.AbortTrans();                            
                            rsp.err_code = -1;
                            rsp.err_msg = err.Message;
                        }
                        result = JsonConvert.SerializeObject(rsp);
                    }
                    else if (action == "delgroup")
                    {
                        JsonRsp rsp = new JsonRsp();
                        Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);                        
                        try
                        {
                            Helper.DbConsole bz = new Helper.DbConsole(tx);                           
                            int groupid = Helper.Aid.Null2Int(Request.Form["groupid"]);
                            List<Model.Sys_Users_Group> etys = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == groupid); //tested
                            if (etys.Count > 0)
                            {
                                int cnt = bz.Delete(etys[0]);
                                if (cnt > 0)
                                {
                                    rsp.result = "ok";
                                }
                                else
                                {
                                    rsp.err_code = -1;
                                    rsp.err_msg = "del failure!";
                                }
                            }
                            else
                            {
                                rsp.err_code = -1;
                                rsp.err_msg = "group not found!";
                            }                         
                            tx.CommitTrans();
                        }
                        catch (Exception err)
                        {                            
                            tx.AbortTrans();
                            rsp.err_code = -1;
                            rsp.err_msg = err.Message;
                        }
                        result = JsonConvert.SerializeObject(rsp);
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