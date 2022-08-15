using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;

using cn.magix.Helper;
namespace ftpd
{
    class Program
    {
        static string yPath = "";
        static List<FtpSalesFile> filesInKey = new List<FtpSalesFile>();

        static void Main(string[] args)
        {
            Global.P1DB_CONN_STR = "user id=pmswh;password=pms123;persist security info=False;data source=PMS";
            Global.FTPDB_CONN_STR = "user id=pmsftp;password=pms123;persist security info=False;data source=PMS_74";
            Global.FTP_IP = "172.21.250.21";
            Global.FTP_USER_NAME = "dxp";
            Global.FTP_PASSWORD = "dxp";
            Global.FTP_BAKFILE_PATH = "BakFile";
            Global.FTP_ERRORFILE_PATH = "ErrorFile";
            Global.DAYS = 5;

            yPath = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                /// 1.取当前所有有效租约的unit(shortcut_key)；
                /// 2.遍历所有unit和lease，
                /// 3.根据unit和lease去找ftp文件，读取内容
                /// 4.将ftp文件内容取出，存入tr_daily_trunover
                int method = 1;
                if (method == 0)
                { }
                else if (method == 1)
                {
                    /*
                     * log
                     * 1.开始处理<unit_key>下的有效文件<counts>
                     * 2.进入single或triple环节
                     */
                    Global.ShortCutKeys = GetAllShortcutKeys();
                    foreach (IGrouping<string, ShortcutKeyQuery> item in Global.ShortCutKeys.GroupBy(x => x.Shortcut_Key))
                    {
                        Console.WriteLine(item.Key);
                        List<FtpSalesFile> files = GetFtpFiles(item.Key, Global.DAYS);
                        filesInKey = files;//这里需要一个key下所有文件的列表（处理“三文件”时使用）
                        Console.WriteLine("file counts:" + files.Count.ToString());
                        for (int i = 0; i < files.Count; i++)
                        {
                            if (files[i].name.ToUpper().Contains("LIST"))
                            {
                                ProcessSingleFile(item.Key, files[i]);
                            }
                            else if (files[i].name.ToUpper().Contains("MAIN"))
                            {
                                ProcessTripleFile(item.Key, files[i]);
                            }
                        }
                    }
                    ComputeTurnover();
                }
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                Console.ReadKey();
            }
        }

        #region turnover ftp task

        static void WriteSingleFtpError(string fileName, string lease_number, Exception err)
        {
            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                DbConsole bz = new DbConsole(tx);
                FtpSalesFilesModel ety = new FtpSalesFilesModel();
                ety.Lease_Number = lease_number;
                ety.Name = fileName;
                ety.Memo = err.Message;
                ety.Recv_Time = DateTime.Now;
                ety.Status = "E"; //error      
                bz.Create(ety);

                tx.CommitTrans();
            }
            catch (Exception ex)
            {
                tx.AbortTrans();
                WriteLog(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        static void WriteLog(string txt)
        {
            string logPath = yPath + @"\log\";
            if (!System.IO.Directory.Exists(logPath))
                System.IO.Directory.CreateDirectory(logPath);
            string logFilePath = logPath + Aid.Date2Str(DateTime.Now, Aid.DateFmt.Standard) + ".log";
            Aid.WriteLog(logFilePath, txt);
        }

        static List<ShortcutKeyQuery> GetAllShortcutKeys()
        {
            DbTx tx = new DbTx(DbVendor.Oracle, Global.P1DB_CONN_STR, false);
            try
            {
                string sql = "select l.lease_number,l.lease_term_from,l.lease_term_to,lu.shortcut_key";
                sql += " from lm_lease_unit lu, lm_lease l";
                sql += " where l.lease_number = lu.lease_number";
                sql += " and l.lease_version = lu.lease_version";
                sql += " and l.status = 'A'";
                sql += " and l.active = 'A'";

                // 调试目的
                //sql += " and lu.shortcut_key='WHTSB1B1080'";
                //sql += " and lu.shortcut_key='WHTSB1B1007'";
                //sql += " and lu.shortcut_key='WHTS2FL2067'"; 
                //sql += " and lu.shortcut_key='WHTSB1B1007'";

                sql += " and nvl(l.lease_term_to, to_date('2073-12-21', 'yyyy-mm-dd')) >= sysdate";

                DbConsole bz = new DbConsole(tx);
                List<ShortcutKeyQuery> keys = bz.Retrieve<ShortcutKeyQuery>(sql);
                tx.CommitTrans();
                return keys;
            }
            catch (Exception)
            {
                tx.AbortTrans();
                return new List<ShortcutKeyQuery>();
            }
        }

        /// <summary>
        /// 获取指定shortcut key下有效的ftp文件列表，无效的文件会记入数据库
        /// </summary>
        /// <param name="unitKey">unit shortcut key</param>
        /// <param name="days">指定日期间隔，默认是距今5天内的ftp文件，0表示不限制</param>
        /// <returns></returns>
        static List<FtpSalesFile> GetFtpFiles(string unitKey, int days = 5)
        {
            List<FtpSalesFile> files = new List<FtpSalesFile>();
            List<FtpSalesFile> filesInTerm = new List<FtpSalesFile>();
            string err_msg = "";
            int c = 0;

            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                FtpClient fc = new FtpClient(Global.FTP_IP, Global.FTP_USER_NAME, Global.FTP_PASSWORD); //WHTS4FL4010_20170801102359_LIST.txt

                List<string> bakFiles = fc.ListDir("/" + unitKey + "/" + Global.FTP_BAKFILE_PATH + "/");
                for (int i = 0; i < bakFiles.Count; i++)
                {
                    files.Add(new FtpSalesFile(Global.FTP_BAKFILE_PATH, bakFiles[i]));
                }

                List<string> errFiles = fc.ListDir("/" + unitKey + "/" + Global.FTP_ERRORFILE_PATH + "/");
                for (int j = 0; j < errFiles.Count; j++)
                {
                    files.Add(new FtpSalesFile(Global.FTP_ERRORFILE_PATH, errFiles[j]));
                }

                int year = 0, month = 0, day = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    bool check = true;
                    string tstr = files[i].name;
                    string[] a = tstr.ToUpper().Split('_');

                    if (a.Length != 3)
                    {
                        //文件格式错误
                        check = false;
                        err_msg = "文件格式错误";
                        Console.WriteLine(tstr + "(文件格式错误)");
                    }
                    else
                    {
                        int.TryParse(a[1].Substring(0, 4), out year);
                        int.TryParse(a[1].Substring(4, 2), out month);
                        int.TryParse(a[1].Substring(6, 2), out day);
                        if (year == 0 || (month <= 0 || month > 12) || (day <= 0 || day > 31))
                        {
                            //日期格式错误
                            check = false;
                            err_msg = "日期格式错误";
                            Console.WriteLine(tstr + "(日期格式错误)");
                        }
                    }

                    if (check)
                    {
                        DateTime file_date = new DateTime(year, month, day);
                        files[i].date = Aid.Date2Str(file_date, Aid.DateFmt.Standard);

                        if (days > 0)
                        {
                            if (DateTime.Now.Subtract(file_date).Days <= days)
                            {
                                filesInTerm.Add(files[i]);
                            }
                        }
                        else
                        {
                            filesInTerm.Add(files[i]);
                        }
                    }
                    else
                    {   //记录有格式错误的文件
                        DbConsole bz = new DbConsole(tx);
                        List<FtpSalesFilesModel> etys = bz.Retrieve<FtpSalesFilesModel>(x => x.Name == tstr);
                        if (etys.Count <= 0)
                        {
                            FtpSalesFilesModel ety = new FtpSalesFilesModel();
                            ety.Name = tstr;
                            ety.Memo = err_msg;
                            ety.Recv_Time = DateTime.Now;
                            ety.Status = "E";
                            c = bz.Create(ety);
                        }
                    }
                }
                tx.CommitTrans();

            }
            catch (Exception err)
            {
                tx.AbortTrans();
                WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
            return filesInTerm;
        }

        /// <summary>
        /// 处理单文件内容
        /// </summary>
        /// <param name="shortcutKey"></param>
        /// <param name="file"></param>
        private static void ProcessSingleFile(string shortcutKey, FtpSalesFile file)
        {
            string lease_number = ""; //每个文件的内容必定只属于一个lease_number。
            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                int cnt = 0;
                DbConsole bz = new DbConsole(tx);
                List<FtpSalesFilesModel> etys = bz.Retrieve<FtpSalesFilesModel>(x => x.Name == file.name || (x.shortcut_key == shortcutKey && x.file_date == file.date));
                if (etys.Count > 0)
                {
                    //如果文件存在不作处理
                    WriteLog(file.name + "文件已存在");
                }
                else
                {
                    Console.Write("file name:" + file.path + "/" + file.name + " ... ");
                    FtpClient fl = new FtpClient(Global.FTP_IP, Global.FTP_USER_NAME, Global.FTP_PASSWORD, "/" + shortcutKey + "/" + file.path + "/" + file.name);
                    List<string> lines = fl.GetFtpFileContent();

                    FtpSalesFilesModel ety = new FtpSalesFilesModel();
                    ety.Name = file.name;
                    ety.Status = "A";
                    ety.Recv_Time = DateTime.Now;
                    ety.File_Type = "L";        // L=List
                    ety.Memo = lines.Count > 0 ? "" : "文件内容为空";
                    ety.shortcut_key = shortcutKey;
                    ety.file_date = file.date;
                    cnt = bz.Create(ety);
                    int file_id = Aid.Null2Int(tx.ExecuteScalar("select seq_fsf_id.Currval from dual"));

                    for (int i = 0; i < lines.Count; i++)
                    {
                        /* 0    1     2                   3      4      5  6 7
                         * 9118,22623,2021-09-02 10:37:05,100.00,100338,01, ,
                         * 9118,22625,2021-09-02 11:56:01,160.00,100338,01, ,                         
                         */
                        string[] arr = lines[i].Split(',');

                        if (arr.Length != 8)
                            throw new Exception("文件内容字段错误:[" + lines[i] + "]");
                        else if (arr[2].Length > 19)
                            throw new Exception("日期格式错误:[" + arr[2] + "]");

                        //这里的deal_date用来获取文件内容属于那个租约(非常关键)
                        DateTime deal_date = DateTime.Parse(arr[2]);
                        ShortcutKeyQuery t = Global.ShortCutKeys.Find(x => x.Lease_Term_From <= deal_date && x.Lease_Term_To >= deal_date && x.Shortcut_Key == shortcutKey);
                        lease_number = t.Lease_Number;

                        FtpSalesListModel lst = new FtpSalesListModel();
                        lst.shop_id = shortcutKey;
                        lst.file_id = file_id;
                        lst.pos_id = arr[0];
                        lst.trade_no = arr[1];
                        lst.trade_time = DateTime.Parse(arr[2]);
                        lst.deal_date = Aid.Date2Str(deal_date, Aid.DateFmt.Standard);
                        lst.trade_amt = Aid.DbNull2Decimal(arr[3]);
                        lst.prod_id = arr[4];
                        lst.pay_code = arr[5];
                        lst.lease_number = lease_number;
                        lst.status = "I"; //插入数据时状态为“I”
                        cnt = bz.Create(lst);
                    }

                    List<FtpSalesFilesModel> etyNew = bz.Retrieve<FtpSalesFilesModel>(x => x.Id == file_id);
                    if (etyNew.Count > 0)
                    {
                        etyNew[0].Lease_Number = lease_number;
                        cnt = bz.Update(etyNew[0]);
                    }

                    Console.WriteLine("ok");
                }
                tx.CommitTrans();

            }
            catch (Exception err)
            {
                tx.AbortTrans();
                WriteSingleFtpError(file.name, "", err);
                Console.WriteLine("failed");
            }
        }

        /// <summary>
        /// 处理三文件内容
        /// </summary>
        /// <param name="shortcutKey"></param>
        /// <param name="file"></param>
        private static void ProcessTripleFile(string shortcutKey, FtpSalesFile file)
        {
            string lease_number = ""; //每个文件的内容必定只属于一个lease_number。
            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                int cnt = 0;
                DbConsole bz = new DbConsole(tx);
                List<FtpSalesFilesModel> etys = bz.Retrieve<FtpSalesFilesModel>(x => x.Name == file.name || (x.shortcut_key == shortcutKey && x.file_date == file.date));
                if (etys.Count > 0)
                {
                    //如果文件存在不作处理
                    WriteLog(file.name + "文件已存在");
                    lease_number = etys[0].Lease_Number;
                }
                else
                {
                    // 取main文件
                    Console.Write("file name:" + file.path + "/" + file.name + " ... ");
                    FtpClient flMain = new FtpClient(Global.FTP_IP, Global.FTP_USER_NAME, Global.FTP_PASSWORD, "/" + shortcutKey + "/" + file.path + "/" + file.name);
                    List<string> mainLines = flMain.GetFtpFileContent();

                    List<string> dtlLines = new List<string>();
                    string dtlName = file.name.Replace("MAIN", "DETAIL");
                    var d = filesInKey.Find(x => x.name == dtlName);
                    if (d != null)
                    {
                        FtpClient flDtl = new FtpClient(Global.FTP_IP, Global.FTP_USER_NAME, Global.FTP_PASSWORD, "/" + shortcutKey + "/" + d.path + "/" + d.name);
                        dtlLines = flDtl.GetFtpFileContent();
                    }
                    List<string> payLines = new List<string>();
                    string payName = file.name.Replace("MAIN", "PAY");
                    var p = filesInKey.Find(x => x.name == payName);
                    if (p != null)
                    {
                        FtpClient flDtl = new FtpClient(Global.FTP_IP, Global.FTP_USER_NAME, Global.FTP_PASSWORD, "/" + shortcutKey + "/" + p.path + "/" + p.name);
                        payLines = flDtl.GetFtpFileContent();
                    }

                    /// 保存文件主表
                    FtpSalesFilesModel ety = new FtpSalesFilesModel();
                    ety.Name = file.name;
                    ety.Status = "A";
                    ety.Recv_Time = DateTime.Now;
                    ety.File_Type = "T";        // T = triple
                    ety.Memo = mainLines.Count > 0 ? "" : "文件内容为空";
                    ety.shortcut_key = shortcutKey;
                    ety.file_date = file.date;
                    cnt = bz.Create(ety);
                    int file_id = Aid.Null2Int(tx.ExecuteScalar("select seq_fsf_id.Currval from dual"));

                    /// 保存main主表
                    for (int i = 0; i < mainLines.Count; i++)
                    {
                        /* 0            1    2          3 4                   5       6  7  8  9 10  11
                         * WHTSB1B1119X,9112,2860457497,1,2021-03-18 14:16:10,4    ,  0,  , 1, 1, 0,
                         * WHTSB1B1119X,9112,2860889068,1,2021-03-18 15:33:20,9.35 ,  0,  , 1, 1, 0,
                         */
                        string[] arr = mainLines[i].Split(',');

                        //这里的deal_date用来获取文件内容属于那个租约(非常关键)
                        DateTime deal_date = DateTime.Parse(arr[4]);
                        ShortcutKeyQuery t = Global.ShortCutKeys.Find(x => x.Lease_Term_From <= deal_date && x.Lease_Term_To >= deal_date && x.Shortcut_Key == shortcutKey);
                        lease_number = t.Lease_Number;

                        FtpSalesMainModel etyMain = new FtpSalesMainModel();
                        etyMain.deal_date = Aid.Date2Str(DateTime.Parse(arr[4]), Aid.DateFmt.Standard);
                        etyMain.detail_cnt = Aid.Null2Int(arr[8]);
                        etyMain.err_msg = "";
                        etyMain.file_id = file_id;
                        etyMain.lease_number = lease_number;
                        etyMain.pos_id = arr[1];
                        etyMain.pro_amt = Aid.Null2Dec(arr[6]);
                        etyMain.ref_no = arr[7];
                        etyMain.shop_id = arr[0];
                        etyMain.status = "I";
                        etyMain.tender_cnt = Aid.Null2Int(arr[9]);
                        etyMain.trade_amt = Aid.Null2Dec(arr[5]);
                        etyMain.trade_no = arr[2];
                        etyMain.trade_time = DateTime.Parse(arr[4]);
                        cnt = bz.Create(etyMain);
                    }
                    /// 保存detail子表
                    for (int j = 0; j < dtlLines.Count; j++)
                    {
                        /* 0            1    2          3     4      5    6 7    8 9
                         * WHTSB1B1119X,9112,2860457497,00001,100304,4,   1,   4,0,
                         * WHTSB1B1119X,9112,2860889068,00001,100304,9.35,1,9.35,0,
                         */
                        string[] aDtl = dtlLines[j].Split(',');
                        FtpSalesDetailModel etyDtl = new FtpSalesDetailModel();
                        etyDtl.shop_id = aDtl[0];
                        etyDtl.pos_id = aDtl[1];
                        etyDtl.trade_no = aDtl[2];
                        etyDtl.prod_id = aDtl[3];
                        etyDtl.prod_code = aDtl[4];
                        etyDtl.price = Aid.Null2Dec(aDtl[5]);
                        etyDtl.qty = Aid.Null2Dec(aDtl[6]);                      
                        etyDtl.amount = Aid.Null2Dec(aDtl[7]);
                        etyDtl.pro_amt = Aid.Null2Dec(aDtl[8]);
                        etyDtl.memo = aDtl[9];
                        cnt = bz.Create(etyDtl);
                    }
                    /// 保存pay子表
                    for (int k = 0; k < payLines.Count; k++)
                    {
                        /* 0            1    2           3 4  5    6  
                         * WHTSB1B1119X,9112,2860457497,01,06,   4, 
                         * WHTSB1B1119X,9112,2860889068,01,06,9.35,
                         */
                        string[] aPay = payLines[k].Split(',');
                        FtpSalesPayModel etyPay = new FtpSalesPayModel();
                        etyPay.shop_id = aPay[0];
                        etyPay.pos_id = aPay[1];
                        etyPay.trade_no = aPay[2];
                        etyPay.pay_id = aPay[3];
                        etyPay.tender = aPay[4];
                        etyPay.amount = Aid.Null2Dec(aPay[5]);
                        etyPay.memo = aPay[6];
                        cnt = bz.Create(etyPay);
                    }

                    List<FtpSalesFilesModel> etyNew = bz.Retrieve<FtpSalesFilesModel>(x => x.Id == file_id);
                    if (etyNew.Count > 0)
                    {
                        etyNew[0].Lease_Number = lease_number;
                        cnt = bz.Update(etyNew[0]);
                    }
                }

                tx.CommitTrans();
                Console.WriteLine("ok");
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                Console.WriteLine("failed with error:" + err.Message);
            }
        }

        private static void ComputeTurnover()
        {
            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                int cnt = 0;
                DbConsole bz = new DbConsole(tx);
                List<FtpSalesListModel> etysList = bz.Retrieve<FtpSalesListModel>(x => x.status == "I");

                foreach (var i in etysList.GroupBy(ii => ii.lease_number))
                {
                    foreach (var j in i.GroupBy(jj => jj.deal_date))
                    {
                        decimal amt = j.Sum(y => y.trade_amt);
                        if (Trans2P1(i.Key, j.Key, amt))
                        {
                            string sql = string.Format("update {2} set status='A' where lease_number='{0}' and deal_date='{1}'", i.Key, j.Key, FtpSalesListModel.TABLE_NAME);
                            cnt = tx.ExecuteNonQuery(sql);
                            Console.WriteLine("lease_number:" + i.Key.Trim() + ";deal_date:" + j.Key + ";amount:" + amt.ToString());
                        }
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
            }

            tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                int cnt = 0;
                DbConsole bz = new DbConsole(tx);
                List<FtpSalesMainModel> etysList = bz.Retrieve<FtpSalesMainModel>(x => x.status == "I");
                foreach (var i in etysList.GroupBy(ii => ii.lease_number))
                {
                    foreach (var j in i.GroupBy(jj => jj.deal_date))
                    {
                        decimal amt = j.Sum(y => y.trade_amt);
                        if (Trans2P1(i.Key, j.Key, amt))
                        {
                            string sql = string.Format("update {2} set status='A' where lease_number='{0}' and deal_date='{1}'", i.Key, j.Key, FtpSalesMainModel.TABLE_NAME);
                            cnt = tx.ExecuteNonQuery(sql);
                            Console.WriteLine("lease_number:" + i.Key.Trim() + ";deal_date:" + j.Key + ";amount:" + amt.ToString());
                        }
                    }
                }
                tx.CommitTrans();
            }
            catch (Exception)
            {
                tx.AbortTrans();
            }
        }

        private static bool Trans2P1(string lease_number, string deal_date, decimal amount)
        {
            int c = 0;
            DbTx tx = new DbTx(DbVendor.Oracle, Global.FTPDB_CONN_STR, true);
            try
            {
                DbConsole bz = new DbConsole(tx);
                List<TrDailyTurnoverModel> etys = bz.Retrieve<TrDailyTurnoverModel>(x => x.lease_number == lease_number && x.deal_date == DateTime.Parse(deal_date));
                if (etys.Count > 0)
                {
                    if (etys[0].status == "DC") //已经审批了就不能再处理了
                    {

                    }
                    else
                    {

                    }
                }
                else
                {
                    Console.Write(" Insert ");
                    TrDailyTurnoverModel ety = new TrDailyTurnoverModel();
                    ety.id = Aid.DbNull2Int(tx.ExecuteScalar("select nvl(max(id + 1),1) as maxid from tr_daily_turnover"));
                    ety.deal_date = DateTime.Parse(deal_date);
                    ety.creation_by = "sysadmin";
                    ety.cn_creation_by = "ftp task";
                    ety.creation_date = DateTime.Now;
                    ety.deal_amount = amount;
                    ety.lease_number = lease_number;
                    ety.shortcut_key = "A[1]";
                    ety.status = "DK";
                    ety.source = "P";
                    ety.tillid = "POS";
                    c = bz.Create(ety);
                }
                tx.CommitTrans();
                return true;
            }
            catch (Exception err)
            {
                tx.AbortTrans();
                WriteLog("Trans to P1 failed with :" + err.Message);
                return false;
            }
        }
        #endregion
    }

    #region model
    public class ShortcutKeyQuery
    {
        public const string TABLE_NAME = "Shortcut_key";

        public string Shortcut_Key { get; set; }
        public string Lease_Number { get; set; }
        public DateTime Lease_Term_From { get; set; }
        public DateTime Lease_Term_To { get; set; }
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
        public string shortcut_key { get; set; }
        public string file_date { get; set; }
    }

    public class FtpSalesListModel
    {
        public const string TABLE_NAME = "Ftp_Sales_List";

        [DbField(true)]
        public int file_id { get; set; }

        public string shop_id { get; set; }
        public string pos_id { get; set; }
        public string trade_no { get; set; }
        public string lease_number { get; set; }
        public DateTime trade_time { get; set; }
        public decimal trade_amt { get; set; }
        public string prod_id { get; set; }
        public string pay_code { get; set; }
        public string status { get; set; }
        public string hash { get; set; }
        public string err_msg { get; set; }
        public string deal_date { get; set; }// { return Aid.Date2Str(this.trade_time, Aid.DateFmt.Standard); }

    }

    public class FtpSalesMainModel
    {
        public const string TABLE_NAME = "Ftp_Sales_Main";

        [DbField(true)]
        public int file_id { get; set; }

        public string shop_id { get; set; }
        public string pos_id { get; set; }
        public string trade_no { get; set; }
        public string lease_number { get; set; }
        public DateTime trade_time { get; set; }
        public decimal trade_amt { get; set; }
        public decimal pro_amt { get; set; }
        public string ref_no { get; set; }
        public string status { get; set; }
        public int tender_cnt { get; set; }
        public int detail_cnt { get; set; }
        public string err_msg { get; set; }
        public string deal_date { get; set; }// { return Aid.Date2Str(this.trade_time, Aid.DateFmt.Standard); }
    }

    public class FtpSalesDetailModel
    {
        public const string TABLE_NAME = "Ftp_Sales_Detail";

        [DbField(true)]
        public string shop_id { get; set; }
        [DbField(true)]
        public string pos_id { get; set; }
        [DbField(true)]
        public string trade_no { get; set; }
        [DbField(true)]
        public string prod_id { get; set; }

        public string prod_code { get; set; }
        public decimal price { get; set; }
        public decimal qty { get; set; }
        public decimal amount { get; set; }
        public decimal pro_amt { get; set; }
        public string memo { get; set; }
    }

    public class FtpSalesPayModel
    {
        public const string TABLE_NAME = "Ftp_Sales_Pay";

        [DbField(true)]
        public string shop_id { get; set; }
        [DbField(true)]
        public string pos_id { get; set; }
        [DbField(true)]
        public string trade_no { get; set; }
        [DbField(true)]
        public string pay_id { get; set; }

        public string tender { get; set; }
        public decimal amount { get; set; }
        public string memo { get; set; }
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

    #endregion

    public class Global
    {
        public static string P1DB_CONN_STR { get; set; }
        public static string FTPDB_CONN_STR { get; set; }
        public static string FTP_IP { get; set; }
        public static string FTP_USER_NAME { get; set; }
        public static string FTP_PASSWORD { get; set; }
        public static string FTP_BAKFILE_PATH { get; set; }
        public static string FTP_ERRORFILE_PATH { get; set; }
        public static int DAYS { get; set; } //日期间隔（默认处理5天内的ftp文件，0表示所有ftp文件）
        public static List<ShortcutKeyQuery> ShortCutKeys { get; set; }
    }

    public class FtpClient
    {
        int Port { get; set; }
        string Ip { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string RemoteFilePath { get; set; }

        public FtpClient(string ip, string username, string password, int port = 21)
        {
            this.UserName = username;
            this.Password = password;
            this.Ip = ip;
            this.Port = port;
        }

        public FtpClient(string ip, string username, string password, string remoteFilePath, int port = 21)
        {
            this.UserName = username;
            this.Password = password;
            this.Ip = ip;
            this.Port = port;
            this.RemoteFilePath = remoteFilePath;
        }

        /// <summary>
        /// 列出目录内容
        /// </summary>
        public List<string> ListDir()
        {
            List<string> lines = new List<string>();
            string method = System.Net.WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse rsp = FtpCmd(method);
            //Console.WriteLine("Status Code:{0}\r\nStatus Desc:{1}\r\nPath:{2}\r\n", rsp.StatusCode, rsp.StatusDescription, rsp.ResponseUri.AbsolutePath);
            using (System.IO.StreamReader sr = new System.IO.StreamReader(rsp.GetResponseStream()))
            {
                string line = sr.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (line != "." && line != "..")
                        lines.Add(line);
                    line = sr.ReadLine();
                }
            }
            return lines;
        }

        /// <summary>
        /// 列出指定目录内容
        /// </summary>
        /// <param name="path">路径，默认根目录</param>
        /// <returns></returns>
        public List<string> ListDir(string path = "/")
        {
            List<string> lines = new List<string>();
            string method = System.Net.WebRequestMethods.Ftp.ListDirectory;
            this.RemoteFilePath = path;
            FtpWebResponse rsp = FtpCmd(method);
            using (System.IO.StreamReader sr = new System.IO.StreamReader(rsp.GetResponseStream()))
            {
                string line = sr.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (line != "." && line != "..")
                        lines.Add(line);
                    line = sr.ReadLine();
                }
            }
            return lines;
        }

        public void Download(string filePath)
        {
            //if (string.IsNullOrEmpty(this.RemoteFilePath))
            //    return;

            //string method = WebRequestMethods.Ftp.DownloadFile;
            //FtpWebResponse rsp = FtpCmd(method);
            //Console.WriteLine("Status Code:{0}\r\nStatus Desc:{1}\r\nFile Path:{2}\r\n", rsp.StatusCode, rsp.StatusDescription, rsp.ResponseUri.AbsolutePath);
            //List<string> lines = ReadLines("", rsp);
        }

        public List<string> GetFtpFileContent()
        {
            if (string.IsNullOrEmpty(this.RemoteFilePath))
                return new List<string>();

            string method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse rsp = FtpCmd(method);
            List<string> lines = ReadLines("", rsp);
            return lines;
        }

        private FtpWebResponse FtpCmd(string method)
        {
            string uri = string.Format("ftp://{0}:{1}{2}", this.Ip, this.Port, this.RemoteFilePath);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uri);
            req.UseBinary = true;
            req.UsePassive = true;
            req.Credentials = new System.Net.NetworkCredential(UserName, Password);
            req.KeepAlive = true;
            req.Method = method;
            FtpWebResponse rsp = (FtpWebResponse)req.GetResponse();
            return rsp;
        }

        private List<string> ReadLines(string filePath, FtpWebResponse rsp)
        {
            List<string> lines = new List<string>();
            System.IO.Stream s = rsp.GetResponseStream();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
            {
                string line = sr.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    lines.Add(line);
                    line = s.CanRead ? sr.ReadLine() : "";
                }

                //string row = "";
                //do
                //{
                //    row = s.CanRead ? sr.ReadLine() : "";
                //} while (!string.IsNullOrEmpty(row));
            }
            return lines;
        }
    }

    public class FtpSalesFile
    {
        public string path { get; set; }
        public string name { get; set; }
        public string date { get; set; }

        public FtpSalesFile(string p, string n)
        {
            this.path = p;
            this.name = n;
        }
    }

    public class TrTask
    {
        public TrTask()
        { }

        public void GetAllShortKeys()
        {

        }

        /*
         * 
         */

    }
}