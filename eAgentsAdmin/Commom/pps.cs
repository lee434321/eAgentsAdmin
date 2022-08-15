using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iPGClientCOM;

namespace PropertyOneAppWeb.Commom
{
    public class pps
    {
        /// <summary>
        /// 通过GenDO发起支付请求
        /// </summary>
        public string GenPaymentRequest(string refno, string amount, string payfor, string locale, string userdata)
        {
            amount = GenHkdAmount(amount);
            string gendourl = System.Configuration.ConfigurationManager.AppSettings["com_gendo"].ToString();
            string merchantid = System.Configuration.ConfigurationManager.AppSettings["com_merchantid"].ToString();
            string txcode = System.Configuration.ConfigurationManager.AppSettings["com_txcode"].ToString();
            string opcode = "00";
            string res = HttpUtil.HttpRequestPost(gendourl,
                "ReferenceNo=" + refno +
                "&MerchantID=" + merchantid +
                "&TxCode=" + txcode +
                "&Amount=" + amount +
                "&OpCode=" + opcode +
                "&PayFor=" + payfor +
                "&Locale=" + locale +
                "&UserData=" + userdata);
            return res;
        }

        /// <summary>
        /// 发起支付查询
        /// </summary>
        public void SendPaymentEnquiry(string refno)
        {
            string gendourl = System.Configuration.ConfigurationManager.AppSettings["com_gendo"].ToString();
            string merchantid = System.Configuration.ConfigurationManager.AppSettings["com_merchantid"].ToString();
            string opcode = "03";
            string res = HttpUtil.HttpRequestPost(gendourl,
                "ReferenceNo=" + refno +
                "&MerchantID=" + merchantid +
                "&OpCode=" + opcode);
        }

        /// <summary>
        /// 解析Host返回的DR
        /// </summary>
        public PPSModel_DR ExtractDR(string dr, string msg)
        {
            try {
                string configFile = System.Configuration.ConfigurationManager.AppSettings["com_config"].ToString();                
                //iPGClientCOM.iPGClient ipg = new iPGClientClass();
                iPGClientCOM.iPGClient ipg = new iPGClient();
                if (ipg.setConfigFile(configFile) == true)
                {
                    //解密DR数据
                    DRObject dro = ipg.decEncDR(dr);
                    //判断DR格式是否正确
                    if (dro.isMsgFormatCorrect() == true && dro.validateSignature() == true)
                    {
                        PPSModel_DR mdr = new PPSModel_DR();
                        mdr.ReferenceNo = dro.getReferenceNo();
                        mdr.UserData = dro.getUserData();
                        mdr.SiteID = dro.getSiteID();
                        mdr.MerchantID = dro.getMerchantID();
                        mdr.TxCode = dro.getTxCode();
                        mdr.OpCode = dro.getOpCode();
                        mdr.Amount = dro.getAmount();
                        mdr.StatusCode = dro.getStatusCode();
                        mdr.ResponseCode = dro.getResponseCode();
                        mdr.BankAccount = dro.getAccountNo();
                        mdr.ValueDate = dro.getValueDate();
                        mdr.TxDate = dro.getTxDate();
                        mdr.POSTID = dro.getPOSTID();
                        mdr.ISN = dro.getISN();
                        mdr.WholeMessage = dro.getWholeMessage();
                        return mdr;
                    }
                    return null;
                }
                return null;
            }
            catch {
                return null;
            }
            
        }

        /// <summary>
        /// 生成Reference No
        /// </summary>
        public string GenRefNo()
        {
            string refno = DateTime.Now.ToString("yyMMddHHmmss") + DateTime.Now.Millisecond.ToString("0000");
            return refno;
        }

        /// <summary>
        /// 将Amount数值转换为DO中的数值类型
        /// E.g. 100=HKD$1
        /// </summary>
        public string GenHkdAmount(string amount)
        {
            try {
                float hkd = float.Parse(amount);
                hkd = hkd * 100;
                return hkd.ToString();
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// PPS的DR数据结构
        /// </summary>
        public class PPSModel_DR
        {
            public string ReferenceNo { get; set; }
            public string UserData { get; set; }
            public string SiteID { get; set; }
            public string MerchantID { get; set; }
            public string TxCode { get; set; }
            public string OpCode { get; set; }
            public string Amount { get; set; }
            public string StatusCode { get; set; }
            public string ResponseCode { get; set; }
            public string BankAccount { get; set; }
            public string ValueDate { get; set; }
            public string TxDate { get; set; }
            public string POSTID { get; set; }
            public string ISN { get; set; }
            public string WholeMessage { get; set; }
        }
    }
}