using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using cn.magix.Helper;
namespace ftpa
{
    /// <summary>
    /// Summary description for engine
    /// </summary>
    public class engine : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string result = "";
            try
            {
                int year = 1900;
                string sp = context.Request.Form["searchParse"];
                int pIdx = 0, pSize = 0;
                int.TryParse(context.Request.Form["pageIdx"], out pIdx);
                int.TryParse(context.Request.Form["pageSize"], out pSize);
                string sorts = context.Request.Form["sorts"];
                switch (context.Request.QueryString["act"])
                {
                    case "search_files": 
                        if (context.Request.Form["year"]=="-1")                        
                            throw new Exception("必须选择年份");                        
                        else                        
                            year = Aid.Null2Int(context.Request.Form["year"]);                        
                        result = SearchFiles(sp, pIdx, pSize, year, sorts);
                        break;

                    case "search_turnover":                         
                        if (context.Request.Form["year"]=="-1")                        
                            throw new Exception("必须选择年份");                        
                        else                        
                            year = Aid.Null2Int(context.Request.Form["year"]);
                        result = SearchTurnover(sp, pIdx, pSize, year, sorts);
                        break;
                    
                    case "search_trc":
                        result = SearchTrc(sp, pIdx, pSize);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                JsonRsp ersp = new JsonRsp(-99, err.Message);
                result = JsonConvert.SerializeObject(ersp);
            }
            
            context.Response.Write(result);
        }

        private string SearchTrc(string sp, int idx, int size)
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Common.FtpDbConnStr, false);
            try
            {
                DbConsole bz = new DbConsole(tx);
                var predicate = PredicateBuilder.True<TrLeaseUnitModel>();
                if (!string.IsNullOrEmpty(sp))
                {
                    predicate = predicate.And(x => x.Lease_Number.Sql_Like(sp) || x.Prod_Id.Sql_Like(sp));
                }
                List<TrLeaseUnitModel> etys = bz.Retrieve<TrLeaseUnitModel>(predicate);
                rsp.result = this.SetPager(etys, idx, size);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string SearchTurnover(string sp, int idx, int size, int year, string sorts = "")
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Common.FtpDbConnStr, false);
            try
            {
                DbConsole bz = new DbConsole(tx);
                //处理查询条件   
                var predicate = PredicateBuilder.True<TrDailyTurnoverModel>();
                if (!string.IsNullOrEmpty(sp))
                    predicate = predicate.And(x => x.lease_number.Sql_Like(sp) || x.source.Sql_Like(sp));
                predicate = predicate.And(x => x.deal_date >= DateTime.Parse(year.ToString() + "-01" + "-01") && x.deal_date <= DateTime.Parse(year.ToString() + "-12" + "-31"));
                //处理排序
                List<Sorts> sort = new List<Sorts>();
                string[] a = sorts.Split(',');
                for (int i = 0; i < a.Length; i++)
                {
                    string[] aa = a[i].Split('|');
                    Sorts s = new Sorts(aa[0], aa[2], int.Parse(aa[1]));
                    sort.Add(s);
                }
                sort = sort.OrderByDescending(x => x.Track).ToList();
                string orderBy = " order by ";
                for (int j = 0; j < sort.Count; j++)
                    orderBy += sort[j].Field + " " + sort[j].Rule + ",";
                orderBy = orderBy.Substring(0, orderBy.Length - 1);
                //执行查询
                List<TrDailyTurnoverModel> etys = bz.Retrieve<TrDailyTurnoverModel>(predicate, orderBy);
                rsp.result = this.SetPager(etys, idx, size);
                tx.CommitTrans();
            }
            catch (Exception err)
            {
                rsp.err_code = -99;
                rsp.err_msg = err.Message;
            }
            return JsonConvert.SerializeObject(rsp);
        }

        private string SearchFiles(string sp, int idx, int size,int year, string sorts = "")
        {
            JsonRsp rsp = new JsonRsp();
            DbTx tx = new DbTx(DbVendor.Oracle, Common.FtpDbConnStr, false);
            try
            {
                DbConsole bz = new DbConsole(tx);
                //处理查询条件
                var predicate = PredicateBuilder.True<FtpSalesFilesModel>();                
                if (!string.IsNullOrEmpty(sp))                
                    predicate = predicate.And(x => x.Name.Sql_Like(sp) || x.Shortcut_key.Sql_Like(sp));                
                predicate = predicate.And(x => x.Recv_Time >= DateTime.Parse(year.ToString() + "-01" + "-01") && x.Recv_Time <= DateTime.Parse(year.ToString() + "-12" + "-31"));
                //处理排序
                List<Sorts> sort = new List<Sorts>();
                string[] a = sorts.Split(',');
                for (int i = 0; i < a.Length; i++)
                {
                    string[] aa = a[i].Split('|');
                    Sorts s = new Sorts(aa[0], aa[2], int.Parse(aa[1]));
                    sort.Add(s);
                }
                sort = sort.OrderByDescending(x => x.Track).ToList();
                string orderBy=" order by ";
                for (int j = 0; j < sort.Count; j++)               
                    orderBy += sort[j].Field + " " + sort[j].Rule + ",";
                orderBy = orderBy.Substring(0, orderBy.Length - 1);
                //执行查询
                List<FtpSalesFilesModel> etys = bz.Retrieve<FtpSalesFilesModel>(predicate, orderBy);  
                rsp.result = this.SetPager(etys, idx, size);                               
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void Check() 
        {
            
        }

        protected Pager<T> SetPager<T>(List<T> etys, int pidx, int pageSize = 10) where T : class
        {
            int pageIdx = pidx; //当前页               
            int q = etys.Count / pageSize;
            int r = etys.Count % pageSize;

            Pager<T> pager = new Pager<T>();
            pager.pageSize = pageSize;
            pager.count = r == 0 ? q : q + 1;
            if (pageIdx > pager.count) //如果超过总页数则就是最后1页。
                pageIdx = pager.count;
            if (pageIdx == 0)//如果为0则修改为第1页(翻页时会出现)。
                pageIdx = 1;
            pager.pageIdx = pageIdx;

            List<T> etysOut = new List<T>();
            int idx = 0;
            for (int i = 0; i < pageSize; i++)
            {
                idx = (pageIdx - 1) * pageSize + i;
                if (idx < etys.Count)
                    etysOut.Add(etys[idx]);
                else
                    break;
            }
            pager.data = etysOut;
            return pager;
        }
    }

    public class Sorts 
    {
        public string Field { get; set; }
        public string Rule { get; set; }
        public int Track { get; set; }

        public Sorts(string field, string rule, int track)
        {
            this.Field = field;
            this.Rule = rule;
            this.Track = track;
        }
    }

    public class Pager<T> where T : class
    {
        public int pageSize { get; set; }   //
        public int count { get; set; }      //
        public int pageIdx { get; set; }    //
        public List<T> data { get; set; }    //array    
    }

    public class FtpSalesFilesModel
    {
        public const string TABLE_NAME = "Ftp_Sales_Files";

        [DbField(true, true)]
        public int Id { get; set; }

        public string File_Type { get; set; }
        public string Name { get; set; }
        public DateTime Recv_Time { get; set; }
        public string Lease_Number { get; set; }
        public string Memo { get; set; }
        public string Status { get; set; }
        public string Shortcut_key { get; set; }
        public string File_date { get; set; }
    }
    public class TrLeaseUnitModel
    {
        public const string TABLE_NAME = "Tr_Lease_Unit";

        public string Shortcut_Key { get; set; }
        public string Lease_Number { get; set; }
        public string Prod_Id { get; set; }
        public string Tr_Flag { get; set; }
        public DateTime Lease_Term_From { get; set; }
        public Nullable<DateTime> Lease_Term_To { get; set; }
        public string Customer_Number { get; set; }
    }
    public class TrDailyTurnoverModel
    {
        public const string TABLE_NAME = "Tr_Daily_Turnover";

        [DbField(true)]
        public int id { get; set; }
        public string lease_number { get; set; }
        public string shortcut_key { get; set; }
        public DateTime deal_date { get; set; }
        public decimal deal_amount { get; set; }
        public string commission_code { get; set; }
        public decimal commission { get; set; }
        public string status { get; set; } //default 'DK',
        public string source { get; set; }
        public string receive_class { get; set; }
        public string memo { get; set; }
        public DateTime creation_date { get; set; }
        public string creation_by { get; set; }
        public Nullable<DateTime> last_updated_date { get; set; }
        public string last_updated_by { get; set; }
        public string invoice_number { get; set; }
        public string cn_creation_by { get; set; }
        public Nullable<DateTime> cn_creation_date { get; set; }
        public string tillid { get; set; }
    }
}