using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;

namespace PropertyOneAppWeb.Web
{
    public partial class OnlinepayGenDO : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
                try
                {
                    pps p = new pps();
                    string refno = p.GenRefNo();
                    string payamount = Session["payamount"].ToString();
                    Session["ppsreferenceno"] = refno;
                    string res = p.GenPaymentRequest(refno, payamount, "", "ENGLISH", "");
                    if (res != null && res != "")
                    {
                        Response.Write(res);
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
        }
    }
}