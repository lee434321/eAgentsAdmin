using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyOneAppWeb.Model
{
    public class JsonApi
    {
        /// <summary>
        /// 3.1	用户登录校验
        /// </summary>
        public class JsonApi_LoginCheck
        {
            public string username { get; set; }
            public string password { get; set; }
            public string logintype { get; set; }
        }

        public class JsonApi_LoginCheck_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string userid { get; set; }
            public string status { get; set; }
            public string lastlogin { get; set; }
            public string leasecount { get; set; }
            public List<JsonApi_LoginCheck_Leaseinfo> leaseinfo { get; set; }
        }

        public class JsonApi_LoginCheck_Leaseinfo
        {
            public string leasenumber { get; set; }
        }


        /// <summary>
        /// 3.2	用户忘记密码
        /// </summary>
        public class JsonApi_ForgetPsw
        {
            public string userid { get; set; }
            public string logintype { get; set; }
        }

        public class JsonApi_ForgetPsw_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string checkcode { get; set; }
            public string email { get; set; }
            public string oldpsw { get; set; }
        }

        /// <summary>
        /// 3.3	更改密码
        /// </summary>
        public class JsonApi_ChangePsw
        {
            public string userid { get; set; }
            public string logintype { get; set; }
            public string oldpsw { get; set; }
            public string newpsw { get; set; }
        }

        public class JsonApi_ChangePsw_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.4	获取待支付账单信息
        /// </summary>
        public class JsonApi_GetOnlinepayInfo
        {
            public string leasenum { get; set; }
        }

        public class JsonApi_GetOnlinepayInfo_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string customername { get; set; }
            public string shopname { get; set; }
            public string premisename { get; set; }
            public string shoparea { get; set; }
            public string totalamount { get; set; }
            public string payinfonum { get; set; }
            public JsonApi_GetOnlinepayInfo_payinfo[] payinfo { get; set; }
        }

        public class JsonApi_GetOnlinepayInfo_payinfo
        {
            public string rowid { get; set; }
            public string transno { get; set; }
            public string chargeitem { get; set; }
            public string descr { get; set; }
            public float amount { get; set; }
            public float outstanding { get; set; }
            public DateTime duedate { get; set; }
            public string invoicelinenum { get; set; }
        }

        /// <summary>
        /// 3.5	通知后台支付结果
        /// </summary>
        public class JsonApi_OnlinepayResult
        {
            public string leasenum { get; set; }
            public string actualamount { get; set; }
            public string actualpaytype { get; set; }
            public string receivecompanycode { get; set; }
            public string currencycode { get; set; }
            public string bankaccount { get; set; }
            public string customercode { get; set; }
            public string actualpaydate { get; set; }
            public string status { get; set; }
            public string ppsreferenceno { get; set; }
            public string ppsmerchantid { get; set; }
            public string ppstxcode { get; set; }
            public string ppsamount { get; set; }
            public string ppsopcode { get; set; }
            public string ppspayfor { get; set; }
            public string ppslocale { get; set; }
            public string ppsuserdata { get; set; }
            public string ppssiteid { get; set; }
            public string ppsstatuscode { get; set; }
            public string ppsresponsecode { get; set; }
            public string ppsbankaccount { get; set; }
            public string ppsvaluedate { get; set; }
            public string ppstxdate { get; set; }
            public string ppspostid { get; set; }
            public string ppsisn { get; set; }
            public string ppswholemessage { get; set; }
            public List<JsonApi_OnlinepayResult_actualpayinfo> actualpayinfo { get; set; }
        }

        public class JsonApi_OnlinepayResult_actualpayinfo
        {
            public string transno { get; set; }
            public string amount { get; set; }
            public string actualpay { get; set; }
            public string chargecode { get; set; }
            public string invoicelinenum { get; set; }
        }

        public class JsonApi_OnlinepayResult_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.6	获取已支付账单历史记录
        /// </summary>
        public class JsonApi_OnlinepayHistory
        {
            public string leasenum { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
        }

        public class JsonApi_OnlinepayHistory_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string paynum { get; set; }
            public List<JsonApi_OnlinepayHistory_payinfo> payinfo { get; set; }
        }

        public class JsonApi_OnlinepayHistory_payinfo
        {
            public DateTime paydate { get; set; }
            public float payamount { get; set; }
            public string currency { get; set; }
            public string payresult { get; set; }
        }

        /// <summary>
        /// 3.7	获取对账单
        /// </summary>
        public class JsonApi_GetStatement
        {
            public string leasenum { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string statementnum { get; set; }
        }

        public class JsonApi_GetStatement_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string statementnum { get; set; }
            public JsonApi_GetStatement_StatementInfo[] statementinfo { get; set; }
        }

        public class JsonApi_GetStatement_StatementInfo
        {
            public string statementnum { get; set; }
            public DateTime statementdate { get; set; }
            public DateTime paymentduedate { get; set; }
            public string statementamount { get; set; }
            public string url { get; set; }
        }

        /// <summary>
        /// 3.8	A-租约用户获取Feedback信息
        /// </summary>
        public class JsonApi_GetFeedback
        {
            public string leasenum { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string type { get; set; }
            public string status { get; set; }
        }

        public class JsonApi_GetFeedback_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string feedbacknum { get; set; }
            public List<JsonApi_GetFeedback_feedbackinfo> feedbackinfo { get; set; }

            public JsonApi_GetFeedback_Res() { }

            public JsonApi_GetFeedback_Res(List<T_FeedBack> etys)
            {
                this.feedbackinfo = new List<JsonApi_GetFeedback_feedbackinfo>(); //feedback主表
                for (int i = 0; i < etys.Count; i++)
                {
                    JsonApi.JsonApi_GetFeedback_feedbackinfo ety = new JsonApi_GetFeedback_feedbackinfo();
                    ety.feedbackid = etys[i].FeedbackId.ToString();
                    ety.leasenum = etys[i].LEASENUMBER;
                    ety.status = etys[i].STATUS.ToString();
                    ety.title = etys[i].Title;
                    ety.type = etys[i].TYPE.ToString();
                    ety.detail = etys[i].Detail;
                    ety.date = etys[i].CreateDate;
                    this.feedbackinfo.Add(ety);
                }

                this.result = "100";
                this.message = "接口完成";
                this.feedbacknum = etys.Count.ToString();
            }
        }

        public class JsonApi_GetFeedback_feedbackinfo
        {
            public string feedbackid { get; set; }
            public string type { get; set; }
            public DateTime date { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
            public string status { get; set; }
            public string leasenum { get; set; }
            public string approve { get; set; } /// 新增字段，标识审批状态。
                                                /// "W"=待回复(res为空和前端用户提交时赋值)；
                                                /// "U"=未审批(后台用户回复时赋值)；
                                                /// "A"=已审批；            
            public int id { get; set; }         // 最新一条回复id       
            public int sort { get; set; }

            public string feedbackStatus { get; set; }
            public string approveStatus { get; set; }

        }

        /// <summary>
        /// 3.8 B-租约用户获取Feedback的回复信息
        /// </summary>
        public class JsonApi_GetFeedbackRes
        {
            public string leasenum { get; set; }
            public string feedbackid { get; set; }
        }

        public class JsonApi_GetFeedbackRes_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string feedbackid { get; set; }
            public List<JsonApi_GetFeedbackRes_Repyinfo> replyinfo { get; set; }
        }

        public class JsonApi_GetFeedbackRes_Repyinfo
        {
            public string id { get; set; }
            public DateTime date { get; set; }
            public string replytype { get; set; }
            public string replyperson { get; set; }
            public string detail { get; set; }
        }

        /// <summary>
        /// 3.8 C-系统管理员获取Feedback信息
        /// </summary>
        public class JsonApi_GetFeedbackSystem
        {
            public string userid { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string type { get; set; }
            public string status { get; set; }
        }

        /// <summary>
        /// 3.8 D-系统管理员获取Feedback回复信息
        /// </summary>
        public class JsonApi_GetFeedbackSystemRes
        {
            public string userid { get; set; }
            public string feedbackid { get; set; }
        }

        /// <summary>
        /// 3.8 E-获取Feedback类型
        /// </summary>
        public class JsonApi_GetFeedbackType_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string typenum { get; set; }
            public List<JsonApi_GetFeedbackTypeInfo> typeinfo { get; set; }
        }

        public class JsonApi_GetFeedbackTypeInfo
        {
            public string order { get; set; }
            public string id { get; set; }
            public string nameeng { get; set; }
            public string namecht { get; set; }
            public string namechs { get; set; }
        }

        /// <summary>
        /// 3.9	新建反馈信息
        /// </summary>
        public class JsonApi_AddFeedback
        {
            public string leasenum { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
        }

        public class JsonApi_AddFeedback_Res
        {
            public string result { get; set; }
            public string message { get; set; }            
        }

        /// <summary>
        /// 3.10 回复反馈信息
        /// </summary>
        public class JsonApi_ReplyFeedback
        {
            public string leasenum { get; set; }
            public string feedbackid { get; set; }
            public string replytype { get; set; }
            public string replydetail { get; set; }
        }

        public class JsonApi_ReplyFeedback_Res
        {
            public string result { get; set; }
            public string message { get; set; } 
        }

        /// <summary>
        /// 3.11 获取通知信息
        /// </summary>
        public class JsonApi_GetNotice
        {
            public string type { get; set; }
            public string userid { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
        }

        public class JsonApi_GetNotice_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string noticenum { get; set; }
            public List<JsonApi_GetNotice_noticeinfo> noticeinfo { get; set; }
        }

        public class JsonApi_GetNotice_noticeinfo
        {
            public string noticeid { get; set; }
            public string type { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
            public string date { get; set; }
            public string imgurl { get; set; }
        }

        /// <summary>
        /// 3.12 新建通知信息
        /// </summary>
        public class JsonApi_NewNotice
        {
            public string userid { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
            public string imgurl { get; set; }
        }

        public class JsonApi_NewNotice_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.13 修改通知信息
        /// </summary>
        public class JsonApi_EditNotice
        {
            public string userid { get; set; }
            public string noticeid { get; set; }
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public string detail { get; set; }
            public string imgurl { get; set; }
        }

        public class JsonApi_EditNotice_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.13 B-删除Notice信息
        /// </summary>
        public class JsonApi_DeleteNotice
        {
            public string userid { get; set; }
            public string noticeid { get; set; }
        }

        public class JsonApi_DeleteNotice_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.14 获取租户信息
        /// </summary>
        public class JsonApi_GetLeaseInfo
        {
            public string leasenum { get; set; }
        }

        public class JsonApi_GetLeaseInfo_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public List<JsonApi_GetLeaseInfo_Leaseinfo> leaseinfo { get; set; }
        }

        public class JsonApi_GetLeaseInfo_Leaseinfo
        {
            public string leasenum { get; set; }
            public string custnum { get; set; }
            public string custname { get; set; }
            public string billingaddress { get; set; }
            public string leasestartdate { get; set; }
            public string leaseenddate { get; set; }
            public string tradename { get; set; }
            public string premises { get; set; }
            public string email { get; set; }
            public string contactperson { get; set; }
            public string contactnum { get; set; }
        }

        /// <summary>
        /// 3.15 获取一张对账单所有信息
        /// </summary>
        public class JsonApi_GetStatementDetail
        {
            public string leasenum { get; set; }
            public string statementnum { get; set; }
        }

        public class JsonApi_GetStatementDetail_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string ppsmerchantcode { get; set; }
            public string accountnumber { get; set; }
            public string statementnumber { get; set; }
            public string statementdate { get; set; }
            public string duedate { get; set; }
            public string cutoffdate { get; set; }
            public string customername { get; set; }
            public string contactaddress1 { get; set; }
            public string contactaddress2 { get; set; }
            public string contactaddress3 { get; set; }
            public string contactaddress4 { get; set; }
            public string companyname { get; set; }
            public string leasenumber { get; set; }
            public string premise { get; set; }
            public string premise4 { get; set; }
            public string previousbalance { get; set; }
            public List<JsonApi_GetStatementDetail_TransactionInfo> transactioninfo { get; set; }
            public string statementbalance { get; set; }
            public string payment { get; set; }
            public List<JsonApi_GetStatementDetail_PaymentInfo> paymentinfo { get; set; }
            public string currentdue { get; set; }
            public string adjustment { get; set; }
            public string overdueinterest { get; set; }
            public string total { get; set; }
            public string overdueprime { get; set; }
            public string overdueprimech { get; set; }
        }

        public class JsonApi_GetStatementDetail_TransactionInfo
        {
            public string transactionno { get; set; }
            public string transactiondate { get; set; }
            public string chargedesc { get; set; }
            public string paymentdesc { get; set; }
            public string transactionamount { get; set; }
        }

        public class JsonApi_GetStatementDetail_PaymentInfo
        {
            public string paymentdate { get; set; }
            public string paymenttrans { get; set; }
            public string paymentchargedesc { get; set; }
            public string paymentdesc { get; set; }
            public string paymentamount { get; set; }
            public List<JsonApi_GetStatementDetail_PaymentHistory> paymenthistory { get; set; }
        }

        public class JsonApi_GetStatementDetail_PaymentHistory
        {
            public string historychargedesc { get; set; }
            public string historypaymentdesc { get; set; }
            public string historyamount { get; set; }
        }

        /// <summary>
        /// 3.16 注册前台账号
        /// </summary>
        public class JsonApi_Register
        {
            public string statementnum { get; set; }
            public string balance { get; set; }
            public string leasenum { get; set; }
            public string expiredate { get; set; }
            public string email { get; set; }
            public string rental { get; set; }
            public string contactname { get; set; }
            public string phone { get; set; }
        }

        public class JsonApi_Register_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string temppsw { get; set; }
        }

        /// <summary>
        /// 3.17 前台账号绑定（删除）租约号
        /// </summary>
        public class JsonApi_LinkLease
        {
            public string loginname { get; set; }
            public string type { get; set; }
            public string statementnum { get; set; }
            public string balance { get; set; }
            public string leasenum { get; set; }
            public string expiredate { get; set; }
            public string rental { get; set; }
        }

        public class JsonApi_LinkLease_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.19 查询/更改账号登录用户信息
        /// </summary>
        public class JsonApi_UpdateAccountInfo
        {
            public string userid { get; set; }
            public string type { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string custname { get; set; }
        }

        public class JsonApi_UpdateAccountInfo_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string custname { get; set; }
        }

        /// <summary>
        /// 3.20 用户更改email
        /// </summary>
        public class JsonApi_UpdateEmail
        {
            public string userid { get; set; }
            public string psw { get; set; }
            public string email { get; set; }
            public string contactname { get; set; }
            public string phone { get; set; }
        }
        public class JsonApi_UpdateEmail_Res
        {
            public string result { get; set; }
            public string message { get; set; }
        }

        /// <summary>
        /// 3.21 校验用户
        /// </summary>
        public class JsonApi_VerifyUser
        {
            public string statementnum { get; set; }
            public string balance { get; set; }
            public string leasenum { get; set; }
            public string expiredate { get; set; }
            public string rental { get; set; }
        }
        public class JsonApi_VerifyUser_Res
        {
            public string result { get; set; }
            public string message { get; set; }
            public string check { get; set; }
        }

        /// <summary>
        /// Bootgrid返回Json格式
        /// </summary>
        public class JsonApi_Bootgrid_SearchOnlinePay
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public List<JsonApi_GetOnlinepayInfo_payinfo> rows { get; set; }
            public int total { get; set; }
        }

        public class JsonApi_Bootgrid_SearchPayHistory
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public List<JsonApi_OnlinepayHistory_payinfo> rows { get; set; }
            public int total { get; set; }
        }

        public class JsonApi_Bootgrid_SearchStatement
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public List<JsonApi_GetStatement_StatementInfo> rows { get; set; }
            public int total { get; set; }
        }

        public class JsonApi_Bootgrid_SearchFeedback
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public List<JsonApi_GetFeedback_feedbackinfo> rows { get; set; }
            public int total { get; set; }
        }

        public class JsonApi_Bootgrid_SearchLeaseGroup
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public List<DataModel.ModelLeaseGroup> rows { get; set; }
            public int total { get; set; }
        }

        /// <summary>
        /// 后台统一Bootgrid样式
        /// </summary>
        public class JsonApi_Bootgrid_System
        {
            public int current { get; set; }
            public int rowCount { get; set; }
            public string rows { get; set; }
            public int total { get; set; }
        }
    }

    
}