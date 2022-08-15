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

namespace PropertyOneAppWeb.system
{
    public partial class exceptionlist : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }
            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action))
            {
                string result = "";
                try
                {
                    if (action == "enquiryexceptionlist")   //查找Exception List
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);
                        int id = Int32.Parse(Session["statementdateid"].ToString());

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_ID", id);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.Output);
                        DataTable dt = (DataTable)oa.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_SELECT_EXCEPTION");
                        result = GlobalUtil.GenBootGridSystem(dt, current, rowCount);
                    }
                    else if (action == "newexceptionlist")  //新增Exception List
                    {
                        int id = Int32.Parse(Session["statementdateid"].ToString());
                        string lease = Request.Form["lease"].ToUpper();
                        string createBy = Session["loginname"].ToString();

                        if (string.IsNullOrEmpty(lease))
                        {
                            throw new Exception("Lease number can not be null");
                        }

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_ID", id);
                        oa.AddParameter("P_LEASE", lease);
                        oa.AddParameter("P_CREATEBY", createBy);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                        result = oa.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_ADD_EXCEPTION").ToString();
                    }
                    else if (action == "delexceptionlist")  //删除Exception List
                    {
                        int id = Int32.Parse(Session["statementdateid"].ToString());
                        string lease = Request.Form["lease"];

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_ID", id);
                        oa.AddParameter("P_LEASE", lease);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                        result = oa.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_DEL_EXCEPTION").ToString();
                    }
                    else if (action == "delselectedlease")  //删除选中的多个Exception List
                    {
                        int id = Int32.Parse(Session["statementdateid"].ToString());
                        string lease = Request.Form["lease[]"];

                        if (string.IsNullOrEmpty(lease))
                        {
                            throw new Exception("Please select one or more lease to delete");
                        }

                        string[] arrayLease = lease.Split(',');
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        foreach (string s in arrayLease)
                        {
                            oa.ClearParameter();
                            oa.AddParameter("P_ID", id);
                            oa.AddParameter("P_LEASE", s);
                            oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.Output, 100);
                            result = oa.ExecuteStoredProcedure("PKG_STATEMENT_DATE.PROC_DEL_EXCEPTION").ToString();
                            if (result != "ok")
                            {
                                throw new Exception("Delete lease error: " + s);
                            }
                        }
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