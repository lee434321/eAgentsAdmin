using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Collections;
using System.Data;
using Newtonsoft.Json;
using PropertyOneAppWeb.Model;
using System.Configuration;

namespace PropertyOneAppWeb.Commom
{
    public class GlobalUtil
    {
        //bootgrid读取错误时的返回值
        public static string bootErrorStr = "{\"current\":1,\"rowCount\":10,\"rows\":[],\"total\":0}";

        /// <summary>
        /// 将字符串转换为财务格式
        /// </summary>
        public static string FormatMoney(string val)
        {
            try
            {
                double money = double.Parse(val);
                string returnVal = String.Format("{0:n2}", money);
                return returnVal;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 将日期中的时间去掉
        /// </summary>
        public static string FormatDate(string val)
        {
            try
            {
                string[] sArray = val.Split(' ');
                return sArray[0];
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 根据前台bootgird，设置翻页
        /// </summary>
        public static List<T> PageList<T>(List<T> list, int current, int rowCount)
        {
            try {
                int pageNum = 0;   //页面总数
                int pageMod = 0;   //最后一页的数量
                if (list.Count % rowCount == 0)
                {
                    pageNum = list.Count / rowCount;
                    pageMod = rowCount;
                }
                else
                {
                    pageNum = (list.Count / rowCount) + 1;
                    pageMod = list.Count % rowCount;
                }

                for (int i = 1; i <= pageNum; i++)
                {
                    if (i == current)
                    {
                        if (pageNum > 1)
                        {
                            if (i < pageNum)
                            {
                                list = list.GetRange((current - 1) * rowCount, rowCount);
                                break;
                            }
                            else if (i == pageNum)  //如果是最后一页
                            {
                                list = list.GetRange((current - 1) * rowCount, pageMod);
                                break;
                            }    
                        }
                        else if (pageNum == 1)
                        {
                            break;
                        }
                    }
                }

                return list;
            }
            catch {
                return list;
            }
        }

        /// <summary>
        /// 返回源DataTable中的某几行
        /// </summary>
        public static DataTable DataTableGetRange(DataTable dt, int index, int count)
        {
            try
            {
                DataTable dtNewTable = dt.Clone();
                DataRow[] rows = dt.Select();
                int dtRowCount = dt.Rows.Count;
                for (int i = index; i < index + count; i++)
                {
                    dtNewTable.ImportRow(rows[i]);
                }
                return dtNewTable;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据前台bootgird，设置翻页(DataTable版本)
        /// </summary>
        public static DataTable PageList(DataTable list, int current, int rowCount)
        {
            try
            {
                int pageNum = 0;   //页面总数
                int pageMod = 0;   //最后一页的数量
                if (list.Rows.Count % rowCount == 0)
                {
                    pageNum = list.Rows.Count / rowCount;
                    pageMod = rowCount;
                }
                else
                {
                    pageNum = (list.Rows.Count / rowCount) + 1;
                    pageMod = list.Rows.Count % rowCount;
                }

                for (int i = 1; i <= pageNum; i++)
                {
                    if (i == current)
                    {
                        if (pageNum > 1)
                        {
                            if (i < pageNum)
                            {
                                list = DataTableGetRange(list, (current - 1) * rowCount, rowCount);
                                break;
                            }
                            else if (i == pageNum)  //如果是最后一页
                            {
                                list = DataTableGetRange(list, (current - 1) * rowCount, pageMod);
                                break;
                            }
                        }
                        else if (pageNum == 1)
                        {
                            break;
                        }
                    }
                }
                return list;
            }
            catch
            {
                return list;
            }
        }

        /// <summary>
        /// 将一种格式日期转换为另一种格式日期
        /// </summary>
        public static string ConvertDateType(string dateStr, string fromType, string toType)
        {
            //GlobalUtil.ConvertDateType(startdate, "dd-MM-yyyy", "yyyy-MM-dd");
            try {
                DateTime dt;
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                dtFormat.ShortDatePattern = fromType;
                dt = Convert.ToDateTime(dateStr, dtFormat);
                string returnDateStr = dt.ToString(toType);
                return returnDateStr;
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// 将字符串转换为DateTime格式
        /// </summary>
        public static DateTime ConvertStrToDateTime(string dateStr, string format)
        {
            try
            {
                DateTime date;
                DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                dateFormat.ShortDatePattern = format;
                date = Convert.ToDateTime(dateStr, dateFormat);
                return date;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 将Json中的时间格式(2018-02-14T00:00:00)，转换成DateTime格式
        /// </summary>
        /// <returns></returns>
        public static DateTime ConvertJsonDateToDateTime(string jsonDate)
        {
            try
            {
                string[] arrayDate = jsonDate.Split('T');
                string dateLeft = arrayDate[0];        //2018-02-14
                string dateRight = arrayDate[1];       //00:00:00
                DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                dateFormat.ShortDatePattern = "yyyy-MM-dd hh-mm-ss";
                DateTime date = Convert.ToDateTime(dateLeft + " " + dateRight, dateFormat);
                return date;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 返回Bootgrid排序的关键字
        /// </summary>
        /// <returns></returns>
        public static ArrayList GetSortSenderFromBootgrid(HttpRequest hr)
        {
            try
            {
                string sortSender = hr.Form.Keys[2];
                string sortType = hr.Form[2];
                if (sortType == "asc" || sortType == "desc")
                {
                    sortSender = sortSender.Substring(5, sortSender.Length - 6);
                    ArrayList returnArray = new ArrayList();
                    returnArray.Add(sortSender);
                    returnArray.Add(sortType);
                    return returnArray;
                }
                else
                {
                    return null;
                }
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// 根据DataTable生成Bootgird的Json 
        /// </summary>
        public static string GenBootGridSystem(DataTable dt, int current, int rowCount)
        {
            try
            {
                int dtrowcount = dt.Rows.Count;
                if (dtrowcount > 0) //如果返回大于一条数据
                {
                    //设置翻页
                    dt = GlobalUtil.PageList(dt, current, rowCount);

                    //初始化bootgrid
                    string dtjson = JsonConvert.SerializeObject(dt);
                    JsonApi.JsonApi_Bootgrid_System jsonboot = new JsonApi.JsonApi_Bootgrid_System();
                    jsonboot.current = current;
                    jsonboot.rowCount = rowCount;
                    jsonboot.rows = dtjson;
                    jsonboot.total = dtrowcount;

                    string json = JsonConvert.SerializeObject(jsonboot);
                    json = json.Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\"[", "[").Replace("]\"", "]");
                    return json;
                }
                else  //如果没有返回数据
                {
                    return bootErrorStr;
                }
            }
            catch
            {
                throw;
            }
        }

        public static string GenBootGridSystemFromList<T>(List<T> etys, int current, int rowCount)
        {
            try
            {
                int dtrowcount = etys.Count;
                if (dtrowcount > 0) //如果返回大于一条数据
                {
                    //设置翻页
                    etys = GlobalUtil.PageList(etys, current, rowCount);

                    //初始化bootgrid
                    string dtjson = JsonConvert.SerializeObject(etys);
                    JsonApi.JsonApi_Bootgrid_System jsonboot = new JsonApi.JsonApi_Bootgrid_System();
                    jsonboot.current = current;
                    jsonboot.rowCount = rowCount;
                    jsonboot.rows = dtjson;
                    jsonboot.total = dtrowcount;

                    string json = JsonConvert.SerializeObject(jsonboot);
                    json = json.Replace("\\\"", "\"").Replace("\"[", "[").Replace("]\"", "]");
                    return json;
                }
                else  //如果没有返回数据
                {
                    return bootErrorStr;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="strPwChar">传入生成的随机字符串可以使用哪些字符</param>
        /// <param name="intlen">传入生成的随机字符串的长度</param>
        public static string MakePassword(int intlen, Random rnd)
        {
            //设定字符范围为:大小写字母及数字的随机字符串.
            string strPwChar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string strRe = "";
            int iRandNum;

            for (int i = 0; i < intlen; i++)
            {
                iRandNum = rnd.Next(strPwChar.Length);
                strRe += strPwChar[iRandNum];
            }
            return strRe;
        }

        /// <summary>
        /// 生成一个根据日期排列的文件名
        /// </summary>
        /// <returns></returns>
        public static string GenFileName()
        {
            Random ran = new Random();
            string fileName = DateTime.Now.Year.ToString()
                + DateTime.Now.Month.ToString().PadLeft(2, '0')
                + DateTime.Now.Day.ToString().PadLeft(2, '0')
                + DateTime.Now.Hour.ToString().PadLeft(2, '0')
                + DateTime.Now.Minute.ToString().PadLeft(2, '0')
                + DateTime.Now.Second.ToString().PadLeft(2, '0')
                + DateTime.Now.Millisecond.ToString().PadLeft(4, '0')
                + ran.Next(999).ToString().PadLeft(4, '0');
            return fileName;
        }

        public static void ImageToBase64()
        { }

        public static string GenerateMD5(string txt)
        {
            try
            {
                using (System.Security.Cryptography.MD5 mi = System.Security.Cryptography.MD5.Create())
                {
                    byte[] buffer = System.Text.Encoding.Default.GetBytes(txt);
                    //开始加密
                    byte[] newBuffer = mi.ComputeHash(buffer);
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 0; i < newBuffer.Length; i++)
                    {
                        sb.Append(newBuffer[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "\r\n" + err.Source + "\r\n" + err.StackTrace);
            }
            
        }
    }
}