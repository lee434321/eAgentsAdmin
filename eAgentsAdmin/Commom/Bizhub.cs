using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using PropertyOneAppWeb.Helper;
namespace PropertyOneAppWeb.Commom
{
    public class RspLite
    {
        public int err_code { get; set; }
        public string result_msg { get; set; }        
    }

    public class CONST
    {
        /// Customer User 状态枚举说明    
        public const string CU_STATUS_PENDING = "0";    // 待发送邮件
        public const string CU_STATUS_SENTMAIL = "1";   // 已经发送邮件
        public const string CU_STATUS_ACTIVE = "2";     // 已激活（不能删除）
        public const string CU_STATUS_INACTIVE = "3";   // 禁用(暂未使用)

        public const string CUSTOMER_USER_MKT_SUFFIX = "M";
        public const string CUSTOMER_USER_FIN_SUFFIX = "F";
    }

    public class Bizhub
    {
        public static string Path { get; set; }        
      
        public static List<Model.Sys_Users_Group_Property> GetProperties(int groupid)
        {           
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {                              
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.Sys_Users_Group_Property> etys = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == groupid);
                tx.CommitTrans();
                return etys;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                return new List<Model.Sys_Users_Group_Property>();                
            }
        }       

        public static DataTable FilterPropertyByGroup(int groupid,DataTable src,string leftFieldName,string rightFieldName)
        {            
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.Sys_Users_Group_Property> etys = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == groupid);
                tx.CommitTrans();

                var query = from p in src.AsEnumerable()
                            from es in etys.AsEnumerable()
                            where p.Field<string>(leftFieldName) == es.GetType().GetProperty(rightFieldName).GetValue(es, null).ToString()
                            select p;

                DataTable dt2 = src.Clone();
                for (int i = 0; i < query.Count<DataRow>(); i++)
                {
                    dt2.ImportRow(query.ElementAt<DataRow>(i));
                }
                return dt2;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                return new DataTable();
            }                      
        }               
              
        public static bool CheckProfile4CustCreation(string loginName, string tmpgrp,Helper.DbTx tx, string leasenum = "")
        {
            //1.当前登录用户所属是fin，mkt，还是em
            //2.取profile中可创建的用户数量
            //3.在已有的customer_user表检查该用户已经创建的customer数量是否符合限制。            
            try
            {
                Helper.DbConsole biz = new Helper.DbConsole(tx);

                string sql = "select a.*,b.leasenumber";
                sql += " from sys_login_account a";
                sql += " left join sys_users_group_lease b";
                sql += " on a.userid = b.userid";
                sql += " and b.create_src = '0'";
                sql += " where 1 = 1";
                sql += " and a.role='";
                sql += tmpgrp == "EM_MAX" ? "E" : tmpgrp == "FIN_MAX" ? "F" : tmpgrp == "MKT_MAX" ? "M" : "";
                sql += "'";
                sql += " and a.status='A'";
                //sql += " and substr(b.leasenumber,1,4)='" + leasenum.Substring(0, 4) + "'";
                sql += " and trim(b.leasenumber)='" + leasenum + "'";

                bool result = true;
                if (tmpgrp != "")
                {
                    List<Model.Profile> etys = biz.Retrieve<Model.Profile>();
                    var r1 = from x in etys
                             where x.Key == tmpgrp
                             select x;
                    int cnt = int.Parse(r1.ElementAt(0).Val);

                    if (cnt > 0)
                    {
                        //biz.Clauses.Add("CreateBy", new Helper.Evaluation(Helper.SqlOperator.EQ, loginName));
                        //biz.Clauses.Add("Status", new Helper.Evaluation(Helper.SqlOperator.EQ, "A"));
                        //biz.Clauses.Add("Role", new Helper.Evaluation(Helper.SqlOperator.EQ, tmpgrp == "EM_MAX" ? "E" : tmpgrp == "FIN_MAX" ? "F" : tmpgrp == "MKT_MAX" ? "M" : ""));
                        List<Model.Sys_Login_Account> etys2 = biz.Retrieve<Model.Sys_Login_Account>(sql);

                        if (etys2.Count < cnt)
                            result = true;
                        else
                            result = false;
                    }
                }
               
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }       
    }
}