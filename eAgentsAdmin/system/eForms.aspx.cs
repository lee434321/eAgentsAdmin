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
    public partial class eForms : PageBase
    {
        static List<string> searchKeyWord = new List<string>() { 
            "approved","unapproved","已审批","未审批"
        };

        string sql2 = "select a.form_id,a.form_code, a.form_name,a.form_active,a.form_status, a.form_efffrom, a.form_effto, a.form_desc, a.form_attachurl,a.form_attachname,a.flow_id,a.flow_code,a.form_property,a.form_lease,a.form_customer,"
            + " a.form_header,a.form_waist,a.form_footer,a.form_logo,a.form_picture,"
            + " a.form_createby,a.form_updateby,a.form_approveby,a.form_createdate,a.form_updatedate,a.form_approvedate,a.form_template,a.form_agreement,a.fmtb_id,a.fmtb_code, "
            + " a.tmdf_id,a.tmdf_code,"
            + " b.userid, b.loginname, b.email, b.registration_id, b.device_id, b.groupid, b.groupname, b.dept,b.approver"         
            + " from e_form a,"
            + " (select distinct d.form_id,c.userid,c.loginname,c.email,c.registration_id,c.device_id,c.groupid,c.groupname,c.dept,c.approver "
            + " from sys_backenduser_role_v c, sys_eform_property_v d"
            + " where trim(c.propertycode) = trim(d.Property_Code)"
            + " and c.loginname = '{0}' and c.groupname like '{1}') b" //变量：{0}表示当前登录的loginname,{1}表示当前的登录的groupname
            + " where a.form_active = 'A' and a.form_id = b.FORM_ID";

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

            if (Helper.Aid.DbNull2Str(Session["ReturnUrl"]) != "")
            {
                Session["ReturnUrl"] = null;
            }

            if (!string.IsNullOrEmpty(action))
            {
                DateTime start = DateTime.Now;
                string dev = "";
                JsonRsp rsp = new JsonRsp();
                string result = "";
                int id;
                switch (action)
                {
                    case "load":
                        result = this.LoadInitData();
                        break;
                    case "checkenv"://检查是否是开发环境                       
                        rsp.result = dev == "1" ? true : false;
                        result = JsonConvert.SerializeObject(rsp);
                        break;
                    case "approve":
                        string ids = Request["ids"];
                        result = this.Approve(ids);
                        break;
                    case "getGroup":
                        //result = this.GetGroup();
                        break;
                    case "del":
                        id = int.Parse(Request.Form["id"].ToString());
                        result = this.Delete(id);
                        break;
                    case "tickall":
                        break;
                    case "getbase64str":
                        result = GetBase64Str();
                        break;
                    case "getFormById":
                        result = GetEformById();
                        break;
                    case "upload":
                        try
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(Request.Files[0].FileName); // 打开文件对象
                            string fnp = fi.Name.Replace(fi.Extension, "_" + Helper.Aid.Null2Str(Helper.Aid.getTimestamp() / 1000) + fi.Extension);//真实保存的文件名                       
                            string p = Request.PhysicalApplicationPath;
                            string rp = @"upload\";
                            if (!System.IO.Directory.Exists(p + rp)) System.IO.Directory.CreateDirectory(p + rp); // 创建路径upload
                            p += rp + fnp;
                            Request.Files[0].SaveAs(p);//保存的文件名
                            rsp.result = "upload/" + fnp;
                            Attachment file = new Attachment();
                            file.name = fi.Name;
                            file.url = "upload/" + fnp;
                            rsp.result = file;

                            /// -- start -- base64上传
                            System.IO.Stream sm = Request.Files[0].InputStream;
                            byte[] buffer = new byte[sm.Length];
                            sm.Read(buffer, 0, buffer.Length);
                            string binStrData = Convert.ToBase64String(buffer);
                            UploadParas data = new UploadParas();
                            data.fileName = fnp;
                            data.base64Data = binStrData;
                            UploadByBase64(data, true);
                            /// --  end  --                     
                        }
                        catch (Exception err)
                        {
                            rsp.result = -99;
                            rsp.err_msg = err.Message;
                        }
                        result = JsonConvert.SerializeObject(rsp);
                        break;
                    case "getProps":
                        //result = this.GetProps();
                        break;
                    case "search":
                        string sp = Request.Form["searchParse"].ToString();
                        int pidx = int.Parse(Request.Form["pageIdx"].ToString());
                        int size = int.Parse(Request.Form["pageSize"]);
                        if (Request.Form.AllKeys.Contains("id"))
                            id = int.Parse(Request.Form["id"].ToString());
                        else
                            id = 0;

                        DateTime s = DateTime.Now;
                        result = this.Search(sp, pidx, size);
                        DateTime end = DateTime.Now;
                        Commom.Log.WriteLog("eform search elapsed " + end.Subtract(s).ToString() + " seconds");
                        break;
                    case "save":
                        start = DateTime.Now;
                        result = this.Save();
                        Log.WriteLog("Save[" + result + "] elasped " + DateTime.Now.Subtract(start).Milliseconds.ToString() + "ms");
                        break;
                    case "timeout":
                        rsp.err_code = -98;
                        rsp.err_msg = "session timeout,need login";
                        rsp.result = null;
                        result = JsonConvert.SerializeObject(rsp);
                        break;
                    case "search_lease":
                        result = this.SearchLease();
                        break;
                    default:
                        rsp.err_code = -1;
                        rsp.err_msg = "action undefined";
                        result = JsonConvert.SerializeObject(rsp);
                        break;
                }

                Response.Write(result);
                Response.End();
            }
        }

        private string GetEformById()
        {
            throw new NotImplementedException();
        }

        private string GetBase64Str()
        {
            JsonRsp rsp = new JsonRsp();
            rsp.result = Helper.Aid.Stream2Base64Str(Request.Files[0].InputStream);
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }     

        /// <summary>
        /// 审批单据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// 注意：这个按钮只有可能是审批者才可以按下，所以不必再做有效性检查
        private string Approve(string ids)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                string[] tstr = Request.Form["ids"].ToString().Split(',');
                int id = 0;
                for (int i = 0; i < tstr.Length; i++)
                {
                    id = int.Parse(tstr[i]);                    
                    List<EFormModel> etys = bz.Retrieve<EFormModel>(x => x.FORM_ID == id);
                    if (etys.Count > 0)
                    {
                        etys[0].FORM_STATUS = "A";
                        etys[0].FORM_APPROVEBY = Session["loginname"].ToString().Trim();
                        etys[0].FORM_APPROVEDATE = DateTime.Now;
                        bz.Update(etys[0]);
                        rsp.result = true;
                    }
                    else
                    {
                        rsp.err_code = -1;
                        rsp.err_msg = "not found!";
                    }
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

        private string LoadInitData()
        {
            InitData idata = new InitData();
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                /// 1. 取group信息
                int grpid = int.Parse(Session["groupid"].ToString());                
                List<Model.Sys_Users_Group> etys = bz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == grpid);
                if (etys.Count > 0)
                {                    
                    List<Model.Sys_Users_Group_Property> etys2 = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == grpid);
                    etys2 = etys2.OrderBy(x => x.PropertyCode).ToList();
                    for (int i = 0; i < etys2.Count; i++)                    
                        etys2[i].PropertyCode = etys2[i].PropertyCode.Trim();                    
                    GroupInfo grp = new GroupInfo();
                    grp.group = etys[0];
                    grp.props = etys2;

                    idata.grpinfo = grp;
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = string.Format("Group id '{0}' not found.", grpid.ToString());
                    rsp.result = null;
                }               
                /// 2.取模板和流信息
                EformBiz50 bz50 = new EformBiz50(tx);
                idata.tpls = bz50.GetEformTemplateV();
                /// 3.取环境标识
                string dev = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["demo"]);
                idata.demo = dev == "1" ? true : false;
                /// 4.取timeperiod下拉信息
                List<ETimeDefineModel> etysETD = bz.Retrieve<ETimeDefineModel>(x => x.tmdf_active.Sql_Like("A") && x.tmdf_status.Sql_Like("A"), " order by tmdf_code ");
                idata.etds = etysETD;

                rsp.result = idata;
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

        private string Delete(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<EFormModel> etys = bz.Retrieve<EFormModel>(x => x.FORM_ID == id);
                if (etys.Count > 0)
                {
                    etys[0].FORM_ACTIVE = "I";
                    bz.Update(etys[0]);
                    rsp.result = true;
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
        /// 检索
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pidx"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private string Search(string criteria, int pidx, int pageSize = 10)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {                
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                sql2 = string.Format(sql2, Helper.Aid.Null2Str(Session["loginname"]), LoginSystem.GroupInfo.GroupName);

                ///条件处理
                if (criteria.Length > 0)
                {
                    searchKeyWord.Contains(criteria);
                    List<string> word = new List<string>();
                    for (int i = 0; i < searchKeyWord.Count; i++)
                    {
                        if (searchKeyWord[i].ToLower() == criteria.ToLower())
                        {
                            word.Add(criteria);
                        }
                    }

                    if (word.Count > 0)
                    {
                        for (int j = 0; j < word.Count; j++)
                        {
                            if (word[j].ToLower() == "approved" || word[j].ToLower() == "已审批")
                            {
                                sql2 += " and a.form_status='A'";
                            }
                            else if (word[j].ToLower() == "unapproved" || word[j].ToLower() == "未审批")
                            {
                                sql2 += " and a.form_status='I'";
                            }
                        }
                    }
                    else
                    {
                        sql2 += " and (upper(a.form_code) like '%" + criteria.ToUpper() + "%'";
                        sql2 += " or upper(a.form_name) like '%" + criteria.ToUpper() + "%'";
                        sql2 += " or upper(a.form_desc) like '%" + criteria.ToUpper() + "%'";
                        sql2 += " or upper(a.form_property) like '%" + criteria.ToUpper() + "%'";
                        int def = 0;
                        if (int.TryParse(criteria, out def))
                            sql2 += " or a.form_id= " + criteria;
                        sql2 += ")";
                    }
                }
                
                sql2 += " order by form_efffrom desc,form_name";                
                List<EFormModel> etys = bz.Retrieve<EFormModel>(sql2);
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

        private string SearchLease()
        {
            JsonRsp rsp = new JsonRsp();
            string sp = Request.Form["searchPharse"].ToString();
            string props = Request.Form["props"].ToString();

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                string sql = "select trim(l.lease_number) as lease_number,l.customer_number, c.customer_name"
                    + " from Pone_Lm_Lease l,pone_customer c"
                    + " where 1 = 1"
                    + "  and l.customer_number=c.customer_number"
                    + " and l.active = 'A'"
                    + " and l.lease_term_to >= sysdate - 720"
                    + " and instr('" + props + "',trim(l.property_code))>=1";

                if (sp != "")
                {
                    sql += " and (";
                    sql += "  upper(l.lease_number) like '%" + sp.ToUpper() + "%'";
                    sql += "  or upper(l.customer_number) like '%" + sp.ToUpper() + "%'";
                    sql += "  or upper(c.customer_name) like '%" + sp.ToUpper() + "%'";
                    sql += ")";
                }

                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<LeasesQuery> etys = bz.Retrieve<LeasesQuery>(sql);
                etys = etys.OrderBy(x => x.Lease_Number).ThenBy(x => x.Customer_Number).ThenBy(x => x.Customer_Name).ToList();
                rsp.result = etys;

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

        /// <summary>
        /// 对原来的save做优化(未启用)
        /// </summary>
        /// <returns></returns>
        private string Save2()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                EFormModel ety = new EFormModel();
                int cnt = 0;                
                EFormModel paras = Newtonsoft.Json.JsonConvert.DeserializeObject<EFormModel>(Helper.Aid.Stream2Str(Request.InputStream));
                string act = paras.action;

                bool bothNeg = (paras.FLOW_ID == -1 && paras.FMTB_ID == -1) ? true : false; //derek提出的新规则，如果flow_id和fmtb_id都没有选择，置为true。                
                List<EformTemplateVQuery> etystpl = bz.Retrieve<EformTemplateVQuery>(x => x.fmtb_id == paras.FMTB_ID);//“模板”是必填项，因为数据库有约束(derek设计)
                if (etystpl.Count <= 0)
                    throw new Exception("template not found for fmtb_id:'" + paras.FMTB_ID.ToString() + "'，Please select a template.");

                if (act == "new") {
                    ety = new EFormModel();
                }
                else
                {
                    //bz.WhereClauses.Add("Form_Id", new Helper.Evaluation(Helper.SqlOperator.EQ, paras.FORM_ID));
                    List<EFormModel> etys = bz.Retrieve<EFormModel>(x => x.FORM_ID == paras.FORM_ID);
                    if (etys.Count == 0)
                        throw new Exception("The eform not found. It might be deleted please refresh.");
                    if (etys[0].FORM_STATUS.Trim() == "A" && LoginSystem.GroupInfo.Approver != "1")
                        throw new Exception("Already approved, can not save again.");
                    ety = etys[0];
                }

                ety.FORM_CODE = paras.FORM_CODE;
                ety.FORM_NAME = paras.FORM_NAME;
                ety.FORM_DESC = paras.FORM_DESC;
                ety.FORM_LEASE = paras.FORM_LEASE;
                ety.FORM_EFFFROM = paras.FORM_EFFFROM;
                ety.FORM_EFFTO = paras.FORM_EFFTO;
                ety.FLOW_CODE = paras.FLOW_CODE;
                ety.FLOW_ID = paras.FLOW_ID;
                ety.FORM_TEMPLATE = bothNeg ? "" : etystpl[0].template_filename;
                ety.FMTB_ID = paras.FMTB_ID;
                ety.FMTB_CODE = bothNeg ? "" : etystpl[0].fmtb_code;
                ety.FORM_HEADER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_HEADER_HTML);
                ety.FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                ety.FORM_FOOTER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_FOOTER_HTML);

                string logo64 = Helper.Aid.Null2Str(paras.FORM_LOGO_Base64);
                if (logo64 != "")
                    ety.FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    
                else
                    ety.FORM_LOGO = null;

                string pic64 = Helper.Aid.Null2Str(paras.FORM_PICTURE_Base64);
                if (pic64 != "")
                    ety.FORM_PICTURE = Convert.FromBase64String(pic64);
                else
                    ety.FORM_PICTURE = new byte[0];

                ety.FORM_PROPERTY = paras.FORM_PROPERTY;
                if (Session["loginname"] == null)
                    throw new Exception("session timeout.");
                ety.FORM_CREATEBY = Session["loginname"].ToString();
                ety.FORM_CREATEDATE = DateTime.Now;
                ety.FORM_STATUS = "I";
                ety.FORM_AGREEMENT = paras.FORM_AGREEMENT;                                                       

                /// -- start -- 处理attachment时考虑删除的情况                    
                if (paras.FORM_ATTACHURL == "" || paras.FORM_ATTACHURL != ety.FORM_ATTACHURL) //如果附件的url为空或者与原来的不同，则处理删除
                {
                    if (Helper.Aid.Null2Str(ety.FORM_ATTACHURL) != "") //如果原来的url有值
                        System.IO.File.Delete(Helper.Aid.yPath + @"\" + ety.FORM_ATTACHURL.Replace("/", "\\"));
                }
                ety.FORM_ATTACHURL = paras.FORM_ATTACHURL;
                ety.FORM_ATTACHNAME = paras.FORM_ATTACHNAME;
                /// --  end  --
                
                ety.FORM_UPDATEBY = Helper.Aid.DbNull2Str(Session["loginname"]);
                ety.FORM_UPDATEDATE = DateTime.Now;

                cnt = act == "new" ? bz.Create(ety) : bz.Update(ety);

                if (cnt>0)                
                    rsp.result = act == "new" ? Convert.ToInt32(tx.ExecuteScalar("select seq_eform_id.Currval from dual")) : ety.FORM_ID;

                tx.CommitTrans();
                if (LoginSystem.GroupInfo.Approver != "1")
                    SendMail2Approver(ety, paras.FORM_PROPERTY.Split(','));
            }
            catch (System.Data.OracleClient.OracleException oerr)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                if (oerr.Message.Contains("ORA-00001"))
                    rsp.err_msg = "This Code has already existed. Please input other Code.";
                else
                    rsp.err_msg = oerr.Message;
                rsp.result = false;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = false;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Save()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                EFormModel ety = new EFormModel();
                int cnt = 0;                
                EFormModel paras = Newtonsoft.Json.JsonConvert.DeserializeObject<EFormModel>(Helper.Aid.Stream2Str(Request.InputStream));
                string act = paras.action;

                /// -- start -- derek提出的新规则，如果flow_id和fmtb_id都没有选择，置为true。                
                /// 邮件原文摘录如下：
                /// RE: HutchisonAgent - Useful forms   2021/12/15 (周三) 8:19
                /// 修改成：如果模板不为空，则Flow必须选， 如果模板为空，则Attachment必须有值                                
                int fmtbIdWithNull = 0;
                if (paras.FMTB_ID == -1) //如果模板为空
                {
                    if (string.IsNullOrEmpty(paras.FORM_ATTACHNAME))
                        throw new Exception("attachment is required when template unselected.");
                }
                else //如果模板不为空
                {
                    fmtbIdWithNull = paras.FMTB_ID;
                    if (paras.FLOW_ID == -1)
                        throw new Exception("flow is required when template selected");
                }

                //bool bothNeg = (paras.FLOW_ID == -1 && paras.FMTB_ID == -1) ? true : false;                 
                //if (!bothNeg)                
                //    fmtb_idWithNull = paras.FMTB_ID;
                //if (paras.FMTB_ID == -1)
                //    fmtb_idWithNull = 0;
                
                List<EformTemplateVQuery> etystpl = bz.Retrieve<EformTemplateVQuery>(x => x.fmtb_id == fmtbIdWithNull);//“模板”是必填项，-1时以0填充。因为数据库有约束(derek设计)
                if (etystpl.Count <= 0)
                    throw new Exception("template not found for fmtb_id:'" + paras.FMTB_ID.ToString() + "'，Please select a template.");
                /// --  end  --
                
                if (act == "new")
                {
                    ety = new EFormModel();
                    ety.FORM_CODE = paras.FORM_CODE;
                    ety.FORM_NAME = paras.FORM_NAME;
                    ety.FORM_DESC = paras.FORM_DESC;
                    ety.FORM_LEASE = paras.FORM_LEASE;
                    ety.FORM_EFFFROM = paras.FORM_EFFFROM;
                    ety.FORM_EFFTO = paras.FORM_EFFTO;
                    ety.FLOW_CODE = paras.FLOW_CODE;
                    ety.FLOW_ID = paras.FLOW_ID;
                    ety.FORM_TEMPLATE = etystpl[0].template_filename;
                    ety.FMTB_ID = fmtbIdWithNull;
                    ety.FMTB_CODE =etystpl[0].fmtb_code;
                    ety.FORM_HEADER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_HEADER_HTML);
                    ety.FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                    ety.FORM_FOOTER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_FOOTER_HTML);
                    if (Aid.Null2Int(paras.TMDF_ID) > 0)
                    {
                        ety.TMDF_ID = paras.TMDF_ID;
                        ety.TMDF_CODE = paras.TMDF_CODE;
                    }
                    string logo64 = Helper.Aid.Null2Str(paras.FORM_LOGO_Base64);
                    if (logo64 != "")
                        ety.FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    
                    else
                        ety.FORM_LOGO = null;
                    
                    string pic64 = Helper.Aid.Null2Str(paras.FORM_PICTURE_Base64);
                    if (pic64 != "")
                        ety.FORM_PICTURE = Convert.FromBase64String(pic64);
                    else
                        ety.FORM_PICTURE = new byte[0];
                   
                    ety.FORM_ATTACHURL = paras.FORM_ATTACHURL;
                    ety.FORM_ATTACHNAME = paras.FORM_ATTACHNAME;
                    ety.FORM_PROPERTY = paras.FORM_PROPERTY;
                    if (Session["loginname"] == null)
                        throw new Exception("session timeout.");
                    ety.FORM_CREATEBY = Session["loginname"].ToString();
                    ety.FORM_CREATEDATE = DateTime.Now;
                    ety.FORM_STATUS = "I";
                    ety.FORM_AGREEMENT = paras.FORM_AGREEMENT;
                    cnt = bz.Create(ety);
                    rsp.result = cnt > 0 ? Convert.ToInt32(tx.ExecuteScalar("select seq_eform_id.Currval from dual")) : 0; //拿到自增ID值
                    Log.WriteLog("new eform creation with name " + ety.FORM_NAME + " by " + ety.FORM_CREATEBY);
                }
                else
                {                   
                    List<EFormModel> etys = bz.Retrieve<EFormModel>(x => x.FORM_ID == paras.FORM_ID);

                    if (etys.Count == 0)
                        throw new Exception("The eform not found. It might be deleted ,please refresh.");

                    if (etys[0].FORM_STATUS.Trim() == "A" && LoginSystem.GroupInfo.Approver != "1")
                        throw new Exception("Already approved, can not save again.");

                    etys[0].FORM_CODE = paras.FORM_CODE;
                    etys[0].FORM_NAME = paras.FORM_NAME;
                    etys[0].FORM_DESC = paras.FORM_DESC;
                    etys[0].FORM_LEASE = paras.FORM_LEASE;
                    etys[0].FORM_EFFFROM = paras.FORM_EFFFROM;
                    etys[0].FORM_EFFTO = paras.FORM_EFFTO;
                    etys[0].FORM_TEMPLATE = etystpl[0].template_filename;
                    etys[0].FMTB_ID = fmtbIdWithNull;
                    etys[0].FMTB_CODE = etystpl[0].fmtb_code;                    
                    etys[0].FLOW_CODE = paras.FLOW_CODE;
                    etys[0].FLOW_ID = paras.FLOW_ID;
                    etys[0].FORM_HEADER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_HEADER_HTML);
                    etys[0].FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                    etys[0].FORM_FOOTER = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_FOOTER_HTML);
                    if (Aid.Null2Int(paras.TMDF_ID) > 0)
                    {
                        etys[0].TMDF_ID = paras.TMDF_ID;
                        etys[0].TMDF_CODE = paras.TMDF_CODE;
                    }
                    string logo64 = Helper.Aid.Null2Str(paras.FORM_LOGO_Base64);
                    if (logo64 != "")
                        etys[0].FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    
                    else
                        etys[0].FORM_LOGO = new byte[0];

                    string pic64 = Helper.Aid.Null2Str(paras.FORM_PICTURE_Base64);
                    if (pic64 != "")
                        etys[0].FORM_PICTURE = Convert.FromBase64String(pic64);
                    else
                        etys[0].FORM_PICTURE = new byte[0];
         
                    etys[0].FORM_PROPERTY = paras.FORM_PROPERTY;
                    
                    /// -- start -- 处理attachment时考虑删除的情况                    
                    if (paras.FORM_ATTACHURL == "" || paras.FORM_ATTACHURL != etys[0].FORM_ATTACHURL) //如果附件的url为空或者与原来的不同，则处理删除
                    {
                        if (Helper.Aid.Null2Str(etys[0].FORM_ATTACHURL) != "") //如果原来的url有值
                            System.IO.File.Delete(Helper.Aid.yPath + @"\" + etys[0].FORM_ATTACHURL.Replace("/", "\\"));
                    }
                    etys[0].FORM_ATTACHURL = paras.FORM_ATTACHURL;
                    etys[0].FORM_ATTACHNAME = paras.FORM_ATTACHNAME;                                       
                    /// --  end  --
                    
                    etys[0].FORM_UPDATEBY = Helper.Aid.DbNull2Str(Session["loginname"]);
                    etys[0].FORM_UPDATEDATE = DateTime.Now;
                    etys[0].FORM_STATUS = "I";
                    etys[0].FORM_AGREEMENT = paras.FORM_AGREEMENT;
                    cnt = bz.Update(etys[0]);
                    ety = etys[0];//这里赋值给ety用来发送邮件

                    rsp.result = ety.FORM_ID;
                }
                tx.CommitTrans();                

                if (LoginSystem.GroupInfo.Approver != "1")
                    SendMail2Approver(ety, paras.FORM_PROPERTY.Split(','));
            }
            catch (System.Data.OracleClient.OracleException oerr)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                if (oerr.Message.Contains("ORA-00001"))
                    rsp.err_msg = "This Code has already existed. Please input other Code.";
                else
                    rsp.err_msg = oerr.Message;
                rsp.result = false;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = false;
            }
            return JsonConvert.SerializeObject(rsp);
        }
     
        private void SendMail2Approver(EFormModel ety, string[] props)
        {
            string inprops = "(";
            for (int i = 0; i < props.Length; i++)
            {
                inprops += "'" + props[i] + "',";
            }
            inprops = inprops.Substring(0, inprops.Length - 1);
            inprops += ")";

            string sql = "select * from sys_login_system ";
            sql += " where userid in (select userid";
            sql += "   from sys_login_system_group";
            sql += "     where groupid in (select groupid";
            sql += "       from sys_users_group";
            sql += "         where groupid in";
            sql += "          (select groupid";
            sql += "             from sys_users_group_property";
            sql += "            where propertycode in " + inprops + ")";
            sql += " and approver = '1' and dept in ('Admin', '" + LoginSystem.GroupInfo.Dept + "')))";

            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>(sql);
                if (etys.Count > 0)
                {
                    List<string> tos = new List<string>();
                    for (int i = 0; i < etys.Count; i++)
                    {
                        if (tos.Contains(etys[i].Email))
                            continue;
                        tos.Add(etys[i].Email);
                    }

                    string link = Request.Url.AbsoluteUri.Replace(Request.Url.Query, "");
                    ExchangeMail em = new ExchangeMail();
                    string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
                    
                    string body = GenMailBody(ety, props, link + "?token=" + System.Guid.NewGuid().ToString().Substring(0, 7) + "&id=" + ety.FORM_ID.ToString() + "&code=" + ety.FORM_CODE);
                    if (dev == "0")
                    {
                        em.SendExchangeMail(tos, "Approval Request of eApplication Form Creation on HutchisonAgent", body);
                        Log.WriteLog("Send eMail to " + FlatArray(tos.ToArray()));
                    }
                    else
                    {
                        em.SendExchangeMail("liwei@hwpg.com", "Approval Request of eApplication Form Creation on HutchisonAgent", body);
                        Log.WriteLog("Send eMail to liwei@hwpg.com");
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                throw err;
            }
        }

        private string GenMailBody(EFormModel ety, string[] props, string link)
        {
            string result = "You have an approval request of eApplication Form Creation on HutchisonAgent." + "<br/>";
            result += "<br/>";
            result += "The following are the eApplication Form info:" + "<br/>";
            result += string.Format("Code : [{0}]", ety.FORM_CODE) + "<br/>";
            result += string.Format("Name : [{0}]", ety.FORM_NAME) + "<br/>";
            result += string.Format("Content : [{0}]", ety.FORM_DESC) + "<br/>";
            result += string.Format("Related Property : [{0}]", FlatArray(props).Replace("'", "")) + "<br/>";
            result += string.Format("Related Lease Number : [{0}]", ety.FORM_LEASE == "" ? "All" : ety.FORM_LEASE) + "<br/>";
            result += string.Format("Created by : [{0}]", ety.FORM_CREATEBY) + "<br/>";
            result += "<br/>";
            result += string.Format("Please check it by the following [<a href={0}>link</a>]", link) + "<br/>";
            result += "<br/>";
            result += "This e-mail serves as notification only, please do not reply to this email." + "<br/>";
            result += "<br/>";
            result += "HutchisonAgent System" + "<br/>";
            result += "<br/>";
            return result;
        }

        private string FlatArray(string[] arr)
        {
            if (arr.Length > 0)
            {
                string result = "";
                for (int i = 0; i < arr.Length; i++)
                {
                    result += "'" + arr[i].Trim() + "',";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
            return "";
        }

        private void UploadByBase64(UploadParas paras, bool isAttach = false)
        {
            /// -- start -- base64上传           
            try
            {
                string sUrl = System.Configuration.ConfigurationManager.AppSettings["transUrl"];
                sUrl += "?act=file";
                string filePath = "/Upload/" + paras.fileName;
                string data = "";
                if (isAttach)
                    data = "{\"isAttach\":\"Y\",\"fileName\":\"" + filePath + "\",\"base64Data\":\"" + paras.base64Data + "\"}";
                else
                    data = "{\"isAttach\":\"N\",\"fileName\":\"" + filePath + "\",\"base64Data\":\"" + paras.base64Data + "\"}";
                EncryptUtil encry = new EncryptUtil();
                data = encry.Encrypt(data);
                string param = HttpUtility.UrlEncode("data") + "=" + HttpUtility.UrlEncode(data);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(param);

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                System.IO.Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();

                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s = response.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(s);
                string result = sr.ReadToEnd();
                s.Close();

                Log.WriteLog("upload to transfer url:" + sUrl + " with filename:" + paras.fileName + " response:" + result);

            }
            catch (Exception err)
            {
                Log.WriteLog("上传失败:" + err.Message + err.StackTrace);
                throw err;
            }
            /// --  end  --
            /// 
            
        }

        private void Test()
        {
            try
            {
                Oracle.ManagedDataAccess.Client.OracleConnection conn = new Oracle.ManagedDataAccess.Client.OracleConnection("");
                
            }
            catch (Exception)
            {
                //err.Message;
            }
        }
    }

    /// <summary>
    /// 分页类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pager<T> where T : class
    {
        public int pageSize { get; set; }   //
        public int count { get; set; }      //
        public int pageIdx { get; set; }    //
        public List<T> data { get; set; }    //array    
    }

    public class Attachment
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class GroupInfo
    {
        public Model.Sys_Users_Group group { get; set; }  //登录用户所属的group信息
        public List<Model.Sys_Users_Group_Property> props = new List<Model.Sys_Users_Group_Property>(); //可访问的prop列表
    }

    /// <summary>
    /// 二进制文档上传参数类
    /// </summary>
    public class UploadParas
    {
        public string fileName { get; set; }
        public string base64Data { get; set; }
    }

    /// <summary>
    /// EForm实体(也用于接收序列化参数)
    /// </summary>
    public class EFormModel
    {
        public const string TABLE_NAME = "E_Form";

        [DbField(true, true)]
        public int FORM_ID { get; set; }
        public string FORM_CODE { get; set; }
        public string FORM_NAME { get; set; }
        public string FORM_DESC { get; set; }
        public string FORM_LEASE { get; set; }
        public string FORM_ATTACHURL { get; set; }
        public string FORM_ATTACHNAME { get; set; }
        public DateTime FORM_EFFFROM { get; set; }
        public Nullable<DateTime> FORM_EFFTO { get; set; }
        public byte[] FORM_HEADER { get; set; } //需要转换
        public byte[] FORM_WAIST { get; set; }  //需要转换
        public byte[] FORM_FOOTER { get; set; }
        public byte[] FORM_LOGO { get; set; }   //需要转换
        public byte[] FORM_PICTURE { get; set; }
        public string FORM_PROPERTY { get; set; }
        public string FORM_STATUS { get; set; } //这里是指审批状态 I：未审批；A：已审批
        public string FORM_ACTIVE { get; set; }
        public Nullable<int> FLOW_ID { get; set; }
        public string FLOW_CODE { get; set; }
        public string FORM_CUSTOMER { get; set; }
        public string FORM_CREATEBY { get; set; }
        public string FORM_UPDATEBY { get; set; }
        public string FORM_APPROVEBY { get; set; }
        public string FORM_TEMPLATE { get; set; }
        public string FORM_AGREEMENT { get; set; } //协议标志
        public int FMTB_ID { get; set; }
        public string FMTB_CODE { get; set; }
        public Nullable<DateTime> FORM_CREATEDATE { get; set; }
        public Nullable<DateTime> FORM_APPROVEDATE { get; set; }
        public Nullable<DateTime> FORM_UPDATEDATE { get; set; }

        public int TMDF_ID { get; set; }
        public string TMDF_CODE { get; set; }

        #region 以下字段不在数据库内

        string m_FORM_HEADER_HTML = "";
        [DbField(false, false, true)]
        public string FORM_HEADER_HTML
        {
            get
            {
                if (FORM_HEADER != null)
                {
                    return System.Text.Encoding.UTF8.GetString(FORM_HEADER);
                }
                else
                    return m_FORM_HEADER_HTML;
            }
            set
            {
                m_FORM_HEADER_HTML = value;
            }
        }

        string m_Form_Waist_Html = "";
        [DbField(false, false, true)]
        public string FORM_WAIST_HTML
        {
            get
            {
                if (FORM_WAIST != null)
                {
                    return System.Text.Encoding.UTF8.GetString(FORM_WAIST);
                }
                else
                    return m_Form_Waist_Html;
            }
            set
            {
                m_Form_Waist_Html = value;
            }
        }

        string m_FORM_FOOTER_HTML = "";
        [DbField(false, false, true)]
        public string FORM_FOOTER_HTML
        {
            get
            {
                if (FORM_FOOTER != null)
                {
                    return System.Text.Encoding.UTF8.GetString(FORM_FOOTER);
                }
                else
                    return m_FORM_FOOTER_HTML;
            }
            set
            {
                m_FORM_FOOTER_HTML = value;
            }
        }

        string m_FORM_LOGO_Base64;
        [DbField(false, false, true)]
        public string FORM_LOGO_Base64
        {
            get
            {
                if (FORM_LOGO != null)
                    return Convert.ToBase64String(FORM_LOGO);
                else
                    return m_FORM_LOGO_Base64;
            }
            set
            {
                m_FORM_LOGO_Base64 = value;
            }
        }

        string m_FORM_PICTURE_Base64;
        [DbField(false, false, true)]
        public string FORM_PICTURE_Base64
        {
            get
            {
                if (FORM_PICTURE != null)
                    return Convert.ToBase64String(FORM_PICTURE);
                else
                    return m_FORM_PICTURE_Base64;
            }
            set
            {
                m_FORM_PICTURE_Base64 = value;
            }
        }

        [DbField(false, false, true)]
        public string action { get; set; }

        [DbField(false, false, true)]
        public string props { get; set; }
        #endregion
    }

    /// <summary>
    /// Lease 查询实体(只能用于查询)
    /// </summary>
    public class LeasesQuery
    {       
        public string Lease_Number { get; set; }
        public string Customer_Number { get; set; }
        public string Customer_Name { get; set; }

        [DbField(false,false,true)]
        public bool Ticked { get; set; }
    }

    public class InitData
    {
        public bool demo { get; set; } //是否为demo环境
        public GroupInfo grpinfo { get; set; }
        public List<EformTemplateVQuery> tpls { get; set; }
        public List<ETimeDefineModel> etds { get; set; } //time period
    }
}
