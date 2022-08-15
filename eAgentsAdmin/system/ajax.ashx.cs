using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
namespace PropertyOneAppWeb.system
{
    public class JsonRsp
    {
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public Object result { get; set; }

        public JsonRsp()
        {
            this.err_code = 0;
            this.err_msg = "";
            this.result = null;
        }

        public JsonRsp(int code, string msg)
        {
            this.err_code = code;
            this.err_msg = msg;
        }
    }

    /// <summary>
    /// “eAgents”请求接口调用类
    /// </summary>
    /// 待优化
    public class ajax : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public static string vp = "";//虚拟路径
        string webRootPath = "";
        const string PKGSP_SUBMIT_PROCESSED_EFORM = "submit_processed_eform";
        const string PKGSP_REJECT_PROCESSED_EFORM = "reject_processed_eform";
        const string PKGSP_CLOSE_PROCESSED_EFORM = "close_processed_eform";
        const string PKGSP_SAVE_REMARKS = "save_remarks";

        /// -- start -- 每个表单的保存逻辑有可能不一样，所以这里分别处理。
        /// derek为每个表单定义了一个id，该值永久不变，故定义成常量。(distinguished by fmtbid)
        const string PKGSP_SAVE120000 = "save_120000"; //Form_AS
        const string PKGSP_SAVE130000 = "save_130000"; //Form_B
        const string PKGSP_SAVE140000 = "save_140000"; //Form_E
        const string PKGSP_SAVE150000 = "save_150000"; //Form_REC
        const string PKGSP_SAVE160000 = "save_160000"; //Form_EHAC        
        const string PKGSP_SAVE170000 = "save_170000"; //Form_VP
        /// --  end  --

        const string PKGSP_GET_LONIN_PROFILE = "get_login_profile";
        const string PKGSP_GET_EFLOW_BY_BACKUSER = "get_eflow_by_backuser";

        public void ProcessRequest(HttpContext context)
        {
            //context.Request                            
            webRootPath = context.Request.MapPath(".");
            string action = Helper.Aid.Null2Str(context.Request.QueryString["act"]);
            string result = "";
            JsonRsp jre = new JsonRsp();
            try
            {
                if (Aid.Null2Str(context.Session["loginname"]) == "" && (context.Request.UserHostAddress != "::1" || context.Request.UserHostAddress != "127.0.0.1"))
                    action = "timeout";

                //收集参数(因为大部分都是通用参数，故一次性赋值)
                int grpid = Helper.Aid.Null2Int(context.Request.Form["groupid"]);
                string loginname = Helper.Aid.Null2Str(context.Request.Form["loginname"]);
                string groupname = Helper.Aid.Null2Str(context.Request.Form["groupname"]);
                int formid = Helper.Aid.Null2Int(context.Request.Form["formid"]);
                int fmtbid = Helper.Aid.Null2Int(context.Request.Form["fmtbid"]);
                int operid = Helper.Aid.Null2Int(context.Request.Form["operid"]);
                int fwdeid = Helper.Aid.Null2Int(context.Request.Form["fwdeid"]);
                int fmdtid = Helper.Aid.Null2Int(context.Request.Form["fmdtid"]); //子表流水id
                int fwdeseq = Helper.Aid.Null2Int(context.Request.Form["fwdeseq"]);
                string reason = Helper.Aid.Null2Str(context.Request.Form["reason"]);
                string remark = Helper.Aid.Null2Str(context.Request.Form["remark"]);
                string refno = Helper.Aid.Null2Str(context.Request.Form["refno"]);
                string from = Helper.Aid.Null2Str(context.Request.Form["from"]); //（preview 时的调用，from=def）
                string json = Helper.Aid.Null2Str(context.Request.Form["eformdata"]);
                string subjson = Helper.Aid.Null2Str(context.Request.Form["subs"]);

                switch (action)
                {
                    case PKGSP_SAVE_REMARKS:
                        result = this.SaveRemarks(operid, formid, fwdeid, remark, loginname, json);
                        break;

                    case PKGSP_GET_LONIN_PROFILE:
                        grpid = Helper.Aid.Null2Int(context.Request.Form["groupid"]);
                        result = this.GetLoginProfile(loginname, grpid);
                        break;

                    case PKGSP_GET_EFLOW_BY_BACKUSER:
                        string propertylist = Helper.Aid.Null2Str(context.Request.Form["propertylist"]);
                        string status = Helper.Aid.Null2Str(context.Request.Form["status"]);
                        string search = Helper.Aid.Null2Str(context.Request.Form["search"]);
                        result = this.GetEFLowByBackUser(loginname, groupname, propertylist, status, search);
                        break;

                    case PKGSP_SUBMIT_PROCESSED_EFORM:
                        result = this.SubmitProcessedEform(formid, operid, refno, loginname, groupname, fwdeid, remark, json);
                        break;

                    case PKGSP_REJECT_PROCESSED_EFORM:
                        result = this.RejectProcessedEform(formid, operid, refno, reason, loginname, groupname);
                        break;

                    case PKGSP_CLOSE_PROCESSED_EFORM:
                        result = this.CloseProcessedEform(formid, operid, refno, loginname, groupname, fwdeid, remark);
                        break;

                    case PKGSP_SAVE120000:
                        json = Helper.Aid.Null2Str(context.Request.Form["eformdata"]);
                        result = this.SaveFormAS(operid, formid, fwdeid, remark, loginname);
                        break;

                    case PKGSP_SAVE140000:
                        result = this.SaveFormE(operid, formid, fwdeid, remark, loginname);
                        break;

                    case PKGSP_SAVE130000:
                        result = this.SaveFormB(operid, formid, fwdeid, remark, loginname, subjson);
                        break;

                    case PKGSP_SAVE150000:
                        result = this.SaveFormREC(operid, formid, fwdeid, remark, loginname);
                        break;

                    case PKGSP_SAVE160000:
                        result = this.SaveFormEHAC(operid, formid, fwdeid, remark, loginname);
                        break;

                    case PKGSP_SAVE170000:
                        result = this.SaveFormVP(operid, formid, fwdeid, remark, loginname);
                        break;

                    case "load":
                        result = this.Load(formid, fmtbid, operid, loginname, refno, fwdeseq, fmdtid, "", groupname, from);
                        break;

                    case "demo":
                        result = "demo invoked.";
                        DbConsoleExam.Demo();
                        break;

                    case "timeout":
                        jre.err_code = -98; //session time out.
                        jre.err_msg = "session time out.";
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(jre);
                        break;

                    case "html2pdf":
                        result = this.GetHtml2Pdf(operid);
                        break;

                    default:
                        jre.err_code = -1;
                        jre.err_msg = "action is required or not found[" + action + "].";
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(jre);
                        //TestInform(); // need delete after test.
                        break;
                }

            }
            catch (Exception err)
            {
                jre.err_code = -99;
                jre.err_msg = err.Message;
                jre.result = null;
                result = Newtonsoft.Json.JsonConvert.SerializeObject(jre);
            }
            context.Response.Write(result);

            /*
             * sample code
                JsonRsp rsp = new JsonRsp();
                Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
                try
                {

                    tx.CommitTrans();
                }
                catch (Exception err)
                {
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    rsp.result = null;
                    tx.AbortTrans();
                }
                return JsonConvert.SerializeObject(rsp);
           */
        }

        private void TestInform()
        { 
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                PropertyOneAppWeb.system.EformMailSender email = new system.EformMailSender(tx);
                email.Send("EAgent_BatchInformStaff_NTC_EFM", "G", DateTime.Parse("2022-05-22"), true);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// 保存FormVP数据
        /// </summary>
        /// <param name="operid"></param>
        /// <param name="formid"></param>
        /// <param name="fwdeid"></param>
        /// <param name="remark"></param>
        /// <param name="loginname"></param>
        /// <returns></returns>
        private string SaveFormVP(int operid, int formid, int fwdeid, string remark, string loginname)
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            { }
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        /// Form页面数据加载
        /// </summary>
        /// <param name="operid"></param>
        /// <param name="formid"></param>
        private string Load(int formid, int fmtbid, int operid, string loginname, string refno, int fwdeseq, int fmdtid, string leasenumber, string groupname = "", string from = "")
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, false, true);
            try
            {
                EformBiz50 bz = new EformBiz50(tx);
                EformLoadQuery dom = new EformLoadQuery();
                dom.fields = bz.GetEFormData(formid, fmtbid, operid, loginname, refno, leasenumber, groupname);
                dom.defs = bz.GetEformDef(formid);
                dom.flowinfo = bz.GetEflowInfo(operid, loginname, groupname, formid);
                if (from == "")
                {
                    dom.privilege = bz.GetEformOperation(refno, operid, fwdeseq, loginname, groupname);
                }
                //如果有子表
                for (int i = 0; i < dom.fields.Count; i++)
                {
                    if (dom.fields[i].field_code == "Sub_Table")
                    {
                        dom.subctrls = bz.GetEFormSubFields(formid, fmtbid, operid, fmdtid, refno, leasenumber, loginname, groupname);
                        dom.subdata = bz.GetEformSubData(operid, formid, fmtbid, refno, leasenumber, loginname, groupname);
                        break;
                    }
                }
                //返回attach文件(名)列表，若不存在则创建
                //1.找到operid下的所有附件                
                /// -- start -- 测试三目运算解析
                List<EformAttachModel> etys = bz.Retrieve<EformAttachModel>(x =>
                    x.Oper_Id == (operid > 0 ? operid : -1) &&
                    (x.Fmat_Status != "D" || x.Fmat_Status.Sql_IsNull())); //注意：“Fmat_Status”这种用一个西文字符来标识状态的字段在数据库里的长度最好是1，否则可能取不出来。
                /// --  end  --
                
                List<EformAttachModel> atchs = new List<EformAttachModel>();

                if (string.IsNullOrEmpty(from))
                {
                    for (int j = 0; j < etys.Count; j++)
                    {
                        if (string.IsNullOrEmpty(etys[j].Fmat_FileUrl))
                        {
                            string filename = Aid.Null2Str(Aid.getTimestamp() / 1000) + "_" + etys[j].Fmat_FileName;
                            string pfilepath = Aid.yPath + "Upload\\" + filename;
                            string vfilePath = "Upload/" + filename;
                            System.IO.Stream s = new System.IO.FileStream(pfilepath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                            //2.生成真实文件
                            byte[] wd = etys[j].Fmat_FileData;
                            s.Write(wd, 0, wd.Length);
                            s.Flush();
                            s.Close();

                            etys[j].Fmat_FileUrl = vfilePath;
                            int cnt = bz.Update(etys[j]);
                        }
                        //3.返回url
                        atchs.Add(new EformAttachModel(operid, etys[j].Fmat_FileName, Aid.vPath + etys[j].Fmat_FileUrl));
                    }
                    dom.atchs = atchs;
                }

                rsp.result = dom;
                tx.CommitTrans();

                //string htmlFile = this.GetStaticHtml(dom, fmtbid);
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

        /// <summary>
        /// 保存表单名FormB的相关数据
        /// </summary>
        /// <param name="operid"></param>
        /// <param name="formid"></param>
        /// <param name="fwdeid"></param>
        /// <param name="remark"></param>
        /// <param name="loginname"></param>
        /// <returns></returns>
        private string SaveFormB(int operid, int formid, int fwdeid, string remark, string loginname, string subs = "")
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            {
                rsp = new JsonRsp();
                int cnt = 1;
                Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                try
                {
                    // 2.处理其他保存内容                     
                    List<EFormDataModel> ss = JsonConvert.DeserializeObject<List<EFormDataModel>>(subs);
                    DbConsole bz = new DbConsole(tx);
                    for (int i = 0; i < ss.Count; i++)
                    {
                        List<EFormDataModel> etys = bz.Retrieve<EFormDataModel>(x => x.Oper_Id == operid && x.Field_Id == ss[i].Field_Id && x.Fmdt_Id == ss[i].Fmdt_Id);
                        if (etys.Count > 0)
                        {
                            etys[0].Fmdt_Value = ss[i].Fmdt_Value;
                            cnt = bz.Update(etys[0]);
                        }
                    }

                    tx.CommitTrans();
                    if (cnt > 0)
                    {
                        rsp.result = true;
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.result = false;
                        rsp.err_msg = "update failure";
                    }
                }
                catch (Exception err)
                {
                    tx.AbortTrans();
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    rsp.result = null;
                }
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string SaveFormE(int operid, int formid, int fwdeid, string remark, string loginname)
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            { }
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        /// 保存表单名FormAS的相关数据
        /// </summary>
        /// <returns></returns>
        private string SaveFormAS(int operid, int formid, int fwdeid, string remark, string loginname = "", string eformdatajson = "")
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            {
                rsp = new JsonRsp();
                int cnt = 1;
                Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
                try
                {
                    // 2.处理其他保存内容 ,暂未实现
                    tx.CommitTrans();

                    if (cnt > 0)
                    {
                        rsp.result = true;
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.result = false;
                        rsp.err_msg = "update failure";
                    }
                }
                catch (Exception err)
                {
                    tx.AbortTrans();
                    rsp.err_code = -99;
                    rsp.err_msg = err.Message;
                    rsp.result = null;
                }
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string SaveFormEHAC(int operid, int formid, int fwdeid, string remark, string loginname)
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            { }
            return JsonConvert.SerializeObject(rsp);
        }

        private string SaveFormREC(int operid, int formid, int fwdeid, string remark, string loginname)
        {
            // 1.先处理一次基本保存
            string result = this.SaveFormFlow(operid, formid, fwdeid, remark, loginname);
            JsonRsp rsp = JsonConvert.DeserializeObject<JsonRsp>(result);
            if (rsp.err_code == 0) // 如果处理成功
            { }
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        /// 保存formflow表数据，主要是remarks字段
        /// </summary>
        /// 由于derek已经提供了数据生成逻辑，这里只做update
        /// <param name="operid"></param>
        /// <param name="formid"></param>
        /// <param name="fwdeid"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        private string SaveFormFlow(int operid, int formid, int fwdeid, string remark, string loginname = "")
        {
            int cnt = 0;
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<EFormFlowModel> etys = bz.Retrieve<EFormFlowModel>(x => x.Oper_Id == operid && x.Form_Id == formid && x.Fwde_Id == fwdeid);
                if (etys.Count > 0)
                {
                    etys[0].Fmfw_Remarks = remark;
                    etys[0].Fmfw_UpdateBy = loginname;
                    etys[0].Fmfw_UpdateDate = DateTime.Now;
                    etys[0].Fmfw_Status = "S"; //固定填'S'，表示保存，derek要用。
                    cnt = bz.Update(etys[0]);
                    rsp.result = true;
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "record not found.";
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }
            string result = JsonConvert.SerializeObject(rsp);
            Log.WriteLog(loginname + " invoke [save] with result: " + result);
            return result;
        }

        /// <summary>
        /// 后台用户获取从表表单明细 
        /// </summary>
        /// <returns></returns>
        /// 当表单的类型（FmTb_Type）是P(Primary)，意味着这是主从表的主表，需要再次调用访问从表表单明细
        /// -- 根据P_OperId和P_FormId进行表单信息获取，前者（Oper_Id）为空，则返回对应的Form_Id的空表单信息，有值则返回用户之前提交的表单信息
        /// -- select pkg_eform_flow.Get_EForm_SubData(328, 110000, 0, '', '', '') from dual;
        /// Function Get_EForm_SubData(
        ///     Pm_FormId      In Number, -- 表单流水号
        ///     Pm_FmTbId      In Number, -- 表单所对应的数据表流水号
        ///     Pm_OperId      In Number, -- 申请单的操作流水号
        ///     Pm_OperRefNo   In Varchar2, -- 申请单编号
        ///     Pm_LeaseNumber In Varchar2, -- 前台登陆用户所属租约，后台用户登录传入空串
        ///     Pm_LoginName   In Varchar2, -- 前台/后台登陆用户名
        ///     Pm_GroupName   In Varchar2 -- 前台/后台登陆用户所属于权限组，前台用户恒输入FRONTUSER
        /// ) Return Sys_RefCursor;
        private string GetEFormSubData(int formid, int fmtbid, int operid, int fmdtid, string refno, string leasenumber, string loginname, string groupname)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FormId", formid));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FmTbId", fmtbid));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FmDtId", fmdtid));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperRefNo", refno));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LeaseNumber", leasenumber));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
                List<EFormDataFieldQuery> etys = bz.Retrieve9iSp<EFormDataFieldQuery>("pkg_eform_flow.Get_EForm_SubData", paras.ToArray());
                rsp.result = etys;
                /// --  end  --               
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

        private string SaveRemarks(int operid, int formid, int fwdeid, string remark, string loginname = "", string eformdatajson = "")
        {
            int cnt = 0;
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<EFormFlowModel> etys = bz.Retrieve<EFormFlowModel>(x => x.Oper_Id == operid && x.Form_Id == formid && x.Fwde_Id == fwdeid);
                if (etys.Count > 0)
                {
                    etys[0].Fmfw_Remarks = remark;
                    etys[0].Fmfw_Status = "S"; //固定填'S'，表示保存，derek要用。
                    etys[0].Fmfw_UpdateBy = loginname;
                    etys[0].Fmfw_UpdateDate = DateTime.Now;

                    ///-- start -- 保存eformdata 
                    if (eformdatajson != "")
                    {
                        List<EFormDataModel> paras = JsonConvert.DeserializeObject<List<EFormDataModel>>(eformdatajson);

                        for (int i = 0; i < paras.Count; i++)
                        {
                            List<EFormDataModel> etysed = bz.Retrieve<EFormDataModel>(x => x.Oper_Id == operid && x.Field_Id == paras[i].Field_Id);
                            if (etysed.Count > 0)
                            {
                                etysed[0].Fmdt_Value = paras[i].Fmdt_Value;
                                etysed[0].Fmdt_UpdateBy = loginname;
                                etysed[0].Fmdt_UpdateDate = DateTime.Now;
                                cnt = bz.Update(etysed[0]);
                            }
                        }
                    }
                    ///--  end  --

                    cnt = bz.Update(etys[0]);
                    if (cnt > 0)
                        rsp.result = true;
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "update failure";
                        rsp.result = false;
                    }
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = "record not found.";
                }

                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }

            WriteActionLog(loginname, "sare remark", rsp);
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        ///  后台用户关闭前台用户申请的表单，返回值为1表示关闭成功
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="operid"></param>
        /// <param name="operrefno"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        /// Function Close_Processed_EForm(
        ///         Pm_FormId    In Number,
        ///         Pm_OperId    In Number,
        ///         Pm_OperRefNo In Varchar2,
        ///         Pm_LoginName In Varchar2,
        ///         Pm_GroupName In Varchar2) Return NUMBER;
        private string CloseProcessedEform(int formid, int operid, string operrefno, string loginname, string groupname, int fwdeid, string remark)
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                string result = this.SaveRemarks(operid, formid, fwdeid, remark, loginname);
                JsonRsp rsp1 = JsonConvert.DeserializeObject<JsonRsp>(result);

                if (rsp1.err_code != 0)
                {
                    rsp.err_code = rsp1.err_code;
                    rsp.err_msg = rsp1.err_msg;
                }
                else
                {
                    Oracle9i o9i = new Oracle9i(GlobalUtilSystem.sdb_local);
                    o9i.AddParameter("Pm_FormId", formid);
                    o9i.AddParameter("Pm_OperId", operid);
                    o9i.AddParameter("Pm_OperRefNo", operrefno);
                    o9i.AddParameter("Pm_LoginName", loginname);
                    o9i.AddParameter("Pm_GroupName", groupname);
                    o9i.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Number, System.Data.ParameterDirection.ReturnValue, 100);
                    int retVal = Helper.Aid.Null2Int(o9i.ExecuteStoredProcedure("pkg_eform_flow.Close_Processed_EForm"));

                    if (retVal == 1)
                        rsp.result = true;
                    else
                        rsp.result = false;

                    EformMailSender ems = new EformMailSender(tx);
                    ems.Send(operid, "C", loginname, groupname);
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }

            WriteActionLog(loginname, "finish", rsp);
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        /// 后台用户拒绝前台用户申请的表单，返回值为1表示驳回成功
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="operid"></param>
        /// <param name="operrefno"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        ///  Function Reject_Processed_EForm(
        ///     Pm_FormId       In Number,
        ///     Pm_OperId       In Number,
        ///     Pm_OperRefNo    In Varchar2,
        ///     Pm_RejectReason In Varchar2,
        ///     Pm_LoginName    In Varchar2,
        ///     Pm_GroupName    In Varchar2) Return NUMBER;
        private string RejectProcessedEform(int formid, int operid, string operrefno, string reason, string loginname, string groupname)
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Oracle9i o9i = new Oracle9i(GlobalUtilSystem.sdb_local);
                o9i.AddParameter("Pm_FormId", formid);
                o9i.AddParameter("Pm_OperId", operid);
                o9i.AddParameter("Pm_OperRefNo", operrefno);
                o9i.AddParameter("Pm_RejectReason", reason);
                o9i.AddParameter("Pm_LoginName", loginname);
                o9i.AddParameter("Pm_GroupName", groupname);
                o9i.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Number, System.Data.ParameterDirection.ReturnValue, 100);
                int retVal = Helper.Aid.Null2Int(o9i.ExecuteStoredProcedure("pkg_eform_flow.Reject_Processed_EForm"));

                if (retVal == 1)
                    rsp.result = true;
                else
                    rsp.result = false;

                EformMailSender ems = new EformMailSender(tx);
                ems.Send(operid, "R", loginname, groupname);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }

            WriteActionLog(loginname, "reject", rsp);
            return JsonConvert.SerializeObject(rsp);
        }

        /// <summary>
        /// 后台用户提交表单，返回值为1表示提交成功
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="operid"></param>
        /// <param name="operrefno"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        ///  Function Submit_Processed_EForm(Pm_FormId    In Number,
        ///                                  Pm_OperId    In Number,
        ///                                  Pm_OperRefNo In Varchar2,
        ///                                  Pm_LoginName In Varchar2,
        ///                                  Pm_GroupName In Varchar2) Return NUMBER;        
        private string SubmitProcessedEform(int formid, int operid, string operrefno, string loginname, string groupname, int fwdeid, string remark, string eformdatajson = "")
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                string result = this.SaveRemarks(operid, formid, fwdeid, remark, loginname, eformdatajson);
                JsonRsp rsp1 = JsonConvert.DeserializeObject<JsonRsp>(result);

                if (rsp1.err_code != 0)
                {
                    rsp.err_code = rsp1.err_code;
                    rsp.err_msg = rsp1.err_msg;
                }
                else
                {
                    Oracle9i o9i = new Oracle9i(GlobalUtilSystem.sdb_local);
                    o9i.AddParameter("Pm_FormId", formid);
                    o9i.AddParameter("Pm_OperId", operid);
                    o9i.AddParameter("Pm_OperRefNo", operrefno);
                    o9i.AddParameter("Pm_LoginName", loginname);
                    o9i.AddParameter("Pm_GroupName", groupname);
                    o9i.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Number, System.Data.ParameterDirection.ReturnValue, 100);
                    int retVal = Helper.Aid.Null2Int(o9i.ExecuteStoredProcedure("pkg_eform_flow.Submit_Processed_EForm"));

                    if (retVal == 1)
                        rsp.result = true;
                    else
                        rsp.result = false;

                    EformMailSender ems = new EformMailSender(tx);
                    ems.Send(operid, "S", loginname, groupname);
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
                Log.WriteLog(err.Message + "\r\n\t" + err.StackTrace);
            }
            WriteActionLog(loginname, "submit", rsp);
            return JsonConvert.SerializeObject(rsp);
        }

        private void SendSubmitMail()
        {
            DbTx tx = new DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                EformMailSender ems = new EformMailSender(tx);
                ems.Send(103, "C", "kin", "EM WG02 Group");          //complete
                ems.Send(102, "S", "WG0200388M09", "EM WG02 Group"); //submit
                ems.Send(104, "R", "WG0200388M09", "EM WG02 Group"); //reject 104
                ems.Send(106, "W", "WG0200388M09", "FrontUser"); //withdraw 106
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
            }
        }

        /// <summary>
        /// 获取当前登录用户的基本数据
        /// </summary>
        /// <param name="loginname"></param>
        /// <param name="grpid"></param>
        /// <returns>
        ///  login      当前登录用户
        ///  grpinfo    当前登录用户的组信息
        ///  grps       当前登录用户所属的所有组信息
        ///  props      当前登录用户可访问的property list
        /// </returns>        
        private string GetLoginProfile(string loginname, int grpid)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                LoginProfile lpro = new LoginProfile();

                //1.取当前登录用户信息
                bz.WhereClauses.Add("upper(trim(LoginName))", new Helper.Evaluation(Helper.SqlOperator.EQ, loginname.Trim().ToUpper()));
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>();
                if (etys.Count == 0)
                    throw new Exception("loginname not found.");
                lpro.login = etys[0];

                //2.取当前用户的所有组                
                List<Model.Sys_Login_System_Group> etys2 = bz.Retrieve<Model.Sys_Login_System_Group>(x => x.UserId == lpro.login.UserId);
                for (int i = 0; i < etys2.Count; i++)
                {
                    List<Model.Sys_Users_Group> etysGrp = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == etys2[i].GroupId && x.Status == "A"); //总是返回一条
                    if (etysGrp.Count > 0)
                    {
                        lpro.grps.Add(etysGrp[0]);

                        if (etysGrp[0].GroupId == grpid) // 取当前登录用户的组信息
                            lpro.grpinfo = etysGrp[0];
                    }
                }

                lpro.grps.Sort((x, y) => { return x.GroupName.CompareTo(y.GroupName); });

                //3.取当前用户的props信息
                List<Model.Sys_Users_Group_Property> etys4 = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == grpid, " order by PropertyCode");

                for (int i = 0; i < etys4.Count; i++)
                {
                    etys4[i].PropertyCode = etys4[i].PropertyCode.Trim();
                }
                lpro.props = etys4;
                rsp.result = lpro;
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
        /// 后台用户获取流信息
        /// </summary>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <param name="propertylist"></param>
        /// <param name="status"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private string GetEFLowByBackUser(string loginname, string groupname, string propertylist, string status, string search)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_PropertyList", propertylist));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FlowStatus", status));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Search", search));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
                List<EFLowModelQuery> etys = bz.Retrieve9iSp<EFLowModelQuery>("pkg_eform_flow.Get_EFLow_By_BackUser", paras.ToArray());
                /// --  end  --          
                rsp.result = etys;
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

        private string GetHtml2Pdf(int operid)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                EformBiz50 bz2 = new EformBiz50(tx);
                rsp = bz2.GetStaticHtml2Pdf(operid, false);
                tx.CommitTrans();

                if (rsp.err_code != 0)
                    Log.WriteLog("Html2Pdf error:" + rsp.err_msg);
                else
                {
                    PdfRsp r = (PdfRsp)rsp.result;
                    Log.WriteLog("Html2Pdf ->" + r.vPath);
                }
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private void WriteActionLog(string loginname, string action, JsonRsp rsp)
        {
            Log.WriteLog(loginname + " invoke [" + action + "] with result:\r\n\t" + JsonConvert.SerializeObject(rsp));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    #region EformMailSender 业务类(邮件发送处理)

    public class EformMailSender : DbConsole
    {
        /*
            //1.取模板  
            //2.取收件人及内容 gettpl(operid,optype,loginname,logingroup) select pkg_eform_flow.Get_Flow_Email(102,'Submit','kin','EM WG02 Group') from dual         
            //3.组装
            //4.发送
            //5.更新状态
         */

        System.Xml.XmlElement root = null;
        string subject = "";
        string content = "";
        int operid = 0;
        string loginname = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// 使用模板EAgent_InformTemplate.xml
        public EformMailSender(DbTx tx, string xmlFilePath = "")
            : base(tx)
        {
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            xd.Load(string.IsNullOrEmpty(xmlFilePath) ? Helper.Aid.yPath + @"system\EAgent_InformTemplate.xml" : xmlFilePath);
            this.root = xd.DocumentElement; //获取根节点
        }

        public void Send(int operid, string type, string loginname, string groupname)
        {
            this.operid = operid;
            this.loginname = loginname;
            List<InFrmQuery> ifs = this.GetFlowEmail(operid, type, loginname, groupname);

            List<string> tos = new List<string>();
            ExchangeMail em = new ExchangeMail();
            string dev = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["dev"]);
            Commom.Log.WriteLog("-- start -- 'eform'业务发送邮件");
            for (int i = 0; i < ifs.Count; i++) //循环收件人
            {
                if (ifs[i].INFRM_EMAILSTATUS == "S") continue;

                string tpl = ifs[i].INFRM_TEMPLATE;
                // 1.识别区域(hk,prc)及业务,获取模板
                System.Xml.XmlNode node = root.SelectSingleNode("//template[@name='" + tpl + "']");//通过xquery查找指定的模板。
                Rig(tpl, node, ifs[i]);

                if (dev == "1")
                {
                    if (tos.Count > 0)
                    {

                    }
                    else
                    {
                        tos.Add("liwei@hwpg.com");
                        tos.Add("derekyu@hwpg.com");
                        Commom.Log.WriteLog("   " + "向测试用户liwei & derek 发送了通知邮件[OperId:" + operid.ToString() + "]（只记log，实际不发送）");
                        em.SendExchangeMail(tos, subject, content);
                    }
                }
                else
                {
                    em.SendExchangeMail(ifs[i].INFRM_EMAILTO, subject, content, Aid.Null2Str(ifs[i].INFRM_EMAILBCC));
                    Commom.Log.WriteLog("   " + "向" + ifs[i].INFRM_EMAILTO + "发送了通知邮件[OperId:" + operid.ToString() + "]");
                    EOperInformModel e = new EOperInformModel();
                    e.Infrm_Id = ifs[i].INFRM_ID;
                    e.Infrm_EmailStatus = "S";
                    e.Infrm_EmailDate = DateTime.Now;
                    int cnt = this.Update(e);
                }
            }
            Commom.Log.WriteLog("--  end  -- ");
        }

        /// <summary>
        /// 发送邮件(notice and eform)
        /// </summary>
        /// <param name="type">业务类型</param>
        public void Send(string type, string oper, DateTime operDate,bool skipCheck=false)
        {
            if (string.IsNullOrEmpty(Helper.Aid.yPath)) //如果“物理根目录”静态变量为空，则不执行发送任务。            
            {
                Commom.Log.WriteLog("yPath is required");
                return;
            }

            try
            {
                lock (this)
                {
                    Helper.Aid.Status4EFM_NTC = "1"; //执行中...    
                }

                if (skipCheck) // 如果跳过检查（测试需要）
                {
                    Log.WriteLog("测试" + type + "业务");
                }
                else
                {
                    if (!Check(type, System.Configuration.ConfigurationManager.AppSettings["dailySchedule4Ntc_Efm"]))
                    {
                        return;
                    }                    
                }                
                string toggle = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["dailyScheduleToggle4Ntc_Efm"]);

                /// 
                /// 1.获取数据
                /// 2.内容组装
                /// 3.执行发送
                string subject = "";
                string content = "";
                if (type == "EAgent_BatchInformStaff_NTC_EFM")
                {
                    List<InFrmQuery> etys = this.GetBatchEmailNoticeEForm(oper, operDate);
                    if (etys.Count <= 0)
                        throw new Exception("没有数据存在，跳过发送。");
                    
                    var gp = etys.GroupBy(x => x.INFRM_EMAILTO).ToList();
                    foreach (var item in gp) //遍历收件人分组
                    {
                        System.Xml.XmlNode node = root.SelectSingleNode("//template[@name='" + type + "']");//通过xquery查找指定的模板。
                        System.Xml.XmlNode efmTr = null, ntcTr = null;
                        subject = node.SelectSingleNode("subject").InnerText.Trim();
                        content = node.SelectSingleNode("content/header").InnerText;
                        content = content.Replace("{sysdate}", Aid.Date2Str(operDate, Aid.DateFmt.Standard));

                        string content_table_efm = "";
                        string content_table_ntc = "";                       
                        foreach (var ety in item) //遍历内容
                        {
                            ety.INFRM_EMAILSTATUS = "";
                            
                            // 判断是哪个模板（这里有优化空间，建议改成一个通用函数。eg. GenSingleRow(nodes,content) ）
                            if (ety.INFRM_REFTYPE.IndexOf("EAGBIS_EFM") >= 0) // 如果是EFM类型，这个是固定值
                            {
                                efmTr = node.SelectSingleNode("content//table[@type='EFM']//thead//tr");
                                InfFrmContentEFM contentEfm = Newtonsoft.Json.JsonConvert.DeserializeObject<InfFrmContentEFM>(ety.INFRM_CONTENT);
                                content_table_efm += "<tr>";
                                foreach (System.Xml.XmlNode n in efmTr.ChildNodes)
                                {
                                    if (n.InnerText == "Form No")
                                    {
                                        string form_no = contentEfm.GetType().GetProperty(n.Attributes["fieldname"].Value).GetValue(contentEfm, null).ToString();
                                        string a = string.Format("<a href='{0}'>{1}</a>", Aid.vPath + "system/eoper.aspx?sp=" + form_no, form_no);
                                        content_table_efm += "<td>" + a + "</td>";
                                    }
                                    else
                                    {
                                        content_table_efm += "<td>" + contentEfm.GetType().GetProperty(n.Attributes["fieldname"].Value).GetValue(contentEfm, null).ToString() + "</td>";
                                    }

                                }
                                content_table_efm += "</tr>";
                            }
                            else if (ety.INFRM_REFTYPE.IndexOf("EAGBIS_NTC") >= 0)// 如果是NTC类型，这个是固定值
                            {
                                ntcTr = node.SelectSingleNode("content//table[@type='NTC']//thead//tr");
                                InfFrmContentNTC contentNtc = Newtonsoft.Json.JsonConvert.DeserializeObject<InfFrmContentNTC>(ety.INFRM_CONTENT);
                                content_table_ntc += "<tr>";
                                foreach (System.Xml.XmlNode n in ntcTr.ChildNodes)
                                {
                                    if (n.InnerText == "Subject")
                                    {
                                        string subject_ntc = contentNtc.GetType().GetProperty(n.Attributes["fieldname"].Value).GetValue(contentNtc, null).ToString();
                                        string a = string.Format("<a href='{0}'>{1}</a>", Aid.vPath + "system/NoticeMaintenance.aspx?sp=" + subject_ntc, subject_ntc);
                                        content_table_ntc += "<td>" + a + "</td>";
                                    }
                                    else
                                    {
                                        content_table_ntc += "<td>" + contentNtc.GetType().GetProperty(n.Attributes["fieldname"].Value).GetValue(contentNtc, null).ToString() + "</td>";
                                    }
                                }
                                content_table_ntc += "</tr>";
                            }

                            // 更改状态
                            EOperInformModel eoi = new EOperInformModel();
                            eoi.Infrm_Id = ety.INFRM_ID;
                            eoi.Infrm_EmailStatus = "S";
                            eoi.Infrm_EmailDate = DateTime.Now;
                            int c = Update(eoi);
                        }
                        // 先处理 ntc
                        if (content_table_ntc.Length > 0)
                        {
                            content += "<table>";
                            content += "<caption style='text-align: left;'>";
                            content += node.SelectSingleNode("content//table[@type='NTC']//thead//caption").InnerText;
                            content += "</caption>";
                            content += "<thead>";
                            content += "<tr>";
                            foreach (System.Xml.XmlNode n in ntcTr.ChildNodes)
                            {
                                content += "<th>" + n.InnerText + "</th>";
                            }
                            content += "</tr>";
                            content += "</thead>";
                            content += "<tbody>";
                            content += content_table_ntc;
                            content += "</tbody>";
                            content += "</table>";
                        }
                        if (content_table_efm.Length > 0)
                        {
                            content += "<table>";
                            content += "<caption style='text-align: left;'>";
                            content += node.SelectSingleNode("content//table[@type='EFM']//thead//caption").InnerText;
                            content += "</caption>";
                            content += "<thead>";
                            content += "<tr>";
                            foreach (System.Xml.XmlNode n in efmTr.ChildNodes)
                            {
                                content += "<th>" + n.InnerText + "</th>";
                            }
                            content += "</tr>";
                            content += "</thead>";
                            content += "<tbody>";
                            content += content_table_efm;
                            content += "</tbody>";
                            content += "</table>";
                        }
                        content = "<style>table{border-collapse:collapse;} table, th, td{border: 1px solid black;padding:4px; text-align: left;}"
                            + "body { font-size: 1rem;font-family: Candara;}"
                            + "</style>"
                            + content;
                        content += node.SelectSingleNode("content//footer").InnerText;
                        Commom.Log.WriteLog(item.Key + "\r\n" + content);

                        // 执行发送                       
                        ExchangeMail em = new ExchangeMail();
                        if (toggle == "1")
                        {
                            // dailyScheduleToggle4Ntc_Efm 配置为1时发送出去                            
                            em.SendExchangeMail(item.Key, subject, content);
                        }
                        else
                        {
                            // 测试环境发送邮件
                            string tos = "liwei@hwpg.com";
                            em.SendExchangeMail(tos, subject, content);
                            em.SendExchangeMail("derekyu@hwpg.com", subject, content);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Commom.Log.WriteLog("ERROR:" + type + "\r\n" + err.Message + "\r\n" + err.StackTrace);
            }
            finally
            {
                lock (this)
                {
                    Helper.Aid.Status4EFM_NTC = "0"; //执行完成    
                }     
            }         
        }

        public bool Check(string type,string schedule)
        {
            if (string.IsNullOrEmpty(schedule))
            {
                Commom.Log.WriteLog("[" + type + "@web.config]未配置schedule");
                return false;
            }

            DateTime now = DateTime.Now;            
            bool needRun = false;
            // 检查是否数据库里有对应的任务
            List<TaskModel> tasks = this.Retrieve<TaskModel>(x => x.Name == type);
            if (tasks.Count > 0) //若有则判断是否需要执行
            {               
                if (tasks[0].Last_Run != null)
                {
                    //schedule时间
                    DateTime stime = DateTime.Parse(Aid.Date2Str(now, Aid.DateFmt.Standard) + " " + schedule); 
                    // 如果当前时间与计划时间间隔不超过3分钟并且比上一次的运行时间大于23小时（daily情况）
                    if (Math.Abs(now.Subtract(stime).TotalMinutes) <= 3 && now.Subtract(tasks[0].Last_Run.Value).TotalHours >= 23)
                    {
                        //Commom.Log.WriteLog("Schedule diff:" + Math.Abs(now.Subtract(stime).TotalMinutes).ToString() + ";Last Run:" + Aid.Date2Str(tasks[0].Last_Run.Value, Aid.DateFmt.Wholly));
                        needRun = true;
                    }
                    else
                    {                       
                        needRun = false;
                    }                    
                }
                else
                {
                    needRun = true;
                }
            }
            else //否则创建任务信息，并执行一次
            {
                TaskModel ety = new TaskModel();
                ety.Id = 12;
                ety.Name = type;
                ety.Description = "每晚定时运行并发送邮件('EAgent_BatchInformStaff_NTC_EFM'业务)";
                ety.Start_Date = DateTime.Now;
                ety.Last_Run = null;
                this.Create(ety);
                needRun = false;
            }

            if (needRun)
            {
                tasks[0].Last_Run = now;
                Update(tasks[0]);
            }
            return needRun;
        }

        /// <summary>
        /// 组装内容
        /// </summary>
        /// <param name="type"></param>
        /// <param name="node"></param>
        /// <param name="ety"></param>
        /// 这里执行xml里的字段替换，这些替换占位符的设计需要优化，最好定义成变量列表的形式，而不是直接放在内容中。
        private void Rig(string type, System.Xml.XmlNode node, InFrmQuery ety)
        {
            System.Xml.XmlNode subjectNode = null; ;
            System.Xml.XmlNode contentNode = null;

            subjectNode = node.SelectSingleNode("subject");
            contentNode = node.SelectSingleNode("content");

            EformMailSubject s = Newtonsoft.Json.JsonConvert.DeserializeObject<EformMailSubject>(ety.INFRM_SUBJECT);
            EformMailContent c = Newtonsoft.Json.JsonConvert.DeserializeObject<EformMailContent>(ety.INFRM_CONTENT);

            subject = subjectNode.InnerText.Replace("{Form_Name}", s.Form_Name).Trim();
            subject = subject.Replace("{Ref_No}", s.Ref_No);

            content = contentNode.InnerText.Replace("{Form_Name}", c.Form_Name).Trim();
            content = content.Replace("{Ref_No}", c.Ref_No);

            if (type == "EForm_ReplyTenant")
            {
                //{Inform_EN} {phone1}  {email}  {EAgent_Company_EN}  {EAgent_Company_CN}  {Inform_CN}
                content = content.Replace("{Inform_EN}", Aid.Null2Str(c.Inform_EN));
                content = content.Replace("{EAgent_Company_EN}", contentNode.Attributes["EAgent_Company_En"].Value);
                content = content.Replace("{EAgent_Company_CN}", contentNode.Attributes["EAgent_Company_CN"].Value);
                content = content.Replace("{phone1}", contentNode.Attributes["phone1"].Value);
                content = content.Replace("{email}", contentNode.Attributes["email"].Value);
                content = content.Replace("{Inform_CN}", Aid.Null2Str(c.Inform_CN));

                /// -- start -- http://172.21.111.108:836/Form/Add?formId=378&fmtbId=130000&flowId=71&operId=119
                //this.WhereClauses.Add("UserId", new Evaluation(SqlOperator.EQ, ety.INFRM_USERID));
                List<Model.Sys_Login_Account> accs = this.Retrieve<Model.Sys_Login_Account>(x=>x.UserId==ety.INFRM_USERID);

                string str = "{\"username\":" + "\"" + accs[0].LoginName + "\",";
                str += "\"password\":\"" + accs[0].Password + "\",";
                str += "\"nonce\":" + "\"" + System.Guid.NewGuid().ToString().Substring(0, 7) + "\",";
                str += "\"expiredTime\":" + "1800" + ",";
                str += "\"lang\":\"en-US\",";
                str += "\"clienttype\":\"B\"";
                str += "}";
                string encryptStr = new EncryptUtil().Encrypt(str);
                string qs_en = "&txt=" + encryptStr;

                string link = Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["TenantMailUrl"]);
                link += string.Format("/form/add?formId={0}&fmtbId={1}&flowId={2}&operId={3}", c.Form_Id, c.FmTb_Id, c.Flow_Id, operid.ToString()); //<add key="TenantMailUrl" value="http://202.82.86.38:836"/>
                link += qs_en;
                content = content.Replace("{link}", "<a href='" + link + "'>" + "link</a>");
                /// --  end  --
            }
            else if (type == "EForm_InformStaff")
            {
                //Pre_RoleGrp Cur_RoleGrp
                subject = subject.Replace("{Inform_EN}", Aid.Null2Str(s.Inform_EN));

                content = content.Replace("{Inform_EN}", Aid.Null2Str(c.Inform_EN));
                content = content.Replace("{Inform_CN}", Aid.Null2Str(c.Inform_CN));
                content = content.Replace("{Lease_Number}", Aid.Null2Str(c.Lease_Number));
                content = content.Replace("{Premise_Name}", Aid.Null2Str(c.Premise_Name));
                content = content.Replace("{Front_User}", Aid.Null2Str(c.Front_User));
                content = content.Replace("{Pre_RoleGrp}", Aid.Null2Str(c.Pre_RoleGrp));
                content = content.Replace("{Cur_RoleGrp}", Aid.Null2Str(c.Cur_RoleGrp));
                content = content.Replace("{link}", "<a href='" + Aid.vPath + "system/eOper.aspx?id=" + this.operid.ToString() + "'>link</a>");
            }
        }

        /// <summary>
        /// 获取“执行流”待发送的内容数据
        /// </summary>
        /// <param name="operid"></param>
        /// <param name="optype"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        private List<InFrmQuery> GetFlowEmail(int operid, string optype, string loginname, string groupname)
        {
            try
            {
                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Operation", optype));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
                List<InFrmQuery> etys = this.Retrieve9iSp<InFrmQuery>("pkg_eform_flow.Get_Flow_Email", paras.ToArray());
                /// --  end  --               
                return etys;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private List<InFrmQuery> GetBatchEmailNoticeEForm(string operation, DateTime operDate)
        {
            try
            {
                /// -- start --  使用“derek”提供的存储过程
                List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Operation", operation));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperDate", operDate));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", "sysadmin"));
                paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", "Admin"));
                paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
                List<InFrmQuery> etys = this.Retrieve9iSp<InFrmQuery>("pkg_eform_flow.Get_BatchEmail_NoticeEForm", paras.ToArray());
                /// --  end  --               
                return etys;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }

    public class EformBiz50 : DbConsole
    {
        public EformBiz50(DbTx tx) : base(tx) { }

        public List<EFormModel> GetEformDef(int formid)
        {
            if (formid > 0)
            {
                List<EFormModel> etys = this.Retrieve<EFormModel>(x => x.FORM_ID == formid);
                return etys;
            }
            else
            {
                return new List<EFormModel>();
            }
        }

        /// <summary>
        /// 根据P_OperId和P_FormId进行表单信息获取
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="fmtbid"></param>
        /// <param name="operid"></param>
        /// <param name="loginname"></param>
        /// <param name="refno"></param>
        /// <param name="leasenumber"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        /// 前者（Oper_Id）为空，则返回对应的Form_Id的空表单信息，有值则返回用户之前提交的表单信息
        /// -- select pkg_eform_flow.Get_EForm_Data(328, 110000, 0, '', '', '') from dual;
        /// Function Get_EForm_Data(
        ///     Pm_FormId      In Number,
        ///     Pm_FmTbId      In Number,
        ///     Pm_OperId      In Number,
        ///     Pm_OperRefNo   In Varchar2,
        ///     Pm_LeaseNumber In Varchar2,
        ///     Pm_LoginName   In Varchar2) Return Sys_RefCursor;
        public List<EFormDataFieldQuery> GetEFormData(int formid, int fmtbid, int operid, string loginname, string refno, string leasenumber, string groupname = "")
        {
            /// -- start --  使用“derek”提供的存储过程
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FormId", formid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FmTbId", fmtbid)); //这里的fmtbid可以为0
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperRefNo", refno));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LeaseNumber", leasenumber));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EFormDataFieldQuery> etys = this.Retrieve9iSp<EFormDataFieldQuery>("pkg_eform_flow.Get_EForm_Data", paras.ToArray());
            /// --  end  --           
            return etys;
        }

        /// <summary>
        /// 后台用户获取表单之前流转信息
        /// </summary>
        /// <returns></returns>
        /// 后台用户查询当前流转步骤之前的信息
        /// --select pkg_eform_flow.Get_EFlow_Info('admin'，'Admin User Group', 18) from dual;
        ///     Function Get_EFlow_Info(
        ///         Pm_LoginName In Varchar2,
        ///         Pm_GroupName In Varchar2,
        ///         Pm_OperId    In Number,
        ///         Pm_FormId    In Number         -- 当Pm_OperId为零，可通过Pm_FormId获取Flow_Id以在定义FORM界面可预览表格内容和空审批流程
        /// ) Return Sys_RefCursor;
        public List<EFlowInfoQuery> GetEflowInfo(int operid, string loginname, string groupname, int formid)
        {
            /// -- start --  使用“derek”提供的存储过程
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FormId", formid));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EFlowInfoQuery> etys = this.Retrieve9iSp<EFlowInfoQuery>("pkg_eform_flow.Get_EFlow_Info", paras.ToArray());

            return etys;
            /// --  end  --           
        }

        /// <summary>
        /// 用户获取表单的可操作信息
        /// </summary>
        /// <param name="refno"></param>
        /// <param name="operid"></param>
        /// <param name="fwdeseq"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        /// --用户查询EForm的可操作步骤和当前状态
        ///  --select pkg_eform_flow.Get_EForm_Operation('', 18, 0, 'admin'，'Admin User Group') from dual;
        ///  Function Get_EForm_Operation(
        ///     Pm_RefNo     In Varchar2,
        ///     Pm_OperId    In Number,
        ///     Pm_FwDeSeq   In Number,
        ///     Pm_LoginName In Varchar2,
        ///     Pm_GroupName In Varchar2) Return Sys_RefCursor;
        public List<EFormOperationQuery> GetEformOperation(string refno, int operid, int fwdeseq, string loginname, string groupname)
        {
            /// -- start --  使用“derek”提供的存储过程
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_RefNo", refno));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FwDeSeq", fwdeseq));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EFormOperationQuery> etys = this.Retrieve9iSp<EFormOperationQuery>("pkg_eform_flow.Get_EForm_Operation", paras.ToArray());
            /// --  end  --                
            return etys;
        }

        /// <summary>
        /// 获取子表数据
        /// </summary>
        /// <param name="operid"></param>
        /// <param name="formid"></param>
        /// <param name="fmtbid"></param>
        /// <param name="refno"></param>
        /// <param name="leasenumber"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public EformSubs GetEformSubData(int operid, int formid, int fmtbid, string refno, string leasenumber, string loginname, string groupname)
        {
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EformSubFmdtIds> etys = this.Retrieve9iSp<EformSubFmdtIds>("pkg_eform_flow.Get_EForm_SubList", paras.ToArray());

            EformSubs subs = new EformSubs();
            for (int i = 0; i < etys.Count; i++)
            {
                EformSubRow row = new EformSubRow();
                List<System.Data.OracleClient.OracleParameter> paras2 = new List<System.Data.OracleClient.OracleParameter>();
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_FormId", formid));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_FmTbId", fmtbid));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_FmDtId", etys[i].fmdt_id));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_OperRefNo", refno));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_LeaseNumber", leasenumber));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
                paras2.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
                paras2.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
                row.cols = this.Retrieve9iSp<EFormDataFieldQuery>("pkg_eform_flow.Get_EForm_SubData", paras2.ToArray());
                subs.rows.Add(row);
            }
            return subs;
        }

        /// <summary>
        /// 获取子表结构(字段列表)
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="fmtbid"></param>
        /// <param name="operid"></param>
        /// <param name="fmdtid"></param>
        /// <param name="refno"></param>
        /// <param name="leasenumber"></param>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public List<EFormDataFieldQuery> GetEFormSubFields(int formid, int fmtbid, int operid, int fmdtid, string refno, string leasenumber, string loginname, string groupname)
        {
            /// -- start --  使用“derek”提供的存储过程
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FormId", formid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FmTbId", fmtbid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperId", operid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FmDtId", fmdtid));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_OperRefNo", refno));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LeaseNumber", leasenumber));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EFormDataFieldQuery> etys = this.Retrieve9iSp<EFormDataFieldQuery>("pkg_eform_flow.Get_EForm_SubData", paras.ToArray());
            return etys;
            /// --  end  -- 
        }

        public List<EformTemplateVQuery> GetEformTemplateV()
        {
            List<EformTemplateVQuery> etys = this.Retrieve<EformTemplateVQuery>(x => true);
            return etys;
        }

        /// <summary>
        /// 后台用户获取流信息
        /// </summary>
        /// <param name="loginname"></param>
        /// <param name="groupname"></param>
        /// <param name="propertylist"></param>
        /// <param name="status"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public List<EFLowModelQuery> GetEFLowByBackUser(string loginname, string groupname, string propertylist, string status, string search)
        {
            /// -- start --  使用“derek”提供的存储过程
            List<System.Data.OracleClient.OracleParameter> paras = new List<System.Data.OracleClient.OracleParameter>();
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_LoginName", loginname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_GroupName", groupname));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_PropertyList", propertylist));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_FlowStatus", status));
            paras.Add(new System.Data.OracleClient.OracleParameter("Pm_Search", search));
            paras.Add(Oracle9i.BuildParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, System.Data.ParameterDirection.ReturnValue));
            List<EFLowModelQuery> etys = this.Retrieve9iSp<EFLowModelQuery>("pkg_eform_flow.Get_EFLow_By_BackUser", paras.ToArray());
            /// --  end  --          
            return etys;
        }

        /// <summary>
        /// 获取指定operid表单的pdf文件
        /// </summary>
        /// <param name="operid">表单操作id</param>
        /// <param name="src">前台或后台调用。true：前台；false：后台，默认是前台</param>
        /// <returns>pdf文件的base64形式</returns>
        /// alter table e_operation add Pdf_Url varchar(200); 
        public JsonRsp GetStaticHtml2Pdf(int operid, bool src = true)
        {
            Log.WriteLog("---- start ---- html2pdf");
            JsonRsp rsp = new JsonRsp();
            try
            {
                EformLoadQuery dom = new EformLoadQuery();
                List<EOperationModel> etys = this.Retrieve<system.EOperationModel>(x => x.Oper_Id == operid);
                if (etys.Count <= 0) throw new Exception("[oper id:" + operid + "] not found");

                //load                
                dom.fields = this.GetEFormData(etys[0].Form_Id, etys[0].Fmtb_Id, etys[0].Oper_Id, etys[0].Oper_CreateBy, Aid.Null2Str(etys[0].Oper_Refno), etys[0].Oper_Lease, "FrontUser");
                dom.defs = this.GetEformDef(etys[0].Form_Id);
                dom.flowinfo = this.GetEflowInfo(etys[0].Oper_Id, etys[0].Oper_CreateBy, "FrontUser", etys[0].Form_Id);
                dom.privilege = this.GetEformOperation(Aid.Null2Str(etys[0].Oper_Refno), etys[0].Oper_Id, 0, etys[0].Oper_CreateBy, "FrontUser");
                //如果有子表
                for (int i = 0; i < dom.fields.Count; i++)
                {
                    if (dom.fields[i].field_code == "Sub_Table")
                    {
                        dom.subctrls = this.GetEFormSubFields(etys[0].Form_Id, etys[0].Fmtb_Id, operid, 0, etys[0].Oper_Refno, etys[0].Oper_Lease, etys[0].Oper_CreateBy, "FrontUser");
                        dom.subdata = this.GetEformSubData(operid, etys[0].Form_Id, etys[0].Fmtb_Id, etys[0].Oper_Refno, etys[0].Oper_Lease, etys[0].Oper_CreateBy, "FrontUser");
                        break;
                    }
                }
                Log.WriteLog("step 1 -> load form data for " + dom.defs[0].FMTB_CODE + "_" + etys[0].Oper_Id.ToString());

                //1.获取指定fmtbid的html文件并读取
                //2.用dom数据替换html文件的占位符
                //3.输出静态html文件，前台调用时不保存生成的文件，后台需要保存
                //4.保存
                System.IO.StreamReader sr = null;
                string ts = Aid.Null2Str(Aid.getTimestamp() / 1000);            //当前时间戳
                string code = dom.defs[0].FMTB_CODE;                            //Form_A
                string staticHtmlFile = code + "_static.html";                  //Form_A_static.html
                string staticHtmlFilePath = Aid.yPath + @"system\tpls4form\" + staticHtmlFile; // ~\eAgentsAdmin\system\tpls4form\Form_A_static.html
                if (!System.IO.File.Exists(staticHtmlFilePath)) throw new Exception("Static html file of " + dom.defs[0].FMTB_CODE + " is not exists.");

                System.IO.Stream s = new System.IO.FileStream(staticHtmlFilePath, System.IO.FileMode.Open);
                System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                sr = new System.IO.StreamReader(s, encode);
                string html = sr.ReadToEnd();
                sr.Close();

                // 控件内容替换
                for (int i = 0; i < dom.fields.Count; i++)
                {
                    html = html.Replace("{" + dom.fields[i].field_code.Trim() + ".field_dispen}", dom.fields[i].field_dispen);
                    html = html.Replace("{" + dom.fields[i].field_code.Trim() + ".field_dispcn}", dom.fields[i].field_dispcn);
                    if (dom.fields[i].field_type.Contains("Chk") || dom.fields[i].field_type.Contains("Rad"))
                    {
                        html = html.Replace("{" + dom.fields[i].field_code + ".fmdt_value}", dom.fields[i].fmdt_value == "Y" ? "checked='checked'" : "");
                    }
                    else
                    {
                        html = html.Replace("{" + dom.fields[i].field_code + ".fmdt_value}", dom.fields[i].fmdt_value);
                    }
                }
                // 子表控件内容替换
                for (int j = 0; j < dom.subctrls.Count; j++)
                {
                    html = html.Replace("{" + dom.subctrls[j].field_code + ".field_dispen}", dom.subctrls[j].field_dispen);
                    html = html.Replace("{" + dom.subctrls[j].field_code + ".field_dispcn}", dom.subctrls[j].field_dispcn);
                }

                // define内容替换(html,description)
                html = html.Replace("{FORM_HEADER_HTML}", dom.defs[0].FORM_HEADER_HTML);//FORM_HEADER_HTML
                html = html.Replace("{FORM_WAIST_HTML}", dom.defs[0].FORM_WAIST_HTML);
                html = html.Replace("{FORM_PICTURE_Base64}", "data:image/jpg;base64," + dom.defs[0].FORM_PICTURE_Base64);
                html = html.Replace("{FORM_FOOTER_HTML}", dom.defs[0].FORM_FOOTER_HTML);//FORM_HEADER_HTML
                html = html.Replace("{FORM_DESC}", dom.defs[0].FORM_DESC);
                html = html.Replace("{FORM_NAME}", dom.defs[0].FORM_NAME);

                //其他替换(dom数据，用于静态js处理)
                html = html.Replace("var dom;", "var dom=" + Newtonsoft.Json.JsonConvert.SerializeObject(dom));

                //保存html文件
                string staticHtmlFileForSave = ts + "_" + staticHtmlFile; //1655842169_Form_A_static.html;                 
                string staticHtmlFilePathForSave = Aid.yPath + @"system\tpls4form\" + staticHtmlFileForSave; //新html文件的物理路径
                System.IO.File.AppendAllText(staticHtmlFilePathForSave, html, System.Text.Encoding.UTF8); //执行保存

                Log.WriteLog("step 2 -> replace content and save static html.");
                //输出pdf文件
                string exePath = Aid.yPath + @"bin\wkhtmltopdf.exe";
                string pageUrl = Aid.vPath + "system/tpls4form/" + staticHtmlFileForSave; //新html文件的虚拟路径
                string pdfFile = code + "_" + etys[0].Oper_Id.ToString() + (src ? "_" + ts : "") + ".pdf";//前台的文件名含时间戳
                string pdfFilePath = Aid.yPath + @"upload\" + pdfFile;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(exePath, pageUrl + " " + pdfFilePath); //如果文件存在则覆盖。
                System.Threading.Thread.Sleep(1500);
                Log.WriteLog("step 3 -> save *.pdf with wkhtmltopdf tool.");

                //读取pdf转为base64格式
                if (!System.IO.File.Exists(pdfFilePath))
                {
                    Log.WriteLog("step 4 -> pdf file are not ready for wkhtmltopdf failed.");
                    throw new Exception("pdf file are not ready ,please retry.");
                }
                else
                {
                    System.IO.FileStream fs = new System.IO.FileStream(pdfFilePath, System.IO.FileMode.Open);
                    byte[] buff = new byte[fs.Length];
                    fs.Read(buff, 0, buff.Length);
                    string b64 = Convert.ToBase64String(buff);
                    fs.Close();

                    PdfRsp pdf = new PdfRsp();
                    pdf.base64 = b64;
                    pdf.vPath = Aid.vPath + "upload/" + pdfFile;
                    rsp.result = pdf;
                    Log.WriteLog("step 4 -> convert pdf file to base64 string.");

                    if (src) { System.IO.File.Delete(pdfFilePath); }//如果是前台调用
                    System.IO.File.Delete(staticHtmlFilePathForSave);
                    Log.WriteLog("step 5 -> invoke completed.");

                }
            }
            catch (Exception err)
            {
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
            Log.WriteLog("----  end  ----");
            return rsp;
        }
    }

    #endregion

    #region 实体类

    public class InfFrmContentNTC
    {
        public string Property_Code { get; set; }
        public string Notice_Subject { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public string Create_By { get; set; }
        public int Notice_Id { get; set; }

    }

    public class InfFrmContentEFM
    {
        public string Form_Name { get; set; }
        public string Form_No { get; set; }
        public string Form_Date { get; set; }
        public string Form_LeaseNumber { get; set; }
        public string Premise_Name { get; set; }
        public string Pre_Group { get; set; }
        public string Cur_Group { get; set; }
        public string Form_Action { get; set; }
        public int Oper_Id { get; set; }
    }

    public class PdfRsp
    {
        public string base64 { get; set; }
        public string vPath { get; set; }
    }

    public class EformMailSubject
    {
        public string Form_Name { get; set; }
        public string Ref_No { get; set; }
        public string Inform_EN { get; set; }
    }

    public class EformMailContent
    {
        /// <summary>
        /// 
        /// </summary>
        public string Form_Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ref_No { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Inform_EN { get; set; }
        /// <summary>
        /// 审批
        /// </summary>
        public string Inform_CN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Lease_Number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Premise_Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Front_User { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pre_RoleGrp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Cur_RoleGrp { get; set; }

        public string Form_Id { get; set; }
        public string FmTb_Id { get; set; }
        public string Flow_Id { get; set; }
    }

    public class InFrmQuery
    {             
        public int INFRM_ID { get; set; }
        public string INFRM_REFTYPE { get; set; }
        public int INFRM_REFID { get; set; }
        public string INFRM_REFNO { get; set; }
        public string INFRM_REFLEASE { get; set; }
        public int INFRM_BATCHID { get; set; }
        public string INFRM_BATCHCODE { get; set; }
        public string INFRM_SUBJECT { get; set; }
        public string INFRM_TEMPLATE { get; set; }
        public string INFRM_CONTENT { get; set; }
        public int INFRM_USERID { get; set; }
        public string INFRM_LOGINNAME { get; set; }
        public string INFRM_USERGRP { get; set; }
        public string INFRM_EMAILTO { get; set; }
        public string INFRM_EMAILBCC { get; set; }
        public string INFRM_EMAILSTATUS { get; set; }
        public string INFRM_DEVICEID { get; set; }
        public string INFRM_APPREGID { get; set; }
        public string INFRM_APPSTATUS { get; set; }
        public string INFRM_WXOPENID { get; set; }
        public string INFRM_WXSTATUS { get; set; }
        public string INFRM_CREATEBY { get; set; }
        public DateTime INFRM_CREATEDATE { get; set; }

        /*
         * public const string TABLE_NAME = "E_OperInform";
         *  
         * INFRM_ID	9	
            INFRM_REFTYPE	EFM	
            INFRM_REFID	102	
            INFRM_REFNO	Form_A_102	
            INFRM_REFLEASE	WG0200388	
            INFRM_BATCHID	3	
            INFRM_BATCHCODE	102_10_P_Submit_kin_EM WG02 Group	
            INFRM_SUBJECT	{"Form_Name":"Form A - EXTENDED AIR-CONDITIONING SUPPLY","Ref_No":"Form_A_102"}	
            INFRM_TEMPLATE	EForm_InformStaff	
            INFRM_CONTENT	{"Form_Name":"Form A - EXTENDED AIR-CONDITIONING SUPPLY","Ref_No":"Form_A_102","Inform_EN":"Approval","Inform_CN":"审批","Lease_Number":"WG0200388","Premise_Name":"SHOP NOS. G1-G2 & G5, G/F & B5-B7, MTR FLOOR (1/B) WHAMPOA GARDEN - SITE 2","Front_User":"WG0200388M09_123","Pre_RoleGrp":"","Cur_RoleGrp":"EM WG02 Group"}	
            INFRM_USERID	198	
            INFRM_LOGINNAME	alee	
            INFRM_USERGRP	EM WG02 Group	
            INFRM_EMAILTO	alicelee@hpgl.com	
            INFRM_EMAILBCC		
            INFRM_EMAILSTATUS	W	
            INFRM_DEVICEID		
            INFRM_APPREGID	191e35f7e0d35c39bc1	
            INFRM_APPSTATUS	W	
            INFRM_WXOPENID		
            INFRM_WXSTATUS	W	
            INFRM_CREATEBY	kin	
            INFRM_CREATEDATE	2021/5/10 10:24:28	

         */
    }

    public class EOperInformModel
    {
        public const string TABLE_NAME = "E_OperInform";

        [DbField(true)]
        public int Infrm_Id { get; set; }
        public string Infrm_EmailStatus { get; set; }
        public Nullable<DateTime> Infrm_UpdateDate { get; set; }
        public Nullable<DateTime> Infrm_EmailDate { get; set; }
    }

    public class EformTemplateVQuery
    {
        public const string TABLE_NAME = "sys_eform_tempalte_v";

        public int fmtb_id { get; set; }
        public string fmtb_code { get; set; }
        public string form_template { get; set; }
        public string template_filename { get; set; }
    }

    public class LoginProfile
    {
        public Model.Sys_Login_System login { get; set; }   //当前登录用户
        public Model.Sys_Users_Group grpinfo { get; set; }  //当前登录用户的组信息
        public List<Model.Sys_Users_Group> grps = new List<Model.Sys_Users_Group>(); //当前登录用户所属的所有组信息
        public List<Model.Sys_Users_Group_Property> props = new List<Model.Sys_Users_Group_Property>(); //当前登录用户可访问的property list
    }

    public class EFormDataFieldQuery
    {
        public int oper_id { get; set; }
        public int field_id { get; set; }
        public string fmdt_value { get; set; }
        public int fmtb_id { get; set; }
        public string field_code { get; set; }
        public string field_type { get; set; }
        public string field_name { get; set; }
        public string field_dispen { get; set; }
        public string field_dispcn { get; set; }
        public string field_disptc { get; set; }
        public string field_control { get; set; }
        public int field_editseq { get; set; }
        public int fmdt_id { get; set; }
        public string field_editctrl { get; set; }
        public string field_valuesql { get; set; }

    }

    public class EFormDataModel
    {
        public const string TABLE_NAME = "E_FormData";

        [DbField(true)]
        public int Oper_Id { get; set; }
        [DbField(true)]
        public int Field_Id { get; set; }
        [DbField(true)]
        public int Fmtb_Id { get; set; }
        [DbField(true)]
        public int Fmdt_Id { get; set; }

        public string Fmdt_Value { get; set; }
        public string Fmdt_UpdateBy { get; set; }
        public Nullable<DateTime> Fmdt_UpdateDate { get; set; }
    }

    public class EFlowInfoQuery
    {
        public int form_id { get; set; }
        public string form_code { get; set; }
        public string form_name { get; set; }
        public int oper_id { get; set; }
        public string flow_id { get; set; }
        public string fmtb_id { get; set; }
        public string oper_property { get; set; }   //WG02
        public string oper_lease { get; set; }      //WG0200388
        public int oper_curseq { get; set; }
        public string oper_createby { get; set; }
        public Nullable<DateTime> oper_createdate { get; set; }
        public string oper_status { get; set; }
        public int fwde_id { get; set; }
        public int fwde_seq { get; set; }
        public string fwde_code { get; set; }
        public string fwde_name { get; set; }
        public int fwde_finalseq { get; set; }
        public int fwde_groupid { get; set; }
        public string fwde_group { get; set; }
        public string fwde_email { get; set; }
        public int fwde_nextseq { get; set; }
        public int fwde_preseq { get; set; }
        public string fwde_where { get; set; }
        public string fmfw_remarks { get; set; }
        public string fmfw_attachurl { get; set; }
        public string fmfw_attachname { get; set; }
        public string fmfw_createby { get; set; }
        public Nullable<DateTime> fmfw_createdate { get; set; }
        public string fmfw_updateby { get; set; }
        public Nullable<DateTime> fmfw_updatedate { get; set; }
        public string fmfw_rejectreason { get; set; }
        public string fmfw_rejectby { get; set; }
        public Nullable<DateTime> fmfw_rejectdate { get; set; }
        public string fmfw_status { get; set; }
    }

    public class EFormOperationQuery
    {
        public string oper_refno { get; set; }
        public int oper_id { get; set; }
        public string oper_status { get; set; }
        public int oper_curseq { get; set; }
        public int fwde_finalseq { get; set; }
        public string fmfw_sign { get; set; }
        public int fmfw_seq { get; set; }
        public string fwde_code { get; set; }
        public string fwde_name { get; set; }
        public int fwde_groupid { get; set; }
        public string fwde_group { get; set; }
        public string fwde_email { get; set; }
        public string fmfw_status { get; set; }
        public string fmfw_remarks { get; set; }
        public string fmfw_updateby { get; set; }
        public Nullable<DateTime> fmfw_updatedate { get; set; }
        public string oper_view { get; set; }
        public string oper_save { get; set; }
        public string oper_submit { get; set; }
        public string oper_cancel { get; set; }
        public string oper_withdraw { get; set; }
        public string oper_reject { get; set; }
        public string oper_finish { get; set; }
    }

    /// <summary>
    /// EformFlow实体
    /// </summary>
    public class EFormFlowModel
    {
        public const string TABLE_NAME = "E_FormFlow";

        [DbField(true)]
        public int Oper_Id { get; set; }
        [DbField(true)]
        public int Form_Id { get; set; }
        [DbField(true)]
        public int Fwde_Id { get; set; }

        public int Flow_Id { get; set; }

        public string Fmfw_Remarks { get; set; }
        public string Fmfw_UpdateBy { get; set; }
        public Nullable<DateTime> Fmfw_UpdateDate { get; set; }
        public string Fmfw_Status { get; set; }
    }

    public class EformSubFmdtIds
    {
        public string fmdt_id { get; set; }
    }

    public class EformLoadQuery
    {
        public List<EFormDataFieldQuery> fields = new List<EFormDataFieldQuery>();
        public List<EFormModel> defs = new List<EFormModel>();
        public List<EFlowInfoQuery> flowinfo = new List<EFlowInfoQuery>();
        public List<EFormOperationQuery> privilege = new List<EFormOperationQuery>();
        public List<EFormDataFieldQuery> subctrls = new List<EFormDataFieldQuery>();
        public List<EformAttachModel> atchs = new List<EformAttachModel>();
        public EformSubs subdata { get; set; }
    }
    public class EformSubs
    {
        public List<EformSubRow> rows = new List<EformSubRow>();
    }
    public class EformSubRow
    {
        public List<EFormDataFieldQuery> cols = new List<EFormDataFieldQuery>();
    }

    public class EformAttachModel
    {
        public const string TABLE_NAME = "E_FormAttach";

        [DbField(true)]
        public int Fmat_Id { get; set; } //SEQ_EINFRM_BATCHID
        public int Oper_Id { get; set; }
        public int Field_Id { get; set; }
        public int Fmdt_Id { get; set; }
        public int Form_Id { get; set; }
        public string Fmat_FileName { get; set; }
        public string Fmat_FileDesc { get; set; }
        public string Fmat_FileUrl { get; set; }
        public byte[] Fmat_FileData { get; set; }
        public string Fmat_Status { get; set; }
        public string Fmat_CreateBy { get; set; }
        public DateTime Fmat_CreateDate { get; set; }
        public string Fmat_UpdateBy { get; set; }
        public Nullable<DateTime> Fmat_UpdateDate { get; set; }

        public EformAttachModel() { }

        public EformAttachModel(int operid, string fileName, string fileUrl)
        {
            this.Oper_Id = operid;
            this.Fmat_FileName = fileName;
            this.Fmat_FileUrl = fileUrl;
        }
    }

    #endregion

    #region experiment

    public class TaskSchedule
    {
        static int Status = 0; //0->standby;1->running
        /*
         * 获取配置文件
         */
        public int Id { get; set; }
        public string Name { get; set; }        
        public int Type { get; set; } //daily，weekly,monthly
        public DateTime StartAt { get; set; }
        
        //public TaskSchedule(DbTx tx) : base(tx) { }
        public void Run()
        {
            Status = 1;
        }
    }

    #endregion
}
