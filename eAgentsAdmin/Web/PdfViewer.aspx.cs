using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PropertyOneAppWeb.Web
{
    public partial class PdfViewer : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            string action = Request.Form["action"];
            if (action != null && action != "")
            {
                if (action == "loadpdf") 
                {
                    Commom.ApiUtil api = new Commom.ApiUtil();
                    string json = api.Api_GetStatement(Session["leasenumber"].ToString(), "", "", Session["StatementNum"].ToString());
                    Preview(Page, "\\Pdf\\" + Session["pdffilename"].ToString());
                }
            }
             */
            if (!IsPostBack)
            {
                Commom.ApiUtil api = new Commom.ApiUtil();
                string json = api.Api_GetStatement(Session["leasenumber"].ToString(), "", "", Session["statementnum"].ToString());
                Preview(Page, "\\Pdf\\" + Session["pdffilename"].ToString());
            }
        }

        public static void Preview(System.Web.UI.Page p, string inFilePath)
        {
            string fileName = inFilePath.Substring(inFilePath.LastIndexOf('\\') + 1);
            p.Response.ContentType = "Application/pdf";
            p.Response.ContentEncoding = System.Text.Encoding.UTF8;  //保持和文件的编码格式一致
            p.Response.AddHeader("content-disposition", "filename=" + fileName);
            p.Response.WriteFile(inFilePath);
            p.Response.End();
        }
    }
}