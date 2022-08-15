using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
namespace PropertyOneAppWeb.system
{
    public partial class eFormDateDef : PageBase
    {
        Sys_Users_Group grpInfo = null;

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

                    case "save":
                        result = this.Save();
                        break;

                    case "approve":
                        result = this.Approve();
                        break;

                    case "del":
                        result = this.Del();
                        break;

                    default:
                        break;
                }
                Response.Write(result);
                Response.End();
            }
        }
       
        private string Del()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                grpInfo = Session["GroupInfo"] as Model.Sys_Users_Group;
                DbConsole bz = new DbConsole(tx);
                List<EDateDefineModel> etys = bz.Retrieve<EDateDefineModel>(x => x.dtdf_id == Aid.Null2Int(Request.Form["id"]));
                if (etys.Count > 0)
                {
                    if (etys[0].dtdf_status.IndexOf("A") >= 0 && grpInfo.Approver != "1")
                        throw new Exception("Can't delete ,for it was approved.");

                    etys[0].dtdf_active = "I";
                    etys[0].dtdf_status = "I";
                    etys[0].dtdf_updateby = Session["loginname"].ToString();
                    etys[0].dtdf_updatedate = DateTime.Now;
                    etys[0].dtdf_approveby = "";
                    etys[0].dtdf_approvedate = null;
                    bz.Update(etys[0]);
                    rsp.result = true;
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Approve()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Sys_Users_Group grp = Session["groupinfo"] as Sys_Users_Group;
                DbConsole bz = new DbConsole(tx);
                List<EDateDefineModel> etys = bz.Retrieve<EDateDefineModel>(x => x.dtdf_id == Aid.Null2Int(Request.Form["id"]));
                if (etys.Count > 0)
                {
                    if (etys[0].dtdf_status.IndexOf("I") >= 0)
                    {
                        etys[0].dtdf_status = "A";
                        etys[0].dtdf_approveby = Session["loginname"].ToString();
                        etys[0].dtdf_approvedate = DateTime.Now;
                        bz.Update(etys[0]);
                        rsp.result = true;
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "invalid status.";
                    }
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "date not found.";
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Save()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                   
                int c = 0;
                int act = Aid.Null2Int(Request.Form["action"]);
                DbConsole bz = new DbConsole(tx);            
                EDateDefineModel ety = null;
                if (act == 0)
                {
                    List<EDateDefineModel> etys = bz.Retrieve<EDateDefineModel>(x => x.dtdf_date == DateTime.Parse(Request.Form["dtdf_date"]));
                    if (etys.Count > 0)
                        throw new Exception("date already exists.");
                    
                    ety = new EDateDefineModel();
                    ety.dtdf_date = DateTime.Parse(Request.Form["dtdf_date"]);
                    ety.dtdf_define = Aid.Null2Str(Request.Form["dtdf_define"]);
                    ety.dtdf_active = "A";
                    ety.dtdf_status = "I";
                    ety.dtdf_createby = Session["loginname"].ToString();
                    ety.dtdf_createdate = DateTime.Now;
                    ety.dtdf_updateby = Session["loginname"].ToString();
                    ety.dtdf_updatedate = ety.dtdf_createdate;
                    c = bz.Create(ety);
                }
                else
                {
                    ety = bz.Retrieve<EDateDefineModel>(x => x.dtdf_id == Aid.Null2Int(Request.Form["dtdf_id"]))[0];
                    Sys_Users_Group grp = Session["groupinfo"] as Sys_Users_Group;
                    if (grp.Approver == "1" || ety.dtdf_status.IndexOf("A") < 0) //审批者或单据未审批可以编辑
                    {
                        ety.dtdf_date = DateTime.Parse(Request.Form["dtdf_date"]);
                        ety.dtdf_define = Aid.Null2Str(Request.Form["dtdf_define"]);
                        ety.dtdf_active = "A";
                        ety.dtdf_status = "I";
                        ety.dtdf_updateby = Session["loginname"].ToString();
                        ety.dtdf_updatedate = DateTime.Now;
                        c = bz.Update(ety);
                    }
                    else
                    {
                        throw new Exception("Can't save, for it was approved.");
                    }
                }

                tx.CommitTrans();
                rsp.err_code = 0;
                rsp.result = true;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Search(string sp, int pidx, int size)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                DbConsole bz = new DbConsole(tx);

                int ymd = 0;
                var pre = PredicateBuilder.True<EDateDefineModel>();                
                if (sp.ToUpper().Trim() == "A" || sp.ToUpper().Trim() == "I")
                {
                    pre = pre.And(x => x.dtdf_status.Sql_Like(sp.ToUpper()) || x.dtdf_active.Sql_Like(sp.ToUpper()));
                }
                else
                {                    
                    if (!int.TryParse(sp, out ymd))
                    {
                        pre = pre.And(x => x.dtdf_define.Sql_Like(sp.ToUpper()));   
                    }                    
                }
                List<EDateDefineModel> etys = bz.Retrieve<EDateDefineModel>(pre," order by dtdf_date");
                if (int.TryParse(sp, out ymd))                
                    etys = etys.FindAll(x => x.char_dtdf_date.Contains(sp)).ToList();
                rsp.result = this.SetPager(etys, pidx, size);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }
    }

    public class EDateDefineModel
    {
        public const string TABLE_NAME = "e_DateDefine";

        [DbField(true, true)]
        public int dtdf_id { get; set; }

        public DateTime dtdf_date { get; set; }
        public string dtdf_define { get; set; }
        public string dtdf_active { get; set; }
        public string dtdf_status { get; set; }
        public string dtdf_createby { get; set; }
        public DateTime dtdf_createdate { get; set; }
        public string dtdf_updateby { get; set; }
        public DateTime dtdf_updatedate { get; set; }
        public string dtdf_approveby { get; set; }
        public Nullable<DateTime> dtdf_approvedate { get; set; }

        [DbField(false,false,true)]
        public string char_dtdf_date { get { return Aid.Date2Str(dtdf_date, Aid.DateFmt.Standard); } }
    }
}