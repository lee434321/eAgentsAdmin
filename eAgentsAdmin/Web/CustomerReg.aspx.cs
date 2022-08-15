using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using PropertyOneAppWeb.Helper;
namespace PropertyOneAppWeb.Web
{
    public partial class CustomerReg : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string leasenum = Request.QueryString["leasenum"];
            this.lblLeasenum.Text = leasenum;
        }

        protected void tbxOK_Click(object sender, EventArgs e)
        {
            string result = "";          
            Helper.DbTx tx = new Helper.DbTx(DbVendor.Oracle, Commom.GlobalUtilSystem.sdb_local, true, true);
            try
            {
                string loginName = this.lblLeasenum.Text.Trim();
                string pwd = this.tbxPwd.Text.Trim();               
                /// -- start --
                Helper.DbConsole bz = new Helper.DbConsole(tx);                
                List<Model.Sys_Login_Account> etys = bz.Retrieve<Model.Sys_Login_Account>(x => x.LoginName == loginName);//ignore.
                if (etys.Count<=0)                
                     throw new Exception("LoginName not found!");

                etys[0].Password = pwd;
                etys[0].CU_Status = Commom.CONST.CU_STATUS_ACTIVE;
                int cnt = bz.Update(etys[0]);
                if (cnt > 0)
                    result = "success";
                else
                    result = "update failure";
                tx.CommitTrans();
                /// --  end  --
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                result = err.Message;
            }
        }
    }
}