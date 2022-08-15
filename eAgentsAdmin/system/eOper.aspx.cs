using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Newtonsoft.Json;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using PropertyOneAppWeb.Helper;
namespace PropertyOneAppWeb.system
{
    public partial class eOper : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Helper.Aid.DbNull2Str(Request.QueryString["act"]);
            if (Session["loginname"] == null)
            {
                if (action == "")
                {
                    Response.Redirect("../LoginSystem.aspx");
                    Response.End();
                }
                else
                {
                    action = "timeout";
                }
            }

            if (!string.IsNullOrEmpty(action))
            {
                JsonRsp rsp = new JsonRsp();
                string result = "";
                switch (action)
                {
                    case "search":
                        string sp = Request.Form["searchParse"].ToString();
                        int pidx = int.Parse(Request.Form["pageIdx"].ToString());
                        int size = int.Parse(Request.Form["pageSize"]);
                        result = this.Search(sp, pidx, size);
                        break;

                    case "timeout":
                        rsp.err_code = -98;
                        rsp.err_msg = "timeout";
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
                        break;
                    default:
                        result = "action not found for'" + action + "'";
                        break;
                }
                Response.Write(result);
                Response.End();
            }
        }

        private string Search(string sp, int pidx, int size)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                DbConsole bz = new DbConsole(tx);                              
                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", Helper.Aid.Null2Str(Session["loginname"])));
                Model.Sys_Users_Group gr = Session["groupinfo"] as Model.Sys_Users_Group;
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", Helper.Aid.Null2Str(gr.GroupName)));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Search", sp));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue));                
                List<EOperationModelQuery> etysx = bz.Retrieve9iSp<EOperationModelQuery>("pkg_eform_flow.Get_Process_EForm", paras.ToArray());                
                /// --  end  --
                rsp.result = SetPager(etysx, pidx, size);               
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }
            return JsonConvert.SerializeObject(rsp);
        }
    }

    public class EOperationModelQuery
    {
        public const string TABLE_NAME = "E_Operation";

        [DbField(true, true)]
        public int Oper_Id { get; set; }
        public int Form_Id { get; set; }
        public string Form_Name { get; set; }
        public string Oper_Status { get; set; }
        public string Oper_CreateBy { get; set; }
        public string Oper_CreateDate { get; set; }
        public string Oper_RefNo { get; set; }
        public int Fwde_Seq { get; set; }
        public int Oper_Curseq { get; set; }
        public string Operate { get; set; }
        public string Flow_CurGroup { get; set; }
        public string Form_Template { get; set; } //这里是模板的webform文件名，如“FormA.html”;注意，是html
        public string Tenant_Name { get; set; }
    }

    public class EOperationModel
    {
        public const string TABLE_NAME = "E_Operation";

        [DbField(true)]
        public int Oper_Id { get; set; }
        public int Form_Id { get; set; } //402
        public int Flow_Id { get; set; }
        public int Fmtb_Id { get; set; }
        public string Oper_Property { get; set; }
        public int Oper_Curseq { get; set; }
        public string Oper_Status { get; set; }
        public string Oper_CreateBy { get; set; }
        public Nullable<DateTime> Oper_CreateDate { get; set; }
        public string Oper_Refno { get; set; }
        public string Oper_Lease { get; set; }
        public string Pdf_Url { get; set; }
        /*
         *  OPER_ID	281	
            FORM_ID	402	
            FLOW_ID	133	
            FMTB_ID	130000	
            OPER_PROPERTY	WG02	
            OPER_CURSEQ	10	
            OPER_STATUS	P         	
            OPER_CREATEBY	WG0200388M09	
            OPER_CREATEDATE	2021/5/31 15:35:18	
            OPER_REFNO	Form_B_281	
            OPER_LEASE	WG0200388	

         */
    }
}
