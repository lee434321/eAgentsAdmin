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

namespace PropertyOneAppWeb.system
{
    public partial class eForms : PageBase
    {
        static List<string> searchKeyWord = new List<string>() { 
            "approved","unapproved","已审批","未审批"
        };

        string sql = "select a.form_id, a.form_code, a.form_name, a.form_active,a.form_status,a.form_efffrom, a.form_effto, a.form_desc, a.form_attachurl,a.form_attachname, a.flow_id, a.flow_code, a.form_property, a.form_lease, a.form_customer"
             + " ,a.form_header,a.form_waist,a.form_footer,a.form_logo"
              + " ,a.form_createby,a.form_updateby,a.form_approveby,a.form_createdate,a.form_updatedate,a.form_approvedate, b.userid, b.loginname, b.email, b.registration_id, b.device_id, b.groupid, b.groupname, b.dept, b.approver, b.propertycode"           
              + " from e_form a, sys_backenduser_role_v b"
              + " where a.form_active = 'A'"
              + " and a.form_property = b.propertycode"
              + " and b.loginname = :p0 ";
             

        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Helper.Aid.DbNull2Str(Request.QueryString["act"]);            
            if (Session["loginname"] == null)
            {
                if (action=="")
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
                JsonRsp rsp = new JsonRsp();
                string result = "";
                int id;
                switch (action)
                {
                    case "checkenv"://检查是否是开发环境
                        string dev = Helper.Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["demo"]);
                        rsp.result = dev == "1" ? true : false;
                        result = JsonConvert.SerializeObject(rsp);
                        break;
                    case "approve":
                        string ids = Request["ids"];
                        result = this.Approve(ids);
                        break;
                    case "getGroup":
                        result = this.GetGroup();
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
                    case "uploadlogo":
                        result = UploadLogo();                        
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
                        result = this.GetProps();
                        break;
                    case "search":
                        string sp = Request.Form["searchParse"].ToString();
                        int pidx = int.Parse(Request.Form["pageIdx"].ToString());
                        int size = int.Parse(Request.Form["pageSize"]);
                        if (Request.Form.AllKeys.Contains("id"))                        
                            id = int.Parse(Request.Form["id"].ToString());                        
                        else
                            id = 0;
                        result = this.Search(sp, pidx, size);
                        break;
                    case "save":
                        //result = this.Save();
                        result = this.SaveByJson();
                        break;
                    case "timeout":
                        rsp.err_code = -1;
                        rsp.err_msg = "session timeout,need login";
                        rsp.result = null;
                        result = JsonConvert.SerializeObject(rsp);
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

        private string GetBase64Str()
        {
            JsonRsp rsp = new JsonRsp();            
            rsp.result = Helper.Aid.Stream2Base64Str(Request.Files[0].InputStream);
            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);            
        }

        private string UploadLogo()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {               
                //string id = Request.QueryString["id"];
                /// -- start -- 保存
                System.IO.FileInfo fi = new System.IO.FileInfo(Request.Files[0].FileName); // 打开文件对象
                System.IO.Stream sm = Request.Files[0].InputStream;
                byte[] buffer = new byte[sm.Length];
                sm.Read(buffer, 0, buffer.Length);
                string binStrData = Convert.ToBase64String(buffer);

                byte[] blob = Convert.FromBase64String(binStrData);

                Helper.DbConsole bz = new Helper.DbConsole(tx);
                bz.Clauses.Add("FORM_ID", new Helper.Evaluation(Helper.SqlOperator.EQ, Helper.Aid.DbNull2Int(Request.QueryString["Id"])));
                List<EFormModel> etys = bz.Retrieve<EFormModel>();
                etys[0].FORM_LOGO = buffer;
                int cnt = bz.Update(etys[0]);

                //System.Data.OracleClient.OracleCommand oc = new System.Data.OracleClient.OracleCommand();
                //System.Data.OracleClient.OracleDataReader or = oc.ExecuteReader();
                //System.Data.OracleClient.OracleLob blob = null;
                //byte[] buf = new byte[blob.Length];
                //blob.Read(buf, 0, buf.Length);
                tx.CommitTrans();
                /// --  end  --
                /// 
                rsp.result = binStrData;
            }
            catch (Exception err)
            {                
                
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
                rsp.result = null;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(rsp);
        }
       
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
                    bz.Clauses.Add("FORM_ID", new Helper.Evaluation(Helper.SqlOperator.EQ, id));
                    List<EFormModel> etys = bz.Retrieve<EFormModel>();
                    if (etys.Count > 0)
                    {
                        if (etys[0].FORM_STATUS.Trim() == "A")
                        {
                            throw new Exception("already approved, can not approve again.");
                        }
                        else
                        {
                            etys[0].FORM_STATUS = "A";
                            etys[0].FORM_APPROVEBY = Session["loginname"].ToString().Trim();
                            etys[0].FORM_APPROVEDATE = DateTime.Now;
                            bz.Update(etys[0]);
                            rsp.result = true;
                        }
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

        private string GetGroup()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);

                int grpid = int.Parse(Session["groupid"].ToString());
                bz.Clauses.Add("GroupId", new Helper.Evaluation(Helper.SqlOperator.EQ, grpid));
                List<Model.Sys_Users_Group> etys = bz.Retrieve<Model.Sys_Users_Group>();
                if (etys.Count > 0)
                {
                    bz.Clauses.Add("GroupId", new Helper.Evaluation(Helper.SqlOperator.EQ, grpid));
                    List<Model.Sys_Users_Group_Property> etys2 = bz.Retrieve<Model.Sys_Users_Group_Property>();

                    GroupInfo grp = new GroupInfo();
                    grp.group = etys[0];
                    grp.props = etys2;
                    rsp.result = grp;
                }
                else
                {
                    rsp.err_code = -1;
                    rsp.err_msg = string.Format("Group id '{0}' not found.", grpid.ToString());
                    rsp.result = null;
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

        private string Delete(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                bz.Clauses.Add("FORM_ID", new Helper.Evaluation(Helper.SqlOperator.EQ, id));
                List<EFormModel> etys = bz.Retrieve<EFormModel>();
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

        private string GetProps()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.pone_pd_property> etys = bz.Retrieve<Model.pone_pd_property>();
                tx.CommitTrans();
                rsp.result = etys;
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
        /// <returns></returns>
        private string Search(string criteria, int pidx, int pageSize = 10)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                sql += " and b.groupid=" + LoginSystem.GroupInfo.GroupId.ToString();
                Helper.DbConsole bz = new Helper.DbConsole(tx);

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
                                sql += " and a.form_status='A'";
                            }
                            else if (word[j].ToLower() == "unapproved" || word[j].ToLower() == "未审批")
                            {
                                sql += " and a.form_status='I'";
                            }
                        }
                    }
                    else
                    {
                        sql += " and (upper(a.form_code) like '%" + criteria.ToUpper() + "%'";
                        sql += " or upper(a.form_name) like '%" + criteria.ToUpper() + "%'";
                        sql += " or upper(a.form_desc) like '%" + criteria.ToUpper() + "%'";
                        sql += " or upper(a.form_property) like '%" + criteria.ToUpper() + "%'";
                        int def = 0;
                        if (int.TryParse(criteria, out def))
                            sql += " or a.form_id= " + criteria;
                        //if (Aid.DbNull2Str(Session["ReturnUrl"]) != "")
                        //{
                           
                        //    if (int.TryParse(criteria, out def))
                        //        sql += " or a.form_id= " + criteria;
                        //    Session["ReturnUrl"] = null;
                        //}
                        sql += ")";
                    }
                }

                bz.Clauses.Add("GroupId", new Helper.Evaluation(Helper.SqlOperator.EQ, LoginSystem.GroupInfo.GroupId));
                List<Model.Sys_Users_Group_Property> etyProps = bz.Retrieve<Model.Sys_Users_Group_Property>();
                if (etyProps.Count > 0)
                {
                    string props = "(";
                    for (int i = 0; i < etyProps.Count; i++)
                    {
                        props += "'" + etyProps[i].PropertyCode + "',";
                    }
                    props = props.Substring(0, props.Length - 1);
                    props += ")";
                    sql += " and b.propertycode in " + props;
                }

                sql += " order by form_efffrom desc,form_name"; 
                Helper.QueryParameter qp1 = new Helper.QueryParameter(":p0", Session["loginname"]);
                List<EFormModelQuery> etys = bz.Retrieve<EFormModelQuery>(sql, qp1);
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

        private Pager SetPager<T>(List<T> etys, int pidx, int pageSize = 10)
        {
            int pageIdx = pidx; //当前页               
            int q = etys.Count / pageSize;
            int r = etys.Count % pageSize;

            Pager pager = new Pager();
            pager.pageSize = pageSize;
            pager.count = r == 0 ? q : q + 1;
            if (pageIdx > pager.count) //如果超过总页数则就是最后1页。
                pageIdx = pager.count;
            if (pageIdx == 0)//如果为0则修改为第1页(翻页时会出现)。
                pageIdx = 1;
            pager.pageIdx = pageIdx;

            List<T> etysOut = new List<T>();
            int idx = 0;
            for (int i = 0; i < pageSize; i++)
            {
                idx = (pageIdx - 1) * pageSize + i;
                if (idx < etys.Count)
                    etysOut.Add(etys[idx]);
                else
                    break;
            }            
            pager.data = etysOut;
            return pager;
        }

        private string SaveByJson()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                EFormModel ety = new EFormModel();
                int cnt = 0;
                EFormModelJson paras = Newtonsoft.Json.JsonConvert.DeserializeObject<EFormModelJson>(Helper.Aid.Stream2Str(Request.InputStream));
                string act = paras.action;
                string[] props = paras.props.Split(',');
                if (act == "new")
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(props[i]))
                        {
                            ety = new EFormModel();                            
                            ety.FORM_CODE = paras.FORM_CODE;
                            ety.FORM_NAME = paras.FORM_NAME;
                            ety.FORM_DESC = paras.FORM_DESC;
                            ety.FORM_LEASE = paras.FORM_LEASE;
                            ety.FORM_EFFFROM = paras.FORM_EFFFROM;
                            ety.FORM_HEADER = paras.FORM_HEADER;
                            ety.FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                            ety.FORM_FOOTER = paras.FORM_FOOTER;

                            string logo64 = Helper.Aid.Null2Str(paras.FORM_LOGO_Base64);
                            if (logo64 != "")
                                ety.FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    

                            ety.FORM_EFFTO = paras.FORM_EFFTO;

                            ety.FORM_ATTACHURL = paras.FORM_ATTACHURL;
                            ety.FORM_ATTACHNAME = paras.FORM_ATTACHNAME;
                            ety.FORM_PROPERTY = props[i].Trim();
                            if (Session["loginname"] == null)
                                throw new Exception("session timeout.");
                            ety.FORM_CREATEBY = Session["loginname"].ToString();
                            ety.FORM_CREATEDATE = DateTime.Now;
                            if (LoginSystem.GroupInfo.Approver == "1")
                                ety.FORM_STATUS = "A";

                            //ety.HEADER_BLOB = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_HEADER);//处理blob形式的header字段
                            //ety.FORM_WAIST_BLOB = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);

                            cnt = bz.Create(ety);
                        }
                    }

                    Log.WriteLog("new eform creation with name " + ety.FORM_NAME + " by " + ety.FORM_CREATEBY);
                }
                else
                {
                    bz.Clauses.Add("Form_Id", new Helper.Evaluation(Helper.SqlOperator.EQ, paras.FORM_ID));
                    List<EFormModel> etys = bz.Retrieve<EFormModel>();
                    etys[0].FORM_CODE = paras.FORM_CODE;
                    etys[0].FORM_NAME = paras.FORM_NAME;
                    etys[0].FORM_DESC = paras.FORM_DESC;
                    etys[0].FORM_LEASE = paras.FORM_LEASE;
                    etys[0].FORM_EFFFROM = paras.FORM_EFFFROM;
                    etys[0].FORM_HEADER = paras.FORM_HEADER;

                    etys[0].FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                    etys[0].FORM_FOOTER = paras.FORM_FOOTER;

                    string logo64 = Helper.Aid.Null2Str(paras.FORM_LOGO_Base64);
                    if (logo64 != "")
                        etys[0].FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    

                    if (string.IsNullOrEmpty(Request.Form["FORM_EFFTO"]))
                        etys[0].FORM_EFFTO = null;
                    else
                        etys[0].FORM_EFFTO = DateTime.Parse(Request.Form["FORM_EFFTO"]);

                    etys[0].FORM_EFFTO = paras.FORM_EFFTO;
                    etys[0].FORM_PROPERTY = props[0];
                    etys[0].FORM_ATTACHURL = paras.FORM_ATTACHURL;
                    etys[0].FORM_ATTACHNAME = paras.FORM_ATTACHNAME;
                    etys[0].FORM_UPDATEBY = Helper.Aid.DbNull2Str(Session["loginname"]);

                    etys[0].FORM_UPDATEDATE = DateTime.Now;
                    if (LoginSystem.GroupInfo.Approver == "1")
                        etys[0].FORM_STATUS = "A";
                    else if (etys[0].FORM_STATUS == "A")
                    {
                        etys[0].FORM_STATUS = "I";
                        //throw new Exception("Non-approver can not modify an approved eform.");
                    }
                    else
                        etys[0].FORM_STATUS = "I";

                    //etys[0].HEADER_BLOB = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_HEADER);
                    //etys[0].FORM_WAIST_BLOB = System.Text.ASCIIEncoding.UTF8.GetBytes(paras.FORM_WAIST_HTML);
                    cnt = bz.Update(etys[0]);
                    ety = etys[0];//这里赋值给ety用来发送邮件
                }
                tx.CommitTrans();
                rsp.result = true;
                if (LoginSystem.GroupInfo.Approver != "1")
                    SendMail2Approver(ety, props);
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
                EFormModelJson paras = Newtonsoft.Json.JsonConvert.DeserializeObject<EFormModelJson>(Helper.Aid.Stream2Str(Request.InputStream));
                string act = Request.Form["action"];                
                string[] props = Request.Form["props"].Split(',');
                if (act == "new")
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(props[i]))
                        {
                            ety = new EFormModel();                           
                            ety.FORM_CODE = Request.Form["Form_Code"];
                            ety.FORM_NAME = Request.Form["Form_Name"];
                            ety.FORM_DESC = Request.Form["Form_Desc"];
                            ety.FORM_LEASE = Request.Form["FORM_LEASE"];
                            ety.FORM_EFFFROM = DateTime.Parse(Request.Form["FORM_EFFFROM"]);
                            ety.FORM_HEADER = Helper.Aid.Null2Str(Request.Form["FORM_HEADER"]);
                            //ety.FORM_HEADER = System.Text.ASCIIEncoding.UTF8.GetBytes(Request.Form["FORM_HEADER"].ToString());
                            ety.FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(Request.Form["FORM_WAIST"].ToString());
                            ety.FORM_FOOTER = Request.Form["FORM_FOOTER"];              
              
                            string logo64 = Helper.Aid.Null2Str(Request.Form["FORM_LOGO_Base64"]);
                            if (logo64 != "")
                                ety.FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    
                                        
                            if (string.IsNullOrEmpty(Request.Form["FORM_EFFTO"]))
                                ety.FORM_EFFTO = null;
                            else
                                ety.FORM_EFFTO = DateTime.Parse(Request.Form["FORM_EFFTO"]);

                            ety.FORM_ATTACHURL = Request.Form["FORM_ATTACHURL"];
                            ety.FORM_ATTACHNAME = Request.Form["FORM_ATTACHNAME"];
                            ety.FORM_PROPERTY = props[i].Trim();
                            if (Session["loginname"] == null)
                                throw new Exception("session timeout.");
                            ety.FORM_CREATEBY = Session["loginname"].ToString();
                            ety.FORM_CREATEDATE = DateTime.Now;
                            if (LoginSystem.GroupInfo.Approver == "1")                            
                                ety.FORM_STATUS = "A";                            
                            cnt = bz.Create(ety);
                        }
                    }

                    Log.WriteLog("new eform creation with name " + ety.FORM_NAME + " by " + ety.FORM_CREATEBY);                
                }
                else
                {
                    bz.Clauses.Add("Form_Id", new Helper.Evaluation(Helper.SqlOperator.EQ, int.Parse(Request.Form["Form_Id"])));
                    List<EFormModel> etys = bz.Retrieve<EFormModel>();
                    etys[0].FORM_CODE = Request.Form["Form_Code"];
                    etys[0].FORM_NAME = Request.Form["Form_Name"];
                    etys[0].FORM_DESC = Request.Form["Form_Desc"];
                    etys[0].FORM_LEASE = Request.Form["FORM_LEASE"];
                    etys[0].FORM_EFFFROM = DateTime.Parse(Request.Form["FORM_EFFFROM"]);                    
                    etys[0].FORM_HEADER = Helper.Aid.Null2Str(Request.Form["FORM_HEADER"]);
                    etys[0].FORM_WAIST = System.Text.ASCIIEncoding.UTF8.GetBytes(Request.Form["FORM_WAIST"].ToString());
                    etys[0].FORM_FOOTER = Request.Form["FORM_FOOTER"];

                    string logo64 = Helper.Aid.Null2Str(Request.Form["FORM_LOGO_Base64"]);
                    if (logo64 != "")
                        etys[0].FORM_LOGO = Convert.FromBase64String(logo64); //这里是blob类型保存logo    
                                        
                    if (string.IsNullOrEmpty(Request.Form["FORM_EFFTO"]))
                        etys[0].FORM_EFFTO = null;
                    else
                        etys[0].FORM_EFFTO = DateTime.Parse(Request.Form["FORM_EFFTO"]);
                    etys[0].FORM_PROPERTY = props[0];
                    etys[0].FORM_ATTACHURL = Request.Form["FORM_ATTACHURL"];
                    etys[0].FORM_ATTACHNAME = Request.Form["FORM_ATTACHNAME"];
                    etys[0].FORM_UPDATEBY = Helper.Aid.DbNull2Str(Session["loginname"]);
                    
                    etys[0].FORM_UPDATEDATE = DateTime.Now;
                    if (LoginSystem.GroupInfo.Approver == "1")
                        etys[0].FORM_STATUS = "A";
                    else if (etys[0].FORM_STATUS == "A")
                    {
                        etys[0].FORM_STATUS = "I";
                        //throw new Exception("Non-approver can not modify an approved eform.");
                    }
                    else
                        etys[0].FORM_STATUS = "I";

                    cnt = bz.Update(etys[0]);

                    ety = etys[0];//这里赋值给ety用来发送邮件

                }
                tx.CommitTrans();
                rsp.result = true;
                if (LoginSystem.GroupInfo.Approver != "1")
                    SendMail2Approver(ety, props);
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
                if (etys.Count>0)
                {
                    List<string> tos = new List<string>();
                    for (int i = 0; i < etys.Count; i++)
                    {
                        if (tos.Contains(etys[i].Email))
                            continue;
                        tos.Add(etys[i].Email);
                    }

                    string link = Request.Url.AbsoluteUri.Replace(Request.Url.Query,"");
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
       
        private string GenMailBody(EFormModel ety, string[] props,string link)
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
            result += "This e-mail serves as notification only, please do not reply to this email." +"<br/>";
            result += "<br/>";
            result += "HutchisonAgent System" + "<br/>";
            result += "<br/>";
            return result;
        }

        private string FlatArray(string[] arr)
        {
            if (arr.Length>0)
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
        }
    }

    public class Pager
    {
        public int pageSize { get; set; }   //
        public int count { get; set; }      //
        public int pageIdx { get; set; }    //
        public object data { get; set; }    //array        
    }
    
    public class Attachment
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class GroupInfo
    {
        public Model.Sys_Users_Group group { get; set; }
        public List<Model.Sys_Users_Group_Property> props = new List<Model.Sys_Users_Group_Property>();
    }

    public class EFormModelQuery
    {
        [DbField(true,true)]
        public int FORM_ID { get; set; }
        public string FORM_CODE { get; set; }
        public string FORM_NAME { get; set; }
        public string FORM_DESC { get; set; }
        public string FORM_LEASE { get; set; }
        public string FORM_ATTACHURL { get; set; }
        public string FORM_ATTACHNAME { get; set; }
        public DateTime FORM_EFFFROM { get; set; }
        public string FORM_HEADER { get; set; }
        public byte[] FORM_WAIST { get; set; }
        public string FORM_FOOTER { get; set; }
        public byte[] FORM_LOGO { get; set; }
        public string FORM_PROPERTY { get; set; }
        public Nullable<DateTime> FORM_EFFTO{get;set;}
        public string FORM_STATUS { get; set; }
        public string FORM_ACTIVE { get; set; }
        public int FLOW_ID { get; set; }
        public string FLOW_CODE { get; set; }
        public string FORM_CUSTOMER { get; set; }
        public string FORM_CREATEBY { get; set; }
        public string FORM_UPDATEBY { get; set; }
        public string FORM_APPROVEBY { get; set; }
        public Nullable<DateTime> FORM_CREATEDATE{ get; set; }       
        public Nullable<DateTime> FORM_APPROVEDATE { get; set; }
        public Nullable<DateTime> FORM_UPDATEDATE { get; set; }

        //public byte[] HEADER_BLOB { get; set; }
        //public byte[] FORM_WAIST_BLOB { get; set; }

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
                    return "";
            }          
        }       
        [DbField(false, false, true)]
        public string FORM_LOGO_Base64
        {
            get
            {
                if (FORM_LOGO != null)
                    return Convert.ToBase64String(FORM_LOGO);
                else
                    return "";
            }
        }
    }

    public class EFormModel
    {
        /*
         *  FORM_ID	N	NUMBER	N			表格ID（流水号）
            FORM_CODE	N	VARCHAR2(30)	N			表格编号
            FORM_NAME	N	VARCHAR2(100)	N			表格名称
            FORM_LOGO	N	BLOB	Y			表格LOGO
            FORM_TABLE	N	VARCHAR2(50)	Y			表格数据的表名
            FORM_DESC	N	VARCHAR2(2000)	Y			表格的描述
            FORM_EFFFROM	N	DATE	N			起始日期
            FORM_EFFTO	N	DATE	Y			截止日期
            FORM_ATTACHURL	N	VARCHAR2(500)	Y			附件地址
            FORM_PROPERTY	N	VARCHAR2(500)	Y			归属的Property（逗号分隔字串）
            FORM_LEASE	N	VARCHAR2(3000)	Y			归属的Lease（逗号分隔字串）
            FORM_CUSTOMER	N	VARCHAR2(3000)	Y			归属的Customer（逗号分隔字串）
            FORM_HEADER	N	VARCHAR2(3000)	Y			表格的页眉
            FORM_FOOTER	N	VARCHAR2(3000)	Y			表格的页末
            FORM_STATUS	N	CHAR(10)	N	'I'		审批状态（I：Init初始；A：Approved已审批）
            FORM_ACTIVE	N	CHAR(10)	N	'A'		有效状态（I：Inactive删除；A：Active有效）
            FORM_CREATEBY	N	VARCHAR2(50)	N			创建者
            FORM_CREATEDATE	N	DATE	N			创建日期
            FORM_UPDATEBY	N	VARCHAR2(50)	Y			修改者
            FORM_UPDATEDATE	N	DATE	Y			修改日期
            FORM_APPROVEBY	N	VARCHAR2(50)	Y			审批者
            FORM_APPROVEDATE	N	DATE	Y			审批日期

         */
        public const string TABLE_NAME = "E_Form";

        [DbField(true,true)]
        public int FORM_ID { get; set; }
        public string FORM_CODE { get; set; }
        public string FORM_NAME { get; set; }
        public string FORM_DESC { get; set; }
        public string FORM_LEASE { get; set; }
        public string FORM_ATTACHURL { get; set; }
        public string FORM_ATTACHNAME { get; set; }
        public DateTime FORM_EFFFROM { get; set; }
        public string FORM_PROPERTY { get; set; }
        public string FORM_CREATEBY { get; set; }
        public Nullable<DateTime> FORM_CREATEDATE { get; set; }
        public Nullable<DateTime> FORM_EFFTO{get;set;}
        public string FORM_STATUS { get; set; }
        public string FORM_ACTIVE { get; set; }
        public Nullable<int> FLOW_ID { get; set; }
        public string FLOW_CODE { get; set; }
        public string FORM_CUSTOMER { get; set; }
        public string FORM_UPDATEBY { get; set; }
        public string FORM_APPROVEBY { get; set;}
        public string FORM_HEADER { get; set; }
        public byte[] FORM_WAIST { get; set; }
        public string FORM_FOOTER { get; set; }
        public byte[] FORM_LOGO { get; set; }
        public Nullable<DateTime> FORM_APPROVEDATE { get; set; }
        public Nullable<DateTime> FORM_UPDATEDATE { get; set; }

        //public byte[] HEADER_BLOB { get; set; }
        //public byte[] FORM_WAIST_BLOB { get; set; }
    }

    public class UploadParas
    {
        public string fileName { get; set; }
        public string base64Data { get; set; }
    }
    public class EFormModelJson : EFormModel
    {
        public string action { get; set; }
        public string props { get; set; }
        public string FORM_LOGO_Base64 { get; set; }
        public string FORM_WAIST_HTML { get; set; }
    }
}