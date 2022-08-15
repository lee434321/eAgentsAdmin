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
    public partial class PropertyMgmt : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }
            string result = "";
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (!oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "20"))
                Response.Redirect("../LoginSystem.aspx");

            string action = Request.Form["action"];
            if (!string.IsNullOrEmpty(action)) 
            {
                if (action == "search")
                {
                    result = this.Search();
                }
                else if (action == "edit")
                {
                    result = this.Editor();
                }
                else if (action == "save") 
                {
                    result = this.Save();
                }
                Log.WriteLog("PorpertyMgmt Response:" + result);
                Response.Write(result);
                Response.End();
            }
        }

        private string Save()
        {
            Commom.RspLite rsp = new RspLite();
            string propertyCode = Request.Form["propertycode"].ToString();
            string emphone = Request.Form["emphone"].ToString();
            string ememail = Request.Form["ememail"].ToString();
            string lephone = Request.Form["lephone"].ToString();
            string leemail = Request.Form["leemail"].ToString();
            string blphone = Request.Form["blphone"].ToString();
            string blemail = Request.Form["blemail"].ToString();
          
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.pone_pd_property> etys = bz.Retrieve<Model.pone_pd_property>(x => x.Property_Code == propertyCode); //tested
                if (etys.Count > 0)
                {
                    etys[0].EM_Phone = emphone;
                    etys[0].EM_Email = ememail;
                    etys[0].LE_Phone = lephone;
                    etys[0].LE_Email = leemail;
                    etys[0].BL_Phone = blphone;
                    etys[0].BL_Email = blemail;

                    int cnt = bz.Update(etys[0]);
                    if (cnt > 0)                    
                        rsp.err_code = 0;                    
                    else
                    {
                        rsp.err_code = -1;
                        rsp.result_msg = "update failure!";
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -1;
                rsp.result_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Editor()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                string propertyCode = Request.Form["propertycode"].ToString();
                Helper.DbConsole dc = new Helper.DbConsole(tx);
                List<Model.pone_pd_property> etys = dc.Retrieve<Model.pone_pd_property>(x => x.Property_Code == propertyCode);//tested
                tx.CommitTrans();

                if (etys.Count > 0)
                {
                    rsp.result = etys;
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "找不到指定property_code为[" + propertyCode.Trim() + "]的数据";
                }                
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp = new JsonRsp(-1, err.Message);                
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Search()
        {
            string result = "";
            try
            {
                int current = Int32.Parse(Request.Form["current"]);
                int rowCount = Int32.Parse(Request.Form["rowCount"]);

                Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_PROPERTY");

                /// -- start -- 这里过滤当期登录用户可以查看的properties
                Model.Sys_Users_Group g = Session["groupinfo"] as Model.Sys_Users_Group;
                List<Model.Sys_Users_Group_Property> etys = Bizhub.GetProperties(g.GroupId);
                var r = from x in dtSelect.AsEnumerable()
                        from y in etys
                        where x.Field<string>("Property_Code").Trim() == y.PropertyCode.Trim()
                        select x;

                DataTable dt2 = dtSelect.Clone();
                for (int i = 0; i < r.Count<DataRow>(); i++)
                {
                    dt2.ImportRow(r.ElementAt<DataRow>(i));
                }
                /// --  end  --

                /// -- start -- 过滤检索                                                  
                string searchPhrase = Request.Form["searchPhrase"];
                var r2 = from x in dt2.AsEnumerable()
                         where (x.Field<string>("Property_Code").Trim().ToUpper().Contains(searchPhrase.ToUpper()) ||
                            x.Field<string>("Property_Name").Trim().ToUpper().Contains(searchPhrase.ToUpper()))
                         select x;
                
                DataTable dt3 = dt2.Clone();
                for (int i = 0; i < r2.Count<DataRow>(); i++)
                {
                    dt3.ImportRow(r2.ElementAt<DataRow>(i));
                }
                /// --  end  --               
                result = GlobalUtil.GenBootGridSystem(dt3, current, rowCount);
            }
            catch (Exception ex)
            {
                result = GlobalUtil.bootErrorStr;
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            return result;
        }
    }
}