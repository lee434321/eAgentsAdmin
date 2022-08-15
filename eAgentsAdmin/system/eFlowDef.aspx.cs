using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
namespace PropertyOneAppWeb.system
{
    public partial class eFlowDef : PageBase
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
                JsonRsp rsp = new JsonRsp();
                string result = "";
                switch (action)
                {
                    case "del":
                        int id = int.Parse(Request.Form["id"].ToString());
                        result = this.Delete(id);
                        break;
                    case "save":
                        result = Save();
                        break;
                    case "search":
                        string sp = Request.Form["searchParse"].ToString();
                        int pidx = int.Parse(Request.Form["pageIdx"].ToString());
                        int size = int.Parse(Request.Form["pageSize"]);
                        result = this.Search(sp, pidx, size);
                        break;                   
                    case "approve":
                        result = this.Approve();
                        break;
                    default:
                        break;
                }
                Response.Write(result);
                Response.End();
            }
        }

        private string Approve()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);               
                List<EFLowModel> etys = bz.Retrieve<EFLowModel>(x => x.Flow_Id == Aid.Null2Int(Request.Form["id"]));
                if (etys.Count > 0)
                {
                    if (etys[0].FLOW_STATUS.IndexOf("A") >= 0)
                        throw new Exception("Can not approve ,for already approved.");
                }  
                tx.CommitTrans();

                Oracle9i o9i = new Oracle9i(GlobalUtilSystem.sdb_local);
                o9i.AddParameter("Pm_FlowId", Helper.Aid.Null2Int(Request.Form["id"]));
                o9i.AddParameter("Pm_LoginName", Session["loginname"]);
                o9i.AddParameter("Pm_GroupName", Session["groupname"]);
                o9i.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Number, ParameterDirection.ReturnValue, 100);
                int retVal = Helper.Aid.Null2Int(o9i.ExecuteStoredProcedure("pkg_eform_flow.Approve_EFlow"));
                if (retVal == 1)
                {
                    rsp.result = true;
                }
                else
                {
                    rsp.result = false;
                }
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);           
        }       

        private string Delete(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<EFLowModel> etys = bz.Retrieve<EFLowModel>(x => x.Flow_Id == id);
                if (etys.Count > 0)
                {
                    if (etys[0].FLOW_STATUS.Trim() == "A")
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "Can not delete , for the flow was approved.";
                    }
                    else
                    {
                        etys[0].FLOW_ACTIVE = "I";
                        bz.Update(etys[0]);
                        rsp.result = true;
                    }
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = string.Format("form id '{0}' not found.", id.ToString());
                    rsp.result = false;
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

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private string Save()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                grpInfo = Session["GroupInfo"] as Model.Sys_Users_Group;
                
                string json = Request.Form["dtl"];
                List<EFlowDetailModel> dtl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EFlowDetailModel>>(json);
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                EFLowModel ety = new EFLowModel();
                int cnt = 0;
                string act = Request.Form["action"];
                int flowId = 0;
                if (act == "new")
                {
                    ety.Flow_Code = Request.Form["FLOW_CODE"];
                    ety.FLOW_NAME = Request.Form["FLOW_NAME"];
                    ety.FLOW_DESC = Request.Form["FLOW_DESC"];
                    ety.FLOW_EFFFROM = DateTime.Parse(Request.Form["FLOW_EFFFROM"].ToString());
                    ety.FLOW_EFFTO = DateTime.Parse(Request.Form["FLOW_EFFTO"].ToString());           
                    ety.FLOW_PROPERTY = Request.Form["FLOW_PROPERTY"];
                    ety.FLOW_ACTIVE = "A";
                    ety.FLOW_STATUS = "I";
                    ety.FLOW_CREATEDATE = DateTime.Now;
                    ety.FLOW_CREATEBY = Session["loginname"].ToString();
                    ety.FLOW_UPDATEDATE = DateTime.Now;
                    ety.FLOW_UPDATEBY = Session["loginname"].ToString();   
                    cnt = bz.Create(ety);
                    flowId = Convert.ToInt32(tx.ExecuteScalar("select seq4Flow_id.Currval from dual")); //拿到自增ID值
                    rsp.result = true;
                }
                else
                {
                    List<EFLowModel> etys = bz.Retrieve<EFLowModel>(x => x.Flow_Id == int.Parse(Request.Form["Flow_Id"]));
                    if (etys.Count>0)
                    {
                        flowId = etys[0].Flow_Id;
                        /// -- start --
                        
                        if (etys[0].FLOW_STATUS.IndexOf("A") >= 0)
                        {
                            if (grpInfo.Approver == "1")
                            {
                                List<EFormFlowModel> etysFfs = bz.Retrieve<EFormFlowModel>(x => x.Flow_Id == etys[0].Flow_Id);
                                if (etysFfs.Count>0)
                                { 
                                    //有限修改
                                    List<EFlowDetailModel> srcDtl = bz.Retrieve<EFlowDetailModel>(x => x.FLOW_ID == flowId);
                                    srcDtl.Sort((x, y) =>
                                    {
                                        if (x.FWDE_SEQ < y.FWDE_SEQ)
                                            return -1;
                                        else if (x.FWDE_SEQ > y.FWDE_SEQ)
                                            return 1;
                                        else
                                            return 0;
                                    });
                                  
                                    if (srcDtl.Count != dtl.Count)
                                    {
                                        throw new Exception("Can not save, counts not match.");
                                    }

                                    for (int i = 0; i < srcDtl.Count; i++)
                                    {
                                        if (srcDtl[i].FWDE_GROUPID != dtl[i].FWDE_GROUPID)
                                            throw new Exception("Can not save, 'Group Id' coundn't modify.");

                                        if (srcDtl[i].FWDE_SEQ != dtl[i].FWDE_SEQ)
                                        {
                                            throw new Exception("Can not save, 'Seq' coundn't modify.");
                                        }
                                    }
                                }                                
                            }
                            else
                            {
                                throw new Exception("Can not save, already approved.");
                            }
                        }                       
                        /// --  end  --                        
                        etys[0].Flow_Code = Request.Form["FLOW_CODE"];
                        etys[0].FLOW_NAME = Request.Form["FLOW_NAME"];
                        etys[0].FLOW_DESC = Request.Form["FLOW_DESC"];
                        etys[0].FLOW_EFFFROM = DateTime.Parse(Request.Form["FLOW_EFFFROM"].ToString());
                        etys[0].FLOW_EFFTO = DateTime.Parse(Request.Form["FLOW_EFFTO"].ToString());                       
                        etys[0].FLOW_PROPERTY = Request.Form["FLOW_PROPERTY"];
                        //etys[0].FLOW_ACTIVE = Request.Form["FLOW_ACTIVE"];
                        etys[0].FLOW_STATUS = "I";
                        etys[0].FLOW_UPDATEDATE = DateTime.Now;
                        etys[0].FLOW_UPDATEBY = Session["loginname"].ToString();
                        cnt = bz.Update(etys[0]);    
                    }                    
                }

                //存子表               
                /// -- start -- 以下逻辑只为满足derek要求，目的是在编辑情况下不变更子表的自增id，因为这些自增id被用在其他业务逻辑里。需要考虑“新建”还是“更新”状态。
                if (act == "new") //如果是新建，则直接保存
                {
                    for (int i = 0; i < dtl.Count; i++)
                    {
                        EFlowDetailModel etyDtl = new EFlowDetailModel();
                        etyDtl.FLOW_ID = flowId;
                        etyDtl.FWDE_SEQ = (i + 1) * 10;
                        etyDtl.FWDE_CODE = dtl[i].FWDE_CODE;
                        etyDtl.FWDE_NAME = dtl[i].FWDE_NAME;
                        etyDtl.FWDE_GROUPID = dtl[i].FWDE_GROUPID;
                        etyDtl.FWDE_GROUP = dtl[i].FWDE_GROUP;
                        etyDtl.FWDE_EMAIL = dtl[i].FWDE_EMAIL;
                        etyDtl.FWDE_ACTIVE = "A";
                        cnt = bz.Create(etyDtl);
                    }
                }
                else
                {
                    List<int> subids = new List<int>();
                    for (int k = 0; k < dtl.Count; k++)
                    {                        
                        List<EFlowDetailModel> subetys = bz.Retrieve<EFlowDetailModel>(x => x.FLOW_ID == flowId && x.FWDE_ID == dtl[k].FWDE_ID);
                        if (subetys.Count > 0) //如果原来的子表记录存在
                        {
                            subetys[0].FWDE_SEQ = (k + 1) * 10;
                            subetys[0].FWDE_CODE = dtl[k].FWDE_CODE;
                            subetys[0].FWDE_NAME = dtl[k].FWDE_NAME;
                            subetys[0].FWDE_GROUPID = dtl[k].FWDE_GROUPID;
                            subetys[0].FWDE_GROUP = dtl[k].FWDE_GROUP;
                            subetys[0].FWDE_EMAIL = dtl[k].FWDE_EMAIL;
                            subetys[0].FWDE_ACTIVE = "A";
                            cnt = bz.Update(subetys[0]);
                            subids.Add(dtl[k].FWDE_ID);
                        }
                        else //否则就创建子表记录
                        {   
                            EFlowDetailModel etyDtl = new EFlowDetailModel();
                            etyDtl.FLOW_ID = flowId;
                            etyDtl.FWDE_SEQ = (k + 1) * 10;
                            etyDtl.FWDE_CODE = dtl[k].FWDE_CODE;
                            etyDtl.FWDE_NAME = dtl[k].FWDE_NAME;
                            etyDtl.FWDE_GROUPID = dtl[k].FWDE_GROUPID;
                            etyDtl.FWDE_GROUP = dtl[k].FWDE_GROUP;
                            etyDtl.FWDE_EMAIL = dtl[k].FWDE_EMAIL;
                            etyDtl.FWDE_ACTIVE = "A";
                            cnt = bz.Create(etyDtl);

                            int sid = Convert.ToInt32(tx.ExecuteScalar("select Seq4Flow_Detail_Id.Currval from dual"));//拿到自增ID值(sid)
                            subids.Add(sid);
                        }
                    }

                    //以下是更新操作时，删除不存在于新子表的记录。如果没有不存在的，则不影响现有记录。                                     
                    List<EFlowDetailModel> etysdel = bz.Retrieve<EFlowDetailModel>(x => x.FLOW_ID == flowId && x.FWDE_ID.Sql_NotIn(subids));
                    cnt = bz.Delete(etysdel);//若没有记录被删除，则返回0
                }                
                /// --  end  --

                //-- 以下是原来的更新方式
                //bz.WhereClauses.Add("Flow_Id", new Helper.Evaluation(Helper.SqlOperator.EQ, flowId));
                //List<EFlowDetailModel> etys2 = bz.Retrieve<EFlowDetailModel>();
                //bz.Delete(etys2);
                //for (int i = 0; i < dtl.Count; i++)
                //{
                //    EFlowDetailModel etyDtl = new EFlowDetailModel();                    
                //    etyDtl.FLOW_ID = flowId;
                //    etyDtl.FWDE_SEQ = (i + 1) * 10;
                //    etyDtl.FWDE_CODE = dtl[i].FWDE_CODE;
                //    etyDtl.FWDE_NAME = dtl[i].FWDE_NAME;
                //    etyDtl.FWDE_GROUPID = dtl[i].FWDE_GROUPID;
                //    etyDtl.FWDE_EMAIL = dtl[i].FWDE_EMAIL;
                //    etyDtl.FWDE_ACTIVE = "A";
                //    cnt = bz.Create(etyDtl);
                //}
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();

                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }        

        /// <summary>
        /// 检索（“derek提供”）
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pidx"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        ///    Function Get_EFLow_By_BackUser(
        ///                      Pm_LoginName    In Varchar2, -- 后台用户登录名
        ///                      Pm_GroupName    In Varchar2, -- 后台用户登录时候所使用的权限组
        ///                      Pm_PropertyList In Varchar2, -- E_Form定义中调用此函数传入Form_Property，否则为空串
        ///                      Pm_FlowStatus   In Varchar2, -- E_Form的状态A已审批（用于Form内下拉选择），空串是所有（用于流程维护界面）
        ///                      Pm_Search       In Varchar2) -- E_Flow查询界面，传入右上角的查询内容
        ///                      Return Sys_RefCursor;
        private string Search(string criteria, int pidx, int pageSize = 10)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", Helper.Aid.Null2Str(Session["loginname"])));
                Model.Sys_Users_Group gr = Session["groupinfo"] as Model.Sys_Users_Group;
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", Helper.Aid.Null2Str(gr.GroupName)));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_PropertyList", ""));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FlowStatus", ""));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Search", criteria));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue));
                List<EFLowModelQuery> etys = bz.Retrieve9iSp<EFLowModelQuery>("pkg_eform_flow.Get_EFLow_By_BackUser", paras.ToArray());
                /// --  end  --
               
                //带出子表数据
                for (int i = 0; i < etys.Count; i++)
                {                    
                    List<EFlowDetailModel> etysDtl = bz.Retrieve<EFlowDetailModel>(x => x.FLOW_ID == etys[i].Flow_Id);
                    etysDtl.Sort((x, y) =>
                    {
                        if (x.FWDE_SEQ < y.FWDE_SEQ)
                            return -1;
                        else if (x.FWDE_SEQ > y.FWDE_SEQ)
                            return 1;
                        else
                            return 0;
                    });
                    etys[i].detail = etysDtl;
                }
                rsp.result = this.SetPager(etys, pidx, pageSize);
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

    /// <summary>
    /// EFlow实体(insert,update,delete)
    /// </summary>
    public class EFLowModel
    {
        public const string TABLE_NAME = "E_Flow";

        [DbField(true)]
        public int Flow_Id { get; set; }

        public string Flow_Code { get; set; }
        public string FLOW_NAME { get; set; }
        public DateTime FLOW_EFFFROM { get; set; }
        public DateTime FLOW_EFFTO { get; set; }
        public string FLOW_PROPERTY { get; set; }
        public string FLOW_DESC { get; set; }
        public int FLOW_STEPSUM { get; set; }
        public string FLOW_ACTIVE { get; set; }
        public string FLOW_STATUS { get; set; }
        public string FLOW_CREATEBY { get; set; }
        public DateTime FLOW_CREATEDATE { get; set; }
        public string FLOW_UPDATEBY { get; set; }
        public DateTime FLOW_UPDATEDATE { get; set; }
        public string FLOW_APPROVEBY { get; set; }
        public Nullable<DateTime> FLOW_APPROVEDATE { get; set; }

        [DbField(false, false, true)]
        public List<EFlowDetailModel> detail = new List<EFlowDetailModel>();
    }

    /// <summary>
    /// EFlow实体（select）
    /// </summary>
    /// 注意:只能用于查询，否则报错
    public class EFLowModelQuery
    {
        public const string TABLE_NAME = "E_Flow";

        [DbField(true)]
        public int Flow_Id { get; set; }

        public string Flow_Code { get; set; }
        public string FLOW_NAME { get; set; }
        public DateTime FLOW_EFFFROM { get; set; }
        public DateTime FLOW_EFFTO { get; set; }
        public string FLOW_PROPERTY { get; set; }
        public string FLOW_DESC { get; set; }
        public int FLOW_STEPSUM { get; set; }
        
        [DbField(false,false,true)]
        public string FLOW_ACTIVE { get; set; } // “derek”提供的查询里不包含该字段，故需要屏蔽

        public string FLOW_STATUS { get; set; }
        public string FLOW_CREATEBY { get; set; }
        public DateTime FLOW_CREATEDATE { get; set; }
        public string FLOW_UPDATEBY { get; set; }
        public DateTime FLOW_UPDATEDATE { get; set; }
        public string FLOW_APPROVEBY { get; set; }
        public Nullable<DateTime> FLOW_APPROVEDATE { get; set; }

        [DbField(false, false, true)]
        public List<EFlowDetailModel> detail = new List<EFlowDetailModel>();
    }  

    public class EFlowDetailModel
    {
        public const string TABLE_NAME = "E_FlowDetail";
        
        [DbField(true,true)]
        public int FWDE_ID { get; set; }
        public int FLOW_ID { get; set; }
        public int FWDE_SEQ { get; set; }
        public string FWDE_CODE { get; set; }
        public string FWDE_NAME { get; set; }
        public int FWDE_FINALSEQ { get; set; }
        public string FWDE_GROUP { get; set; }
        public int FWDE_GROUPID { get; set; }
        public string FWDE_EMAIL { get; set; }
        public int FWDE_NEXTSEQ { get; set; }
        public int FWDE_PRESEQ { get; set; }
        public string FWDE_WHERE { get; set; }
        public string FWDE_ACTIVE { get; set; }
    }
}

/*
 vue = new Vue({
    el: "",
    data: {
        log: true,      //是否console输出
        row: {},        //当前编辑的行
        page: {
            rows: [],   //当前页条目数组
            size: 10,   //每页条目数
            count: 0,   //总页数
            idx: 1,     //当前页
            range: 5,   //可显示的页码范围
            start: 1    //可显示的起始页码
        },
        status: "new"   //editor status. 'new' or 'edit'
    },
    filters: {
    },
    methods: {
        init: function () { },
        search: function (c, i, s) { //c,i,s => criteria,pidx,psize

        },
        create: function () {

        },
        edit: function () {

        },
        del: function (id) {

        },
        jump: function (pidx) {

        },
        save: function () {

        },
        cleanup: function () {

        },
        txtChange: function () { // 用于检索

        }
    }
 });
 vue.init();
 * 
    <!--修正-->
    <div class="row"></div>     
    <!--检索-->
    <!--表格-->
    <!--翻页-->
    <!--模态窗口-->
    <!--模态提示对话框 -->
 */
