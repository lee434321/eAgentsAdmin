using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyOneAppWeb.Model;

namespace PropertyOneAppWeb.Commom
{
    public class ApiUtil
    {
        string apiUrl = ConfigurationManager.AppSettings["api_url"].ToString();  //接口链接地址

        /// <summary>
        /// 接受接口返回的json
        /// </summary>
        public string Api_GetResponseJson(string requestJson, string act)
        {
            try
            {
                string head = GenRequestHead(requestJson);

                //正式使用
                string res = Commom.HttpUtil.HttpRequestPost(apiUrl + "act=" + act, head);
                Log.WriteLog("send request：" + apiUrl + "act=" + act + ";head=" + head);
                string analyRes = AnalyseResponse(res);
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.1	用户登录校验
        /// </summary>
        public string Api_LoginCheck(string username, string password, string logintype)
        {
            try
            {
                JsonApi.JsonApi_LoginCheck jsonData = new JsonApi.JsonApi_LoginCheck();
                jsonData.username = username;
                jsonData.password = password;
                jsonData.logintype = logintype;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "logincheck");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.2	用户忘记密码
        /// </summary>
        public string Api_ForgetPsw(string userid, string logintype)
        {
            try
            {
                JsonApi.JsonApi_ForgetPsw jsonData = new JsonApi.JsonApi_ForgetPsw();
                jsonData.userid = userid;
                jsonData.logintype = logintype;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "forgetpsw");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.3	更改密码
        /// </summary>
        public string Api_ChangePsw(string userid, string logintype, string oldpsw, string newpsw)
        {
            try
            {
                JsonApi.JsonApi_ChangePsw jsonData = new JsonApi.JsonApi_ChangePsw();
                jsonData.userid = userid;
                jsonData.logintype = logintype;
                jsonData.oldpsw = oldpsw;
                jsonData.newpsw = newpsw;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "changepsw");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.4	获取待支付账单信息
        /// </summary>
        public string Api_GetOnlinePayInfo(string leasenum)
        {
            try
            {
                JsonApi.JsonApi_GetOnlinepayInfo jsonData = new JsonApi.JsonApi_GetOnlinepayInfo();
                jsonData.leasenum = leasenum;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getonlinepayinfo");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.5	通知后台支付结果
        /// </summary>
        public string Api_SendOnlinepayResult(
            string leasenum,
            string actualamount,
            string actualpaytype,
            string receivecompanycode,
            string currencycode,
            string bankaccount,
            string customercode,
            string actualpaydate,
            string status,
            List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo> list)
        {
            try
            {
                JsonApi.JsonApi_OnlinepayResult jsonData = new JsonApi.JsonApi_OnlinepayResult();
                jsonData.leasenum = leasenum;
                jsonData.actualamount = actualamount;
                jsonData.actualpaytype = actualpaytype;
                jsonData.receivecompanycode = receivecompanycode;
                jsonData.currencycode = currencycode;
                jsonData.bankaccount = bankaccount;
                jsonData.customercode = customercode;
                jsonData.actualpaydate = actualpaydate;
                jsonData.status = status;
                jsonData.actualpayinfo = list;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "onlinepayresult");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        public string Api_SendOnlinepayResult(
            string leasenum, 
            string actualamount, 
            string actualpaytype, 
            string receivecompanycode, 
            string currencycode, 
            string bankaccount, 
            string customercode, 
            string actualpaydate, 
            string status,
            string ppsreferenceno,
            string ppsmerchantid,
            string ppstxcode,
            string ppsamount,
            string ppsopcode,
            string ppspayfor,
            string ppslocale,
            string ppsuserdata,
            string ppssiteid,
            string ppsstatuscode,
            string ppsresponsecode,
            string ppsbankaccount,
            string ppsvaluedate,
            string ppstxdate,
            string ppspostid,
            string ppsisn,
            string ppswholemessage,
            List<JsonApi.JsonApi_OnlinepayResult_actualpayinfo> list)
        {
            try 
            {
                JsonApi.JsonApi_OnlinepayResult jsonData = new JsonApi.JsonApi_OnlinepayResult();
                jsonData.leasenum = leasenum;
                jsonData.actualamount = actualamount;
                jsonData.actualpaytype = actualpaytype;
                jsonData.receivecompanycode = receivecompanycode;
                jsonData.currencycode = currencycode;
                jsonData.bankaccount = bankaccount;
                jsonData.customercode = customercode;
                jsonData.actualpaydate = actualpaydate;
                jsonData.status = status;
                jsonData.ppsreferenceno = ppsreferenceno;
                jsonData.ppsmerchantid = ppsmerchantid;
                jsonData.ppstxcode = ppstxcode;
                jsonData.ppsamount = ppsamount;
                jsonData.ppsopcode = ppsopcode;
                jsonData.ppspayfor = ppspayfor;
                jsonData.ppslocale = ppslocale;
                jsonData.ppsuserdata = ppsuserdata;
                jsonData.ppssiteid = ppssiteid;
                jsonData.ppsstatuscode = ppsstatuscode;
                jsonData.ppsresponsecode = ppsresponsecode;
                jsonData.ppsbankaccount = ppsbankaccount;
                jsonData.ppsvaluedate = ppsvaluedate;
                jsonData.ppstxdate = ppstxdate;
                jsonData.ppspostid = ppspostid;
                jsonData.ppsisn = ppsisn;
                jsonData.ppswholemessage = ppswholemessage;
                jsonData.actualpayinfo = list;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "onlinepayresult");
                return analyRes;
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// 3.6	获取已支付账单历史记录
        /// </summary>
        public string Api_GetPayHistory(string leasenum, string startdate, string enddate)
        {
            try {
                JsonApi.JsonApi_OnlinepayHistory jsonData = new JsonApi.JsonApi_OnlinepayHistory();
                jsonData.leasenum = leasenum;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "onlinepayhistory");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.7	获取对账单
        /// </summary>
        public string Api_GetStatement(string leasenum, string startdate, string enddate, string statementnum)
        {
            try
            {
                JsonApi.JsonApi_GetStatement jsonData = new JsonApi.JsonApi_GetStatement();
                jsonData.leasenum = leasenum;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                jsonData.statementnum = statementnum;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getstatement");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.8	A-租约用户获取Feedback信息
        /// </summary>
        public string Api_GetFeedback(string leasenum, string startdate, string enddate, string type, string status)
        {
            try
            {
                JsonApi.JsonApi_GetFeedback jsonData = new JsonApi.JsonApi_GetFeedback();
                jsonData.leasenum = leasenum;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                jsonData.type = type;
                jsonData.status = status;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getfeedback");
                return analyRes;
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// 3.8 B-租约用户获取Feedback的回复信息
        /// </summary>
        public string Api_GetFeedbackRes(string leasenum, string feedbackid)
        {
            try
            {
                JsonApi.JsonApi_GetFeedbackRes jsonData = new JsonApi.JsonApi_GetFeedbackRes();
                jsonData.leasenum = leasenum;
                jsonData.feedbackid = feedbackid;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getfeedbackres");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.8 C-系统管理员获取Feedback信息 
        /// </summary>
        public string Api_GetFeedbackSystem(string userid, string startdate, string enddate, string type, string status)
        {
            try {
                JsonApi.JsonApi_GetFeedbackSystem jsonData = new JsonApi.JsonApi_GetFeedbackSystem();
                jsonData.userid = userid;
                jsonData.startdate = startdate;
                if (startdate!="")
                {
                    string[] tmp = startdate.Split('-');
                    DateTime d = new DateTime(int.Parse(tmp[1]), int.Parse(tmp[0]), 1);
                    jsonData.startdate = d.ToString("yyyy-MM-dd");
                }

                jsonData.enddate = enddate;
                jsonData.type = type;
                jsonData.status = status;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getfeedbacksystem");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.8 E-获取Feedback类型 
        /// </summary>
        public string Api_GetFeedbackType()
        {
            try {
                string data = "";

                string analyRes = Api_GetResponseJson(data, "getfeedbacktype");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.8 D-系统管理员获取Feedback回复信息
        /// </summary>
        public string Api_GetFeedbackSystemRes(string userid, string feedbackid)
        {
            try {
                JsonApi.JsonApi_GetFeedbackSystemRes jsonData = new JsonApi.JsonApi_GetFeedbackSystemRes();
                jsonData.userid = userid;
                jsonData.feedbackid = feedbackid;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getfeedbacksystemres");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.9	新建Feedback信息
        /// </summary>
        public string Api_AddFeedback(string leasenum, string type, string title, string detail)
        {
            try {
                JsonApi.JsonApi_AddFeedback jsonData = new JsonApi.JsonApi_AddFeedback();
                jsonData.leasenum = leasenum;
                jsonData.type = type;
                jsonData.title = title;
                jsonData.detail = detail;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "addfeedback");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.10 回复Feedback信息
        /// </summary>
        public string Api_ReplyFeedback(string leasenum, string feedbackid, string replytype, string replydetail)
        {
            try
            {
                JsonApi.JsonApi_ReplyFeedback jsonData = new JsonApi.JsonApi_ReplyFeedback();
                jsonData.leasenum = leasenum;
                jsonData.feedbackid = feedbackid;
                jsonData.replytype = replytype;
                jsonData.replydetail = replydetail;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "replyfeedback");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.11 获取通知信息
        /// </summary>
        public string Api_GetNotice(string type, string userid, string startdate, string enddate)
        {
            try
            {
                JsonApi.JsonApi_GetNotice jsonData = new JsonApi.JsonApi_GetNotice();
                jsonData.type = type;
                jsonData.userid = userid;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getnotice");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.12 新建Notice信息
        /// </summary>
        public string Api_AddNotice(string userid, string startdate, string enddate, string type, string title, string detail, string imgurl)
        {
            try {
                JsonApi.JsonApi_NewNotice jsonData = new JsonApi.JsonApi_NewNotice();
                jsonData.userid = userid;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                jsonData.type = type;
                jsonData.title = title;
                jsonData.detail = detail;
                jsonData.imgurl = imgurl;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "newnotice");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.13 修改Notice信息
        /// </summary>
        public string Api_EditNotice(string userid, string noticeid, string startdate, string enddate, string type, string title, string detail, string imgurl)
        {
            try {
                JsonApi.JsonApi_EditNotice jsonData = new JsonApi.JsonApi_EditNotice();
                jsonData.userid = userid;
                jsonData.noticeid = noticeid;
                jsonData.startdate = startdate;
                jsonData.enddate = enddate;
                jsonData.type = type;
                jsonData.title = title;
                jsonData.detail = detail;
                jsonData.imgurl = imgurl;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "editnotice");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.13 B-删除Notice信息 
        /// </summary>
        public string Api_DeleteNotice(string userid, string noticeid)
        {
            try {
                JsonApi.JsonApi_DeleteNotice jsonData = new JsonApi.JsonApi_DeleteNotice();
                jsonData.userid = userid;
                jsonData.noticeid = noticeid;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "deletenotice");
                return analyRes;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 3.14 获取租户信息
        /// </summary>
        public string Api_GetLeaseInfo(string leasenum)
        {
            try
            {
                JsonApi.JsonApi_GetLeaseInfo jsonData = new JsonApi.JsonApi_GetLeaseInfo();
                jsonData.leasenum = leasenum;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getleaseinfo");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.15 获取一张对账单所有信息
        /// </summary>
        public string Api_GetStatementDetail(string leasenum, string statementnum)
        {
            try
            {
                JsonApi.JsonApi_GetStatementDetail jsonData = new JsonApi.JsonApi_GetStatementDetail();
                jsonData.leasenum = leasenum;
                jsonData.statementnum = statementnum;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "getstatementdetail");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.16 注册前台账号
        /// </summary>
        public string Api_Register(string statementnum, string balance, string leasenum, string expiredate, string email, string rental, string contactname, string phone)
        {
            try
            {
                JsonApi.JsonApi_Register jsonData = new JsonApi.JsonApi_Register();
                jsonData.statementnum = statementnum;
                jsonData.balance = balance;
                jsonData.leasenum = leasenum;
                jsonData.expiredate = expiredate;
                jsonData.email = email;
                jsonData.rental = rental;
                jsonData.contactname = contactname;
                jsonData.phone = phone;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "register");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.17 前台账号绑定（删除）租约号 
        /// </summary>
        public string Api_LinkLease(string loginname, string type, string statementnum, string balance, string leasenum, string expiredate, string rental)
        {
            try
            {
                JsonApi.JsonApi_LinkLease jsonData = new JsonApi.JsonApi_LinkLease();
                jsonData.loginname = loginname;
                jsonData.type = type;
                jsonData.statementnum = statementnum;
                jsonData.balance = balance;
                jsonData.leasenum = leasenum;
                jsonData.expiredate = expiredate;
                jsonData.rental = rental;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "linklease");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        public string Api_UpdateAccountInfo(string userid, string type, string email, string phone, string custname)
        {
            try
            {
                JsonApi.JsonApi_UpdateAccountInfo jsonData = new JsonApi.JsonApi_UpdateAccountInfo();
                jsonData.userid = userid;
                jsonData.type = type;
                jsonData.email = email;
                jsonData.phone = phone;
                jsonData.custname = custname;
                string data = JsonConvert.SerializeObject(jsonData);

                string analyRes = Api_GetResponseJson(data, "updateaccountinfo");
                return analyRes;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 3.20 用户更改email
        /// </summary>
        public string Api_UpdateEmail(string userid, string email, string contactname, string phone)
        {
            JsonApi.JsonApi_UpdateEmail jsonData = new JsonApi.JsonApi_UpdateEmail();
            jsonData.userid = userid;
            jsonData.email = email;
            jsonData.contactname = contactname;
            jsonData.phone = phone;
            string data = JsonConvert.SerializeObject(jsonData);

            string analyRes = Api_GetResponseJson(data, "updateemail");
            return analyRes;
        }

        /// <summary>
        /// 3.21 校验用户
        /// </summary>
        public string Api_VerifyUser(string statementnum, string balance, string leasenum, string expiredate, string rental)
        {
            JsonApi.JsonApi_VerifyUser jsonData = new JsonApi.JsonApi_VerifyUser();
            jsonData.statementnum = statementnum;
            jsonData.balance = balance;
            jsonData.leasenum = leasenum;
            jsonData.expiredate = expiredate;
            jsonData.rental = rental;
            string data = JsonConvert.SerializeObject(jsonData);

            string analyRes = Api_GetResponseJson(data, "verifyuser");
            return analyRes;
        }

        /// <summary>
        /// 生成请求包结构
        /// </summary>
        public string GenRequestHead(string data)
        {
            Log.WriteLog("GenRequestHead():" + data);
            /*请求包结构*/
            RequestStructure reqStructure = new RequestStructure();
            reqStructure.from = ConfigurationManager.AppSettings["from"].ToString();
            reqStructure.timestamp = GenTimestamp();
            reqStructure.nonce = GenNonce();
            reqStructure.sign = GenSign(reqStructure.from, reqStructure.timestamp, reqStructure.nonce, data);
            reqStructure.data = data;

            string jsonRequestHead = JsonConvert.SerializeObject(reqStructure);
            return jsonRequestHead.Replace("\\\"", "\"").Replace("\"data\":\"{\"", "\"data\":{\"").Replace("}\"}", "}}");
        }

        /// <summary>
        /// 分析响应包结构
        /// </summary>
        public string AnalyseResponse(string res)
        {
            try
            {
                ResponseStructure res_structure = JsonConvert.DeserializeObject<ResponseStructure>(res);
                if (res_structure.status == "true" && res_structure.code == "100")
                {
                    if (res_structure.data.ToString().Length >= 0)
                    {
                        return res_structure.data.ToString();
                    }
                    else
                    {
                        throw new Exception("Interface error:" + res);
                    }
                }
                else
                {
                    throw new Exception("Interface error: " + res);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 生成sign
        /// </summary>
        public string GenSign(string from, string timestamp, string nonce, string data)
        {
            string privateKey = ConfigurationManager.AppSettings["private_key"].ToString(); //HMAC sha1算法的private key
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("from", from);
            dic.Add("timestamp", timestamp);
            dic.Add("nonce", nonce);
            string str_signing = DictonarySort(dic) + data;

            /* HMAC SHA1加密 */
            HMACSHA1 hmaSha1 = new HMACSHA1();
            hmaSha1.Key = Encoding.UTF8.GetBytes(privateKey);
            byte[] dataBuffer = Encoding.UTF8.GetBytes(str_signing);
            byte[] hashBytes = hmaSha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 字典排序
        /// </summary>
        private string DictonarySort(Dictionary<string, string> dic)
        {
            var dicSort = from objDic in dic orderby objDic.Value select objDic;
            //var dicSort = from objDic in dic orderby objDic.Value descending select objDic;
            string strSorted = "";
            foreach (KeyValuePair<string, string> kvp in dicSort)
            {
                strSorted = strSorted + kvp.Value;
            }
            return strSorted;
        }


        /// <summary>
        /// 生成timestamp
        /// </summary>
        public string GenTimestamp()
        {
            System.DateTime dt = DateTime.Now;
            string returnTimestamp = dt.Year.ToString()
                + "-" + dt.Month.ToString().PadLeft(2, '0')
                + "-" + dt.Day.ToString().PadLeft(2, '0')
                + " " + dt.Hour.ToString().PadLeft(2, '0')
                + ":" + dt.Minute.ToString().PadLeft(2, '0')
                + ":" + dt.Second.ToString().PadLeft(2, '0');
            return returnTimestamp;
        }

        /// <summary>
        /// 生成随机数 0-99
        /// </summary>
        public string GenNonce()
        {
            Random rd = new Random();
            string strRan = rd.Next(0, 100).ToString().PadLeft(2, '0');
            return strRan;
        }

        /// <summary>
        /// 请求包结构
        /// </summary>
        public class RequestStructure
        {
            public string from { get; set; }
            public string timestamp { get; set; }
            public string nonce { get; set; }
            public string sign { get; set; }
            public string data { get; set; }
        }

        /// <summary>
        /// 返回包结构
        /// </summary>
        public class ResponseStructure
        {
            public string status { get; set; }
            public string message { get; set; }
            public string code { get; set; }
            public string time { get; set; }
            public object data { get; set; }

            //需要数组
            //public TestStruct2[] data { get; set; }
            //不需要数组
            //public TestStruct2 data { get; set; }
        }

        public class TestStruct2
        {
            public string test { get; set; }
            public string test2 { get; set; }
        }

        /// <summary>
        /// 根据账号对应的租约号获取待支付金额 
        /// </summary>
        public string GetOutstanding(string leasenum)
        {
            /*
             * 暂时不使用
            string json = Api_GetOnlinePayInfo(leasenum);
            JsonApi.JsonApi_GetOnlinepayInfo_Res res = JsonConvert.DeserializeObject<Model.JsonApi.JsonApi_GetOnlinepayInfo_Res>(json);
            return res.totalamount;
             */
            string result = "0";
            string startdate = DateTime.Now.AddDays(-183).ToString("yyyy-MM-dd");
            string json = Api_GetStatement(leasenum, startdate, "", "");
            JsonApi.JsonApi_GetStatement_Res jsonDataRes = JsonConvert.DeserializeObject<JsonApi.JsonApi_GetStatement_Res>(json);
            if (jsonDataRes.result == "100")
            {
                if (jsonDataRes.statementnum == "0")
                {
                    result = "0";
                }
                else
                {
                    List<JsonApi.JsonApi_GetStatement_StatementInfo> list = new List<JsonApi.JsonApi_GetStatement_StatementInfo>();
                    foreach (JsonApi.JsonApi_GetStatement_StatementInfo statementinfo in jsonDataRes.statementinfo)
                    {
                        list.Add(statementinfo);
                    }
                    list = list.OrderByDescending(o => o.statementdate).ToList();  //按时间排序
                    result = list[0].statementamount.ToString();
                }
            }
            else
            {
                throw new Exception(jsonDataRes.message);
            }

            return result;
        }
    }
}