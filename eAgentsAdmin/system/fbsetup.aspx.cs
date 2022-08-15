using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Helper;
using PropertyOneAppWeb.Model;

namespace PropertyOneAppWeb.system
{
    public partial class fbsetup : PageBase
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

                    case "getNotis":
                        result = this.GetNotis();
                        break;

                    case "getProps":
                        result = this.GetProps();
                        break;

                    case "getUsers":
                        result = this.GetUsers();
                        break;

                    case "del":
                        int id = int.Parse(Request.Form["id"].ToString());
                        result = this.Delete(id);
                        break;
                    case "save":
                        result = this.Save();
                        break;

                    default:
                        break;
                }
                Response.Write(result);
                Response.End();
            }
        }

        private string GetUsers()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.Sys_Login_System> etys = bz.Retrieve<Model.Sys_Login_System>(x => x.Status == "A");
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

        private string GetProps()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.pone_pd_property> etys = bz.Retrieve<Model.pone_pd_property>(x => x.Active == "A");

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

        private string Delete(int id)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<system.SysMailParamsModel> etys = bz.Retrieve<system.SysMailParamsModel>(x => x.Id == id);
                bz.Delete(etys[0]);
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

        private string Save()
        {
           JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, true, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                system.SysMailParamsModel ety = null;
                int cnt = 0;
                string act = Request.Form["action"];

                if (act == "new")
                {
                    ety = new SysMailParamsModel();
                    ety.PARAM_TYPE = Request.Form["PARAM_TYPE"];
                    ety.PARAM_NAME = Request.Form["PARAM_NAME"];
                    ety.PARAM_DESC = Request.Form["PARAM_DESC"];
                    ety.PARAM_VALUE = Request.Form["PARAM_VALUE"];
                    cnt = bz.Create(ety);
                    //Log.WriteLog("new eform creation with name " + ety.FORM_NAME + " by " + ety.FORM_CREATEBY);                
                }
                else
                {                    
                    List<SysMailParamsModel> etys = bz.Retrieve<SysMailParamsModel>(x => x.Id == int.Parse(Request.Form["Id"]));
                    etys[0].PARAM_NAME = Request.Form["PARAM_NAME"];
                    etys[0].PARAM_DESC = Request.Form["PARAM_DESC"];
                    etys[0].PARAM_VALUE = Request.Form["PARAM_VALUE"];
                    cnt = bz.Update(etys[0]);

                    ety = etys[0];//这里赋值给ety用来发送邮件

                }
                tx.CommitTrans();
                rsp.result = true;
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

        private string GetNotis()
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<system.SysMailParamsModel> etys = bz.Retrieve<system.SysMailParamsModel>(x => x.PARAM_VALUE.Sql_Like("NOTIFICATION"));
                tx.CommitTrans();

                List<string> notis = new List<string>();                     
                foreach (var item in etys.GroupBy(x => x.PARAM_VALUE))
                {
                    notis.Add(item.Key);
                }
                rsp.result = notis;                
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string Search(string criteria, int pidx, int pageSize = 10)
        {
            JsonRsp rsp = new JsonRsp();
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, GlobalUtilSystem.sdb_local, false, true);
            try
            {
                Helper.DbConsole bz = new Helper.DbConsole(tx);
                List<system.SysMailParamsModel> etys = bz.Retrieve<SysMailParamsModel>(
                    x => (x.PARAM_TYPE.Sql_Like(criteria) ||
                        x.PARAM_NAME.Sql_Like(criteria) ||
                        x.PARAM_VALUE.Sql_Like(criteria)) &&
                        x.PARAM_TYPE.Sql_Like("NOTIFICATION"));
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
}