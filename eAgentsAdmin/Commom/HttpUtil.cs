using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Security;

namespace PropertyOneAppWeb.Commom
{
    public class HttpUtil
    {
        /// <summary>  
        /// GET请求与获取结果  
        /// </summary>  
        public static string HttpRequestGet(string Url, string postDataStr)
        {
            //添加代理
            WebProxy proxy = new WebProxy();
            proxy = WebProxy.GetDefaultProxy();
            proxy.UseDefaultCredentials = true;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            //添加代理
            request.Proxy = proxy;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>  
        /// Post请求与获取结果  
        /// </summary>  
        public static string HttpRequestPost(string Url, string postDataStr,string contentTpye="application/x-www-form-urlencoded")
        {
           
            //正式用代理
            WebProxy proxy = new WebProxy();
            proxy = WebProxy.GetDefaultProxy();
            proxy.UseDefaultCredentials = true;
            

            /*
            //测试用代理
            WebProxy proxy = new WebProxy();
            proxy.Credentials = new NetworkCredential("tonny.peng", "5tgbnhy^");
            */

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //request.Proxy = proxy;  //测试用代理，正式需删除
            request.Method = "POST";
            request.ContentType = contentTpye;
            //request.ContentLength = postDataStr.Length;
            request.SendChunked = true;
            //StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8);
            writer.Write(postDataStr);
            writer.Flush();
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        ///<summary>
        /// MD5加密
        /// </summary>
        /// <param name="toCryString">被加密字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5(string toCryString)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(toCryString, "MD5").ToUpper();
        }

        /// <summary>
        /// 推送通知
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="regids"></param>
        public static string PushNotification(string url,string title,string msg,params String[] regids)
        {
            try
            {
                PushNotification notification = new PushNotification();
                notification.title = title;
                notification.message = msg;
                notification.registrationIds = regids;
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(notification);
                EncryptUtil encry = new EncryptUtil();
                data = encry.Encrypt(data);
                string param = HttpUtility.UrlEncode("data") + "=" + HttpUtility.UrlEncode(data);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(param);
                //
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                System.IO.Stream writer = request.GetRequestStream();
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream stream = response.GetResponseStream();
                /// -- start --
                System.IO.StreamReader sr = new StreamReader(stream);
                string responseStr = sr.ReadToEnd();
                /// --  end  --
                //System.Xml.XmlTextReader Reader = new System.Xml.XmlTextReader(stream);
                //Reader.MoveToContent();
                //string result = Reader.ReadInnerXml();
                //Reader.Close();
                stream.Close();
                return responseStr;
            }
            catch (Exception err)
            {
                Log.WriteLog(err.Message);
                return "";
            }
        }

        public static void SendHPGApp()
        { }
    }

    public class PushNotification
    {
        public string[] registrationIds { get; set; }
        public string title { get; set; }
        public string message { get; set; }
    }
}