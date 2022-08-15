using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PropertyOneAppWeb.Commom;
using PropertyOneAppWeb.Model;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace PropertyOneAppWeb.Web
{
    public partial class StatementViewer : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = Request.Form["action"];
            string downloadAction = Request.Form["downloadpdf"];

            string result = "";
            try
            {
                if (!string.IsNullOrEmpty(action))
                {
                    if (action == "loadstatement")
                    {
                        ApiUtil api = new ApiUtil();
                        string json = api.Api_GetStatementDetail(Session["leasenumber"].ToString(), Session["statementnum"].ToString());
                        JsonApi.JsonApi_GetStatementDetail_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetStatementDetail_Res>(json);

                        if (res.result == "100")
                        {
                            result = json;
                        }
                        else
                        {
                            throw new Exception("Get statement detail error: " + res.message);
                        }
                    }
                    else if (action == "genpdf")
                    {
                        ApiUtil api = new ApiUtil();
                        string json = api.Api_GetStatement(Session["leasenumber"].ToString(), "", "", Session["statementnum"].ToString());
                        JsonApi.JsonApi_GetStatement_Res res = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetStatement_Res>(json);

                        if (res.result == "100")
                        {
                            result = "ok";
                        }
                        else
                        {
                            throw new Exception("Get statement error: " + res.message);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(downloadAction))
                {
                    string[] pdflist = new string[2];
                    pdflist[0] = Session["pdffilename"].ToString();
                    pdflist[1] = "merge.pdf";
                    string newFileName = mergePDFFiles(pdflist);
                    Preview(Page, newFileName);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Log.WriteLog(ex.Message + ex.StackTrace);
            }
            finally
            {
                if (!string.IsNullOrEmpty(result))
                {
                    Response.Write(result);
                    Response.End();
                }
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

        /// <summary>
        /// 合并PDF,从接口返回的PDF文件与固定的meger.pdf文件合并。
        /// </summary>
        /// 该merge.pdf文件是一个固定不变的说明文件，附在每个账单pdf末尾。
        private string mergePDFFiles(string[] fileList)
        {
            Document document = new Document();
            ArrayList readers = new ArrayList();

            try
            {
                string mergeFileName = GlobalUtil.GenFileName() + ".pdf";
                string mergePath = "\\Pdf\\Merge\\";
                string outMergeFile = Server.MapPath(mergePath + mergeFileName);
                PdfReader reader;
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outMergeFile, FileMode.Create));
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage newPage;

                for (int i = 0; i < fileList.Length; i++)
                {
                    string newFileName = GlobalUtil.GenFileName() + i.ToString() + ".pdf";
                    File.Copy(Server.MapPath("\\Pdf\\" + fileList[i]), Server.MapPath(mergePath + newFileName));
                    
                    reader = new PdfReader(Server.MapPath(mergePath + newFileName));
                    readers.Add(reader);
                    int iPageNum = reader.NumberOfPages;
                    for (int j = 1; j <= iPageNum; j++)
                    {
                        document.NewPage();
                        newPage = writer.GetImportedPage(reader, j);
                        cb.AddTemplate(newPage, 0, 0);
                    }
                }

                return mergePath + mergeFileName;
            }
            catch(Exception ex)
            {
                Log.WriteLog(ex.Message + ex.StackTrace);
                throw;
            }
            finally
            {
                document.Close();
                document.Dispose();
                foreach (PdfReader reader in readers)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
    }
}