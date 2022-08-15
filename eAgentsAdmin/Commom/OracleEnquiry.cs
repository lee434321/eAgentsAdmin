using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using PropertyOneAppWeb.Model;

namespace PropertyOneAppWeb.Commom
{
    public class OracleEnquiry
    {
        public OracleEnquiry() { }
        
        /// <summary>
        /// 校验用户是否有权限访问
        /// </summary>
        /// <returns></returns>
        public bool CheckUserAuth(string loginName, string authId)
        {
            try
            {
                //OracleHelper oh = new OracleHelper(GlobalUtilSystem.sdb_local);                
                Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                oh.AddParameter("P_LOGINNAME", loginName);                
                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtAuthList = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTH");

                foreach (DataRow dr in dtAuthList.Rows)
                {
                    string dtAuthId = dr["AUTHTYPEID"].ToString();
                    if (authId == dtAuthId)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户权限列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetAuthList(string loginName)
        {
            try 
            {                
                Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                oh.AddParameter("P_LOGINNAME", loginName);                
                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtAuthList = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTH");
                return dtAuthList;
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户权限列表，并返回json格式
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns></returns>
        public string GetAuthListJson(string loginName)
        {
            try
            {
                Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                oh.AddParameter("P_LOGINNAME", loginName);              
                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtAuthList = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTH");

                DataModel.ModelUserAuthList modelUserAuthList = new DataModel.ModelUserAuthList();
                modelUserAuthList.loginName = loginName;
                List<DataModel.ModelUserAuthList_AuthInfo> list = new List<DataModel.ModelUserAuthList_AuthInfo>();
                foreach (DataRow dr in dtAuthList.Rows) {
                    DataModel.ModelUserAuthList_AuthInfo authInfo = new DataModel.ModelUserAuthList_AuthInfo();
                    authInfo.authId = dr["AUTHTYPEID"].ToString();
                    list.Add(authInfo);
                }
                modelUserAuthList.authInfo = list;

                string json = JsonConvert.SerializeObject(modelUserAuthList);
                return json;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户Property权限列表
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public DataTable GetPropertyList(string loginName)
        {
            try
            {
                Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                oh.AddParameter("P_LOGINNAME", loginName);
                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtPropertyList = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTH_PRO");

                return dtPropertyList;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户Property权限列表(由逗号分隔的字符串)
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public string GetPropertyListStr(string loginName)
        {
            try
            {                
                Oracle9i oh = new Oracle9i(GlobalUtilSystem.sdb_local);
                oh.AddParameter("P_LOGINNAME", loginName);
                oh.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);                
                DataTable dtPropertyList = (DataTable)oh.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_AUTH_PRO");
                
                string str = "";
                foreach (DataRow dr in dtPropertyList.Rows)
                {
                    str += dr["PROPERTYCODE"].ToString().Trim() + ",";
                }
                return str;
            }
            catch
            {
                throw;
            }
        }
    }
}