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
    public partial class eFormTimeDef : PageBase
    {
        Model.Sys_Users_Group grpInfo = null;
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
                grpInfo = Session["groupinfo"] as Sys_Users_Group;
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
                DbConsole bz = new DbConsole(tx);
                List<ETimeDefineModel> etys = bz.Retrieve<ETimeDefineModel>(x => x.tmdf_id == Aid.Null2Int(Request.Form["tmdf_id"]));              
                if (etys.Count > 0)
                {
                    if (bz.Retrieve<EFormModel>(x => x.TMDF_ID == etys[0].tmdf_id && x.TMDF_CODE == etys[0].tmdf_code).Count > 0)
                        throw new Exception("Can't delete, for it is in use.");

                    if (etys[0].tmdf_status.IndexOf("A") >= 0 && grpInfo.Approver != "1") //如果是审批状态并且不是审批者，则不可以删除
                        throw new Exception("Can't delete ,for it was approved.");

                    etys[0].tmdf_active = "I";
                    etys[0].tmdf_status = "I";
                    etys[0].tmdf_updateby = Session["loginname"].ToString();
                    etys[0].tmdf_updatedate = DateTime.Now;
                    etys[0].tmdf_approveby = "";
                    etys[0].tmdf_approvedate = null;

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
                DbConsole bz = new DbConsole(tx);
                List<ETimeDefineModel> etys = bz.Retrieve<ETimeDefineModel>(x => x.tmdf_id == Aid.Null2Int(Request.Form["id"]));
                if (etys.Count > 0)
                {
                    if (etys[0].tmdf_status.IndexOf("I") >= 0)
                    {
                        etys[0].tmdf_status = "A";
                        etys[0].tmdf_approveby = Session["loginname"].ToString();
                        etys[0].tmdf_approvedate = DateTime.Now;
                        bz.Update(etys[0]);
                        rsp.result = true;
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "Invalid status or it was already approved.";
                    }
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "time not found.";
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
                grpInfo = Session["GroupInfo"] as Model.Sys_Users_Group;
                int c = 0;
                int act = Aid.Null2Int(Request.Form["action"]);
                string json = Request.Form["dtl"];
                List<ETimePeriodModel> dtl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ETimePeriodModel>>(json);
                ETimeDefineModel etyH = null;
                int hid = 0;
                DbConsole bz = new DbConsole(tx);
                if (act==0)
                {
                    etyH = new ETimeDefineModel();
                    etyH.tmdf_active = "A";
                    etyH.tmdf_code = Request.Form["tmdf_code"];
                    etyH.tmdf_desc = Request.Form["tmdf_desc"];
                    etyH.tmdf_name = Request.Form["tmdf_name"];
                    etyH.tmdf_status = "I";
                    etyH.tmdf_createby = Session["loginname"].ToString();
                    etyH.tmdf_createdate = DateTime.Now;
                    etyH.tmdf_updateby = Session["loginname"].ToString();
                    etyH.tmdf_updatedate = etyH.tmdf_createdate;
                    c = bz.Create(etyH);
                    hid = Aid.Null2Int(tx.ExecuteScalar("select SEQ_ETMDF_ID.Currval from dual"));//拿到自增ID值                    
                }
                else
                {                   
                    hid = Aid.Null2Int(Request.Form["tmdf_id"]);
                    etyH = bz.Retrieve<ETimeDefineModel>(x => x.tmdf_id == hid)[0];

                    if (bz.Retrieve<EFormModel>(x => x.TMDF_ID == hid && x.TMDF_CODE == etyH.tmdf_code).Count > 0)
                        throw new Exception("Can't save, for it is in use.");

                    //审批者或单据未审批可以编辑( 审批者 or （status!=A and active==A）)
                    if (grpInfo.Approver == "1" || (etyH.tmdf_status.IndexOf("A") < 0 && etyH.tmdf_active.IndexOf("A") >= 0)) 
                    {
                        etyH.tmdf_active = "A";
                        etyH.tmdf_code = Request.Form["tmdf_code"];
                        etyH.tmdf_desc = Request.Form["tmdf_desc"];
                        etyH.tmdf_name = Request.Form["tmdf_name"];
                        etyH.tmdf_status = "I";
                        etyH.tmdf_updateby = Session["loginname"].ToString();
                        etyH.tmdf_updatedate = etyH.tmdf_createdate;
                        c = bz.Update(etyH);

                        List<ETimePeriodModel> etysD = bz.Retrieve<ETimePeriodModel>(x => x.tmdf_id == hid);
                        bz.Delete(etysD);
                    }
                    else
                    {
                        throw new Exception("Can't save, for it was approved or was a invalid status.");
                    }
                    
                }

                if (dtl.Count>0)
                {
                    //处理子表
                    for (int j = 0; j < dtl.Count; j++)
                    {
                        ETimePeriodModel etyD = new ETimePeriodModel();
                        etyD.tmdf_id = hid;
                        etyD.tmper_code = dtl[j].tmper_code;
                        etyD.tmper_seq = j + 1;
                        etyD.tmper_mon = dtl[j].tmper_mon;
                        etyD.tmper_tue = dtl[j].tmper_tue;
                        etyD.tmper_wed = dtl[j].tmper_wed;
                        etyD.tmper_thu = dtl[j].tmper_thu;
                        etyD.tmper_fri = dtl[j].tmper_fri;
                        etyD.tmper_sat = dtl[j].tmper_sat;
                        etyD.tmper_sun = dtl[j].tmper_sun;
                        etyD.tmper_hld = dtl[j].tmper_hld;
                        etyD.tmper_from = dtl[j].tmper_from;
                        etyD.tmper_to = dtl[j].tmper_to;
                        etyD.tmper_active = "A";
                        c = bz.Create(etyD);
                    }
                }
                else
                {
                    throw new Exception("detail is required.");
                }
                
                tx.CommitTrans();
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
                var pre = PredicateBuilder.True<ETimeDefineModel>();                
                //查询条件
                if (sp.Trim().ToUpper()=="I" ||sp.Trim().ToUpper()=="A")                
                    pre = pre.And(x => x.tmdf_active.Sql_Like(sp.Trim().ToUpper()) || x.tmdf_status.Sql_Like(sp.Trim().ToUpper()));                
                else                
                    pre = pre.And(x => x.tmdf_code.Sql_Like(sp.Trim()) || x.tmdf_desc.Sql_Like(sp.Trim()) || x.tmdf_name.Sql_Like(sp.Trim()));

                List<ETimeDefineModel> etys = bz.Retrieve<ETimeDefineModel>(pre, "order by tmdf_code");
                Pager<ETimeDefineModel> pager = this.SetPager(etys, pidx, size);

                //取子表
                for (int i = 0; i < pager.data.Count; i++)
                {
                    List<ETimePeriodModel> dtls = bz.Retrieve<ETimePeriodModel>(x => x.tmdf_id == pager.data[i].tmdf_id);
                    dtls.Sort((x, y) =>
                    {
                        if (x.tmper_seq < y.tmper_seq)
                            return -1;
                        else if (x.tmper_seq > y.tmper_seq)
                            return 1;
                        else
                            return 0;
                    });
                    pager.data[i].detail = dtls;
                }
                rsp.result = pager;
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

    public class ETimeDefineModel
    {
        public const string TABLE_NAME = "e_TimeDefine";

        [DbField(true, true)]
        public int tmdf_id { get; set; }
        public string tmdf_code { get; set; }
        public string tmdf_name { get; set; }
        public string tmdf_desc { get; set; }
        public string tmdf_active { get; set; }
        public string tmdf_status { get; set; }
        public string tmdf_createby { get; set; }
        public DateTime tmdf_createdate { get; set; }
        public string tmdf_updateby { get; set; }
        public DateTime tmdf_updatedate { get; set; }
        public string tmdf_approveby { get; set; }
        public Nullable<DateTime> tmdf_approvedate { get; set; }

        //以下字段不存在于数据库
        [DbField(false,false,true)] 
        public List<ETimePeriodModel> detail = new List<ETimePeriodModel>(); 
    }

    public class ETimePeriodModel
    {
        public const string TABLE_NAME = "e_TimePeriod";

        [DbField(true, true)]
        public int tmper_id { get; set; }

        public int tmdf_id { get; set; } //foreign key
        public string tmper_code { get; set; }
        public int tmper_seq { get; set; }
        public string tmper_mon { get; set; }
        public string tmper_tue { get; set; }
        public string tmper_wed { get; set; }
        public string tmper_thu { get; set; }
        public string tmper_fri { get; set; }
        public string tmper_sat { get; set; }
        public string tmper_sun { get; set; }
        public string tmper_hld { get; set; }
        public string tmper_from { get; set; }
        public string tmper_to { get; set; }
        public string tmper_active { get; set; }
    }
}