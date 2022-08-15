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
    public partial class NoticeMaintenance : PageBase
    {
        string url = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["loginname"] == null)
            {
                Response.Redirect("../LoginSystem.aspx");
            }
            url = Request.Url.AbsoluteUri;
            //校验用户权限
            OracleEnquiry oraEnquiry = new OracleEnquiry();
            if (Session["loginname"] == null || !oraEnquiry.CheckUserAuth(Session["loginname"].ToString(), "11"))
            {
                Response.Redirect("../LoginSystem.aspx");
            }

            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "search")
                {
                    try
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);
                        string searchPhrase = Request.Form["searchPhrase"];

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_LOGINNAME", Session["loginname"].ToString());
                        if (searchPhrase != null && searchPhrase != "")
                        {
                            oa.AddParameter("P_SEARCH_PHRASE", searchPhrase);
                        }
                        else
                        {
                            oa.AddParameter("P_SEARCH_PHRASE", "");
                        }
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelectNotice = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_NOTICE_BY_LOGINNAME");
                        
                        /// -- start --
                        //dtSelectNotice.Columns.Add("attach_url_encoded", typeof(string));
                        //for (int j = 0; j < dtSelectNotice.Rows.Count; j++)
                        //{
                        //    DataRow dr = dtSelectNotice.Rows[j];
                        //    dr["attach_url_encoded"] = HttpUtility.HtmlEncode(dr["attach_url"].ToString());
                        //}
                        /// --  end  --
                        
                        /// -- start -- 这里要过滤当前用户的groupid权限                                               
                        List<Model.Sys_Users_Group_Property> etys = this.GetGroupInfo(int.Parse(Session["groupid"].ToString())); //tested
                        string filterStr = "(";
                        for (int i = 0; i < etys.Count; i++)                        
                            filterStr += "'" + etys[i].PropertyCode + "',";                        
                        filterStr = filterStr.Substring(0, filterStr.Length - 1);
                        filterStr += ")";

                        DataTable dt2 = dtSelectNotice.Clone();
                        DataRow[] drs = dtSelectNotice.Select("property_code in " + filterStr);
                        if (drs.Length > 0)
                        {
                            for (int j = 0; j < drs.Length; j++)                            
                                dt2.ImportRow(drs[j]);
                        }
                        else
                        {
                            dt2 = dtSelectNotice;
                        }
                        
                        /// --  end  --

                        if (dtSelectNotice != null)
                        {
                            
                            /// 这里需要做个日期转换。从数据库里来时已经变成"dd-mm-yyyy"格式的日期，只能用在en_GB的情况，不能用于zh-CN的情况，直接转换会报错
                            /// 故在zh-CN时要变为"yyyy-mm-dd" 
                            for (int j = 0; j < dt2.Rows.Count; j++)
                            {
                                string[] t = dt2.Rows[j]["startdate"].ToString().Split('-');
                                string o = t[2] + "-" + t[1] + "-" + t[0];
                                dt2.Rows[j]["startdate"] = o;
                                t = dt2.Rows[j]["enddate"].ToString().Split('-');
                                o = t[2] + "-" + t[1] + "-" + t[0];
                                dt2.Rows[j]["enddate"] = o;
                            }
                            
                            string json = GlobalUtil.GenBootGridSystem(dt2, current, rowCount);
                            Response.Write(json);
                            Response.End();
                        }
                        else
                        {
                            Response.Write(GlobalUtil.bootErrorStr);
                            Response.End();
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write(GlobalUtil.bootErrorStr);
                        Response.End();
                    }
                }
                else if (action=="getdefaultimg")
                {
                    JsonRsp rsp = new JsonRsp();
                    try
                    {
                        /// 1.获取类型
                        /// 2.找该类型下的所有图片文件
                        /// 3.生成文件名及url返回
                        string type = Request.Form["imgType"];
                        string ext = "";
                        if (type == "S")
                        {
                            ext = "notice";
                        }
                        else if (type == "P")
                        {
                            ext = "Event";
                        }
                        else if (type=="C")
                        {
                            ext = "Carpark";                          
                        }
                        else if (type=="V")
                        {
                            ext = "Service";
                        }

                        string dir = Request.MapPath(".");
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir + @"\notice_default_img");
                        System.IO.FileInfo[] fis = di.GetFiles(ext + "*.*");
                        List<ImageUrl> ius = new List<ImageUrl>();
                        for (int i = 0; i < fis.Length; i++)
                        {
                            System.Drawing.Image img = System.Drawing.Image.FromFile(fis[i].FullName);                           
                            ImageUrl iu = new ImageUrl();
                            iu.name = fis[i].Name;
                            iu.url = "notice_default_img/" + iu.name;
                            iu.height = img.Height;
                            iu.width = img.Width;
                            ius.Add(iu);
                        }
                        rsp.result = ius;
                        string json = JsonConvert.SerializeObject(rsp);
                        Response.Write(json);
                        Response.End();

                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch (Exception err)
                    {
                        rsp.err_code = -99;
                        rsp.err_msg = err.Message;
                        string json = JsonConvert.SerializeObject(rsp);
                        Response.Write(json);
                    }
                }
                else if (action == "upload")
                {
                    /*
                      +    URL 中+号表示空格                      %2B   
                      空格 URL中的空格可以用+号或者编码           %20 
                      /   分隔目录和子目录                        %2F     
                      ?    分隔实际的URL和参数                    %3F     
                      %    指定特殊字符                           %25     
                      #    表示书签                               %23     
                      &    URL 中指定的参数间的分隔符             %26     
                      =    URL 中指定参数的值                     %3D
                     */
                    string saveFullPath = "";
                    Resjson rj = new Resjson();
                    try
                    {
                        /// -- start -- 上传文件
                        /// 1. 获取文件                    
                        /// 2. 保存本地                    
                        /// 3. 写入路径到数据库
                        string fnPath = Request.Files[0].FileName; //获取文件完整路径
                        string[] s = fnPath.Split('\\');    //分割成数组
                        string fn = s[s.Length - 1];        //拿到文件名(最后一个)
                        System.IO.FileInfo fi = new System.IO.FileInfo(Request.Files[0].FileName); // 打开文件对象
                        string fnp = fi.Name.Replace(fi.Extension, "_" + Helper.Aid.Null2Str(Helper.Aid.getTimestamp() / 1000) + fi.Extension);//真实保存的文件名
                        string p = Request.PhysicalApplicationPath;
                        string rp = @"upload\";
                        if (System.IO.Directory.Exists(p + rp)) System.IO.Directory.CreateDirectory(p + rp); // 创建路径upload
                        p += rp + fnp;

                        Request.Files[0].SaveAs(p);
                        rj.data = "ok";
                        string extension = fi.Extension.ToUpper();

                        if (Request.Files["file"].ContentLength > 5242880)              /// 大于5M不能上传
                            throw new Exception("the size of attachment can not exceed 5M！ ");
                        if (fi.Extension.ToUpper() == "EXE" || fi.Extension.ToUpper() == "COM") /// 可执行文件不能上传
                            throw new Exception("attachment file can not be executable!");

                        if (Request.Form["isAttachment"] != "Y") //如果是图片
                        {
                            if (extension.Contains("JPEG") || extension.Contains("JPG") || extension.Contains("PNG") || extension.Contains("BMP"))
                            {
                                rj.imgurl = "/Upload/" + fnp; //web虚拟路径
                                rj.imgname = fn;
                            }
                            else
                                throw new Exception("Image Format is not support!");
                        }
                        else
                        {
                            rj.attachurl = "/Upload/" + fnp; //web虚拟路径
                            rj.attachname = fn;
                        }
                        /// --  end  --      

                        /// -- start -- base64上传
                        System.IO.Stream sm = Request.Files[0].InputStream;
                        byte[] buffer = new byte[sm.Length];
                        sm.Read(buffer, 0, buffer.Length);
                        string binStrData = Convert.ToBase64String(buffer);
                        UploadParas data = new UploadParas();
                        data.fileName = fnp;
                        data.base64Data = binStrData;
                        if (Request.Form["isAttachment"] == "Y")
                            UploadByBase64(data, true);
                        else
                            UploadByBase64(data);
                        /// --  end  --                      
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(ex.Message + ex.StackTrace);
                        rj.data = ex.Message + " [" + saveFullPath + "]";
                    }
                    finally
                    {
                        NetworkConnection.Disconnect("X:");
                        string json = JsonConvert.SerializeObject(rj);
                        Response.Write(json);
                        Response.End();
                    }
                }
                else if (action=="newnotice")
                {
                    
                }
                else if (action == "addnotice") // 新增notice，如果有多个propertycode则会创建多条，因为每条notice只允许有一个propertycode
                {
                    string saveinfo = Request.Form["saveinfo"];
                    string[] arraySaveInfo = saveinfo.Split('|');

                    string title = Request.Form["title"];
                    string startdate = Request.Form["startdate"];
                    startdate = GlobalUtil.ConvertDateType(startdate, "dd-MM-yyyy", "yyyy-MM-dd");
                    string enddate = Request.Form["enddate"];
                    enddate = GlobalUtil.ConvertDateType(enddate, "dd-MM-yyyy", "yyyy-MM-dd");
                    string content = Request.Form["content"];
                    string imgurl = Request.Form["imgurl"]; //
                    string imgname = Request.Form["imgname"];
                    string arrayproperty = Request.Form["arrayproperty"];
                    string type = Request.Form["type"];
                    string attachurl = Request.Form["attachurl"];

                    try
                    {
                        /// -- start -- use default image   
                        string defaultImgFileName = Helper.Aid.DbNull2Str(Request.Form["defaultImg"]);
                        if (defaultImgFileName != "") // defaultImg参数不为空说明使用默认的图片文件
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(defaultImgFileName);
                            string leftName = fi.Name.Substring(0, fi.Name.IndexOf(fi.Extension));
                            string src = "", fn = "", dst = this.MapPath("../Upload");
                            string cpath = this.MapPath(".");

                            src = cpath + @"\notice_default_img\" + fi.Name;
                            fn = leftName + "_" + (Helper.Aid.getTimestamp() / 1000).ToString() + fi.Extension;                            
                            System.IO.File.Copy(src, dst + @"\" + fn);

                            imgurl = "/upload/" + fn;

                            /// -- start -- base64上传
                            System.IO.Stream sm = System.IO.File.OpenRead(dst + @"\" + fn);
                            byte[] buffer = new byte[sm.Length];
                            sm.Read(buffer, 0, buffer.Length);
                            string binStrData = Convert.ToBase64String(buffer);
                            UploadParas data = new UploadParas();
                            data.fileName = fn;
                            data.base64Data = binStrData;
                            UploadByBase64(data);
                            /// --  end  --
                        }
                        /// --  end  --
                    
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_ATTACH_URL", attachurl);
                        oa.AddParameter("P_PROPERTYINFO", arrayproperty);
                        oa.AddParameter("P_TYPE", type);
                        oa.AddParameter("P_TITLE", title);
                        oa.AddParameter("P_DETAIL", content);
                        oa.AddParameter("P_IMGURLSMALL", "");

                        int posi=imgurl.IndexOf("?");
                        if (posi > 0)
                            imgurl = imgurl.Substring(0, posi );
                        
                        oa.AddParameter("P_IMGURLLARGE", imgurl);
                        oa.AddParameter("P_STATUS", "A");
                        oa.AddParameter("P_APPROVE", "I");
                        oa.AddParameter("P_CREATEBY", Session["loginname"].ToString());
                        oa.AddParameter("P_STARTDATE", Convert.ToDateTime(startdate));
                        oa.AddParameter("P_ENDDATE", Convert.ToDateTime(enddate));
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string result = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.INSERT_NOTICE").ToString();
                        if (result != null && result == "ok")
                        {
                            Log.WriteLog("新增Notice成功~！");
                            /// -- start -- 向有关的approve组成员发送邮件
                            //1.提交notice时，获取该用户的propertycode列表
                            //2.根据propertycode列表获取相应的approve组
                            //3.向该组成员发送邮件
                            string[] props = arrayproperty.Split(',');
                            this.SendEmail(props, title, Session["loginname"].ToString(), -1, content, Session["loginname"].ToString());
                            /// --  end  --                            
                            Response.Write("ok");
                            Response.End();
                        }
                        else
                        {
                            Response.Write("fail");
                            Response.End();
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write("fail");
                        Response.End();
                    }
                }
                else if (action == "editnotice")
                {
                    string noticeid = Request.Form["noticeid"];
                    string title = Request.Form["title"];
                    string startdate = Request.Form["startdate"];
                    startdate = GlobalUtil.ConvertDateType(startdate, "dd-MM-yyyy", "yyyy-MM-dd");
                    string enddate = Request.Form["enddate"];
                    enddate = GlobalUtil.ConvertDateType(enddate, "dd-MM-yyyy", "yyyy-MM-dd");
                    string content = Request.Form["content"];
                    string imgurl = Request.Form["imgurl"];
                    string imgname = Request.Form["imgname"];

                    /// -- start -- #19172
                    string type = Request.Form["type"];
                    string attachurl = Request.Form["attachurl"];
                    /// --  end  --
                    try
                    {
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_NOTICEID", noticeid);
                        oa.AddParameter("P_TITLE", title);
                        oa.AddParameter("P_STARTDATE", Convert.ToDateTime(startdate));
                        oa.AddParameter("P_ENDDATE", Convert.ToDateTime(enddate));
                        oa.AddParameter("P_DETAIL", content);

                        int posi = imgurl.IndexOf("?");
                        if (posi > 0)
                            imgurl = imgurl.Substring(0, posi );

                        oa.AddParameter("P_IMGURLLARGE", imgurl.Replace("..", ""));
                        oa.AddParameter("P_APPROVE", "I");
                        oa.AddParameter("P_TYPE", type);
                        oa.AddParameter("P_ATTACH_URL", attachurl);
                        oa.AddParameter("P_UPDATEBY", Session["loginname"]);
                        oa.AddParameter("P_UPDATEDATE", DateTime.Now);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string result = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.UPDATE_NOTICE").ToString();
                        
                        if (result != null && result == "1")
                        {
                            Log.WriteLog("编辑Notice成功！");
                            /// -- start -- 向有关的approve组成员发送邮件              
                            this.SendEmail(null, title, Session["loginname"].ToString(), int.Parse(noticeid), content, Session["loginname"].ToString());
                            /// --  end  --
                            Response.Write("ok");
                            Response.End();
                        }
                        else
                        {
                            Response.Write("fail");
                            Response.End();
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write("fail");
                        Response.End();
                    }
                }
                else if (action == "deletenotice")
                {
                    string noticeid = Request.Form["noticeid"];

                    try
                    {
                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_NOTICEID", noticeid);
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.VarChar, ParameterDirection.ReturnValue, 100);
                        string result = oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.DELETE_NOTICE").ToString();

                        if (result != null && result == "ok")
                        {
                            Response.Write("ok");
                            Response.End();
                        }
                        else
                        {
                            Response.Write("fail");
                            Response.End();
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write("fail");
                        Response.End();
                    }
                }
                else if (action == "searchproperty") //查找所有property
                {

                    try
                    {
                        int current = Int32.Parse(Request.Form["current"]);
                        int rowCount = Int32.Parse(Request.Form["rowCount"]);

                        Oracle9i oa = new Oracle9i(GlobalUtilSystem.sdb_local);
                        oa.AddParameter("P_LOGINNAME", Session["loginname"].ToString());
                        oa.AddParameter("P_RETURN", System.Data.OracleClient.OracleType.Cursor, ParameterDirection.ReturnValue);
                        DataTable dtSelect = (DataTable)oa.ExecuteStoredProcedure("PKG_SYS_FUNCTIONS.SELECT_PROPERTY_BY_ID");

                        /// -- start -- 根据session里的groupid过滤properties
                        DataTable dt2 = Bizhub.FilterPropertyByGroup(int.Parse(Session["groupid"].ToString()), dtSelect, "Property_Code", "PropertyCode");
                        /// --  end  --

                        if (dtSelect != null)
                        {
                            string json = GlobalUtil.GenBootGridSystem(dt2, current, rowCount);
                            Response.Write(json);
                            Response.End();
                        }
                        else
                        {
                            Response.Write(GlobalUtil.bootErrorStr);
                            Response.End();
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        //do nothing
                    }
                    catch
                    {
                        Response.Write(GlobalUtil.bootErrorStr);
                        Response.End();
                    }
                }
            }
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

                Log.WriteLog("上传至transfer中间层:" + sUrl);

            }
            catch (Exception err)
            {
                Log.WriteLog("上传失败:" + err.Message + err.StackTrace);
                throw err;
            }
            /// --  end  --
        }

        /// <summary>
        /// 新增或修改notice时向审批人员发送邮件
        /// </summary>
        /// <param name="props"></param>
        /// <param name="title"></param>
        /// <param name="user"></param>
        /// <param name="id"></param>
        private void SendEmail(string[] props, string title, string user, int id = -1, string content = "", string createby = "")
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole biz = new Helper.DbConsole(tx);
                List<Model.Sys_Login_System> etysa = biz.Retrieve<Model.Sys_Login_System>(x => x.LoginName == user); //tested
                if (etysa.Count == 0) return;

                /// 取分组名                
                List<Model.Sys_Users_Group> ety4grp = biz.Retrieve<Model.Sys_Users_Group>(x => x.GroupId == int.Parse(Session["groupid"].ToString()));//tested
                if (ety4grp.Count == 0) return;

                string deptStr = "";
                if (ety4grp[0].Dept != "") //如果新的分组策略有值（部门字段有值）
                {
                    deptStr = ety4grp[0].Dept;
                }
                else
                {
                    string[] tmparr = ety4grp[0].GroupName.Split(' ');
                    if (tmparr.Length <= 0) return;
                    deptStr = tmparr[0];
                }

                /// -- start --
                string sql = "select *  from sys_login_system";
                sql += " where userid in (select distinct userid";
                sql += "  from sys_backenduser_role_v";
                sql += " where propertycode in";
                sql += "       (select distinct propertycode";
                sql += "          from t_notice";
                sql += "         where 1 = 1";
                sql += "           and title = '" + title.Replace("'", "''") + "'";
                if (id > 0)
                    sql += " and noticeid=" + id.ToString();
                sql += " and status <> 'I'";
                
                if (props != null)
                {
                    string tstr = "";
                    for (int i = 0; i < props.Length; i++)
                    {
                        tstr += "'" + props[i].Trim() + "',";
                    }

                    if (tstr.Length > 0)
                    {
                        tstr = tstr.Substring(0, tstr.Length - 1);
                        sql += " and propertycode in (" + tstr + ")";
                    }
                }

                sql += ")";
                sql += "    and approver = '1'";
                if (ety4grp[0].Dept != "")
                {
                    sql += " and dept = '" + ety4grp[0].Dept + "'";
                }
                else
                {
                    sql += " and groupname like '%" + deptStr + "%'";// 这里是指具有审批权限的组
                }
                sql += ")";
                /// --  end  --               

                List<Model.Sys_Login_System> etys0 = biz.Retrieve<Model.Sys_Login_System>(sql);

                ExchangeMail em = new ExchangeMail();
                string body = "System user " + user + " submit a notice[title:" + title + "] to you,please approve!";
                /// -- start --
                string link = string.Format("<a href='{0}'>link</a>", url.Replace("NoticeMaintenance", "noticeapprove") + "?title=" + title); //http://localhost:58175/system/NoticeMaintenance.aspx
                body = GenEmailBody(title, content, createby, link);
                /// --  end  --
                HashSet<string> emailsent = new HashSet<string>(); //用于判断是否已经发送过邮件                
                string dev = System.Configuration.ConfigurationManager.AppSettings["dev"];
                
                for (int i = 0; i < etys0.Count; i++)
                {
                    if (emailsent.Contains(etys0[i].Email))
                        continue;

                    if (etys0[i].LoginName != Session["loginname"].ToString()) //如果审批人不是当前登录人，则发送邮件
                    {
                        if (dev == "0")
                        {
                            em.SendExchangeMail(etys0[i].Email, "Approval request of Notice / Promotional Event on HutchisonAgent", body);
                        }
                        else
                        {   //测试用途
                            if (!emailsent.Contains("liwei@hwpg.com"))
                                em.SendExchangeMail("liwei@hwpg.com", "Approval request of Notice / Promotional Event on HutchisonAgent", body);
                            emailsent.Add("liwei@hwpg.com");
                        }
                        emailsent.Add(etys0[i].Email);
                        Log.WriteLog("向审批者" + etys0[i].Email + "发送邮件~！");
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
                throw;
            }
        }

        private List<Model.Sys_Users_Group_Property> GetGroupInfo(int groupid)
        {
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<Model.Sys_Users_Group_Property> etys = bz.Retrieve<Model.Sys_Users_Group_Property>(x => x.GroupId == groupid);//tested
                tx.CommitTrans();
                return etys;
            }
            catch (Exception)
            {
                tx.AbortTrans();                
                throw;
            }
        }

        private string GenEmailBody(string title, string content, string createby, string link)
        {
            string body = " You have an approval request of Notice / Promotional Event on HutchisonAgent." + "<br/>";
            body += "<br/>";
            body += "The following are the Notice / Promotional Event info:" + "<br/>";

            body += string.Format("Subject :{0}", title) + "<br/>";
            body += string.Format("Content : {0}", content) + "<br/>";
            body += string.Format("Created by : {0}", createby) + "<br/>";
            body += "<br/>";
            body += string.Format("Please check it by the following {0}", link) + "<br/>";
            body += "<br/>";
            body += "This e-mail serves as notification only, please do not reply to this email." + "<br/>";
            body += "<br/>";
            body += "HutchisonAgent System";
            body += "<br/>";

            return body;
        }

        public class Resjson
        {
            public string data { get; set; }
            public string imgurl { get; set; }
            public string imgname { get; set; }
            public string attachurl { get; set; }
            public string attachname { get; set; }
        }

        public class UploadParas
        {
            public string fileName { get; set; }
            public string base64Data { get; set; }
        }

        public class ImageUrl
        {
            public string name { get; set; }
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }
    }
}