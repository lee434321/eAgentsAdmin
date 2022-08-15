using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.IO;
using System.Data.OracleClient; //这里使用过期的类库操作oracle，因为p1数据库较老
using System.Data.SqlClient;
using System.Linq.Expressions;
namespace cn.magix.Helper
{
    public class JsonRsp
    {
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public object result { get; set; }
        public JsonRsp(int code, string msg)
        {
            this.err_code = code;
            this.err_msg = msg;
        }
        public JsonRsp()
        {
            this.err_code = 0;
            this.err_msg = "";
        }
    }

    /// <summary>
    /// 轻量助手类
    /// </summary>
    public class Aid
    {
        #region 枚举
        public enum DateFmt
        {
            /// <summary>
            /// yyyyMMdd
            /// </summary>
            Simple = 0,

            /// <summary>
            /// yyyy-mm-dd
            /// </summary>
            Standard = 1,

            /// <summary>
            /// yyyy/mm/dd 
            /// </summary>
            StandardSlash = 2,

            /// <summary>
            /// yyyy-mm-dd HH:MM:ss 
            /// </summary>
            Wholly = 3,

            /// <summary>
            /// yyyy-MM-dd HH:mm:ss.fff 带毫秒
            /// </summary>
            WhollyMs = 4,

            /// <summary>
            ///  yyyy-mm-dd HH:MM 
            /// </summary>
            YmdHm = 5,

            /// <summary>
            /// dd-mm-yyyy
            /// </summary>
            Dmy = 6
        }

        #endregion

        #region 静态属性
        public static string NewGuid { get { return System.Guid.NewGuid().ToString(); } }

        /// <summary>
        /// "LoginSystem.aspx"虚拟根目录（~/eagentsadmin/）
        /// </summary>
        public static string vPath { get; set; }

        /// <summary>
        /// "LoginSystem.aspx"物理根目录(~\eAgentsAdmin\)
        /// </summary>
        public static string yPath { get; set; }
        #endregion

        #region 时间戳处理
        //当前时间=>自1970年来的毫秒
        public static long getTimestamp()
        {
            TimeSpan ts = new TimeSpan(System.DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }
        //自1970年来的毫秒=> 时间
        public static DateTime getTime(long timestamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timestamp * 10000;
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        //自1970年来的毫秒=>时间
        public static DateTime getTime(string timestamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timestamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        //时间=>自1970年来的毫秒
        public static long getTimestamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            TimeSpan span = time - startTime;
            return (long)span.TotalMilliseconds;
        }

        #endregion

        #region 日期相关

        /// <summary>
        /// 日期转指定形式的字符串
        /// </summary>
        /// <param name="val"></param>
        /// <param name="given"></param>
        /// <returns></returns>
        public static string Date2Str(DateTime val, DateFmt given)
        {
            switch (given)
            {
                case DateFmt.Simple:
                    return val.ToString("yyyyMMdd");
                case DateFmt.Standard:
                    return val.ToString("yyyy-MM-dd");
                case DateFmt.StandardSlash:
                    return val.ToString("yyyy/MM/dd");
                case DateFmt.YmdHm:
                    return val.ToString("yyyy-MM-dd HH:mm");
                case DateFmt.Wholly:
                    return val.ToString("yyyy-MM-dd HH:mm:ss");
                case DateFmt.WhollyMs:
                    return val.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case DateFmt.Dmy:
                    return val.ToString("dd-MM-yyyy");
                default:
                    return val.ToString();
            }
        }

        #endregion

        #region 安全转换
        public static decimal DbNull2Decimal(object val)
        {
            if (val == DBNull.Value || val.ToString() == "")
                return 0;
            else
                return Convert.ToDecimal(val);
        }

        public static char DbNull2Char(object val)
        {
            if (DBNull.Value == val)
                return char.MinValue;
            else
                return Convert.ToChar(val);
        }

        public static int DbNull2Int(object val)
        {
            if (val == DBNull.Value || val.ToString() == "" || val == null)
                return 0;
            else
                return Convert.ToInt32(val);
        }

        public static string DbNull2Str(object val)
        {
            if (val == null || val == System.DBNull.Value)
                return "";
            else
                return val.ToString();
        }

        public static int Null2Int(object val)
        {
            try
            {
                return Convert.ToInt32(val);
            }
            catch
            {
                return 0;
            }
        }

        public static string Null2Str(object val)
        {
            if (val == null)
            {
                return "";
            }
            else
                return Convert.ToString(val);
        }

        public static decimal Null2Dec(object val)
        {
            if (val == null || val.ToString() == "")
            {
                return 0;
            }
            else
                return Convert.ToDecimal(val);
        }

        #endregion

        #region 其他
        public static string GenSONum(string store_id, string dept_code, string trans_id)
        {
            return "M2" + store_id + System.DateTime.Now.ToString("yyyyMMddHHmmss") + dept_code + trans_id.PadLeft(5, '0');
        }

        public static string GenOutTradeNo()
        {
            return System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString();
        }

        public static string Stream2Str(System.IO.Stream s)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(s);
            string str = sr.ReadToEnd();
            sr.Close();
            return str;
        }

        public static string Stream2Base64Str(System.IO.Stream s)
        {
            try
            {
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                return Convert.ToBase64String(buffer);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetMD5(string txt)
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

        public static void WriteLog(string filePath,string txt)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
            using (System.IO.StreamWriter sw = fi.Exists ? fi.AppendText() : fi.CreateText())
                sw.WriteLine("[" + Aid.Date2Str(DateTime.Now, Aid.DateFmt.WhollyMs) + "] " + txt);
        }
        #endregion
    }

    /// <summary>
    /// 数据库厂商枚举
    /// </summary>
    public enum DbVendor
    {
        Firebird,
        MySQL,
        SqlServer,
        Oracle
    }

    /// <summary>
    /// 自定义特性
    /// </summary>
    public class DbFieldAttribute : Attribute
    {
        public bool PrimaryKey { get; set; }
        public bool AutoIncrease { get; set; }
        public bool Ignore { get; set; }
        public string DisplayName { get; set; }
        public int length { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pk">primary key</param>
        public DbFieldAttribute(bool pk)
        {
            this.PrimaryKey = pk;
            this.AutoIncrease = false;
            this.Ignore = false;
            this.length = 0;
            this.DisplayName = "";
        }

        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="pk">primary key</param>
        /// <param name="ai">auto increase</param>
        public DbFieldAttribute(bool pk, bool ai)
            : this(pk)
        {
            this.AutoIncrease = ai;
        }

        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="pk">primary key</param>
        /// <param name="ai">auto increase</param>
        /// <param name="ig">ignore column</param>
        public DbFieldAttribute(bool pk, bool ai, bool ig)
            : this(pk, ai)
        {
            this.Ignore = ig;
        }

        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="ai"></param>
        /// <param name="ig"></param>
        /// <param name="len"></param>
        public DbFieldAttribute(bool pk, bool ai, bool ig, int len)
            : this(pk, ai, ig)
        {
            this.length = len;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pk">primary key</param>
        /// <param name="ai">auto increase</param>
        /// <param name="ig">ignore column</param>
        /// <param name="dn">display name</param>
        public DbFieldAttribute(bool pk, bool ai, bool ig, string dn)
            : this(pk, ai, ig)
        {
            this.DisplayName = dn;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dn">display name</param>
        public DbFieldAttribute(string dn)
        {
            this.DisplayName = dn;
        }
    }

    /// <summary>
    /// 数据库链接
    /// </summary>
    public class DbTx
    {
        #region declaration
        public static int ConnectionCount { get { return connCnt; } }
        private static int connCnt = 0;

        IDbConnection connection = null;
        IDbTransaction traxction = null;
        DbVendor vendor;
        public string Version
        {
            get
            {
                switch (this.vendor)
                {
                    case DbVendor.Firebird:                       
                    case DbVendor.MySQL:                        
                    case DbVendor.SqlServer:
                        return "";
                    case DbVendor.Oracle:
                        OracleConnection orclConn = this.connection as OracleConnection;
                        return orclConn.ServerVersion;                      
                    default:
                        return "";
                }
            }
        }

        #endregion

        #region constructor

        public DbTx(DbVendor vdr, string connStr, bool openTx)
        {
            try
            {
                this.vendor = vdr;
                switch (vdr)
                {
                    case DbVendor.Firebird:
                        //firebird 连接字符串样例：connstr = @"server type=Embedded;user id=sysdba;password=masterkey;initial catalog=data\embed.fdb;packet size=4096"
                        //connection = new FbConnection(connStr);
                        break;
                    case DbVendor.MySQL:
                        //connection = new MySqlConnection(connStr);
                        break;
                    case DbVendor.SqlServer:
                        connection = new SqlConnection(connStr);
                        break;
                    case DbVendor.Oracle:
                        connection = new OracleConnection(connStr);
                        break;
                    default:
                        break;
                }
                this.connection.Open();
                this.traxction = openTx ? connection.BeginTransaction() : null;
                connCnt++;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region instance methods

        /// <summary>
        /// Execute sql statement (unsafe)
        /// </summary>
        /// <param name="sql">sql statement</param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(string sql, params IDataParameter[] parameters)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(sql, parameters);
                IDataReader ir = cmd.ExecuteReader(CommandBehavior.Default);
                return ir;
            }
            catch (Exception err)
            {
                if (cmd != null)
                    cmd.Dispose();
                throw err;
            }
        }

        /// <summary>
        /// 返回结果集查询，转换为datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, params IDataParameter[] parameters)
        {
            try
            {
                DataSet ds = new DataSet();
                this.GetDataAdapter(sql, parameters).Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
                else
                {
                    return new DataTable();
                }
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        /// <summary>
        /// 返回非结果集查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params IDataParameter[] parameters)
        {
            int cnt = 0;
            try
            {
                IDbCommand command = this.GetCommand(sql, parameters);
                cnt = command.ExecuteNonQuery();
                if (command != null)
                    command.Dispose();
            }
            catch (Exception)
            {
                throw;
            }
            return cnt;            
        }

        public object ExecuteScalar(string sql, params IDataParameter[] parameters)
        {
            try
            {
                IDbCommand command = this.GetCommand(sql, parameters);
                object result = command.ExecuteScalar();
                if (command != null)
                    command.Dispose();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// 提交事务
        /// </summary>
        public void CommitTrans()
        {
            try
            {
                if (this.traxction != null)
                    this.traxction.Commit();
            }
            catch
            {
                this.traxction.Rollback();
            }
            finally
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    this.connection.Close();
                }
                connCnt--;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void AbortTrans()
        {
            try
            {
                if (this.traxction != null)
                {
                    this.traxction.Rollback();
                }
            }
            catch
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    if (this.traxction != null)
                        this.traxction.Rollback();
                    this.connection.Close();
                }
            }
            finally
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    this.connection.Close();
                }
                connCnt--;
            }
        }

        public IDataParameter BuildParameter(string name, object value)
        {
            IDataParameter para = null;
            switch (this.vendor)
            {
                case DbVendor.Firebird:
                    //para = new FbParameter(name, value);
                    break;
                case DbVendor.MySQL:
                    //para = new MySqlParameter(name, value);
                    break;
                case DbVendor.SqlServer:
                    para = new SqlParameter(name, value);
                    break;
                case DbVendor.Oracle:
                    para = new OracleParameter(name, value);
                    break;
                default:
                    break;
            }
            return para;
        }

        public string GetParamPrefix()
        {
            switch (this.vendor)
            {
                case DbVendor.Oracle:
                    return ":p";

                case DbVendor.Firebird:
                case DbVendor.MySQL:
                case DbVendor.SqlServer:
                    return "@p";

                default:
                    throw new Exception("Database vendor not found for parameter prefix!");
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// 获取command接口对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IDbCommand GetCommand(string sql, params IDataParameter[] parameters)
        {
            IDbCommand command = null;
            switch (this.vendor)
            {
                case DbVendor.Oracle:                    
                    command = new OracleCommand(sql, this.connection as OracleConnection, this.traxction as OracleTransaction);
                    break;
                case DbVendor.MySQL:
                    //command = new MySqlCommand(sql, this.connection as MySqlConnection, this.traxction as MySqlTransaction);
                    break;
                case DbVendor.Firebird:
                    //command = new FbCommand(sql, this.connection as FbConnection, this.traxction as FbTransaction);
                    break;
                case DbVendor.SqlServer:
                    command = new SqlCommand(sql, this.connection as SqlConnection, this.traxction as SqlTransaction);
                    break;
                default:
                    throw new Exception("Database vendor unknown!");
            }

            if (parameters != null)
            {
                foreach (IDataParameter item in parameters)
                    command.Parameters.Add(item);
            }
            return command;
        }

        /// <summary>
        /// 获取dataadapter对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IDataAdapter GetDataAdapter(string sql, params IDataParameter[] parameters)
        {
            IDataAdapter ida = null;
            IDbCommand command = this.GetCommand(sql, parameters);
            switch (this.vendor)
            {
                case DbVendor.Firebird:
                    //ida = new FbDataAdapter(command as FbCommand);
                    break;
                case DbVendor.MySQL:
                    //ida = new MySqlDataAdapter(command as MySqlCommand);
                    break;
                case DbVendor.SqlServer:
                    ida = new SqlDataAdapter(command as SqlCommand);
                    break;
                case DbVendor.Oracle:
                    ida = new OracleDataAdapter(command as OracleCommand);
                    break;
                default:
                    break;
            }
            return ida;
        }

        #endregion

        #region static methods
       
        #endregion
    }

    /// <summary>
    /// Lightweight ORM manipulator
    /// </summary>
    public class DbConsole
    {
        enum DmlType
        {
            Select,
            Update,
            Delete,
            Insert
        }

        const string FIELD_TABLE_NAME = "TABLE_NAME";           // entity table name property
        List<PropertyInfo> fnvs = new List<PropertyInfo>();     // fields and values =>fnvs
        List<PropertyInfo> keys = new List<PropertyInfo>();     // primary keys     
        List<IDataParameter> paras = new List<IDataParameter>();// parameters safer.                
        DbTx m_tx;

        public DbConsole(DbTx tx) { this.m_tx = tx; }

        /// <summary>
        /// 执行返回结果集的lambda表达式查询，并转换为泛型实体列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public List<T> Retrieve<T>(Expression<Func<T, bool>> expr) where T : class
        {
            try
            {
                T ety = Activator.CreateInstance<T>();
                string sql = "select * from " + typeof(T).GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                sql += " where " + (expr == null ? "1=1" : new ExprVistor().Parse(expr));
                using (System.Data.IDataReader idr = this.m_tx.ExecuteReader(sql))
                {
                    List<T> etys = this.FillBy<T>(idr);
                    return etys;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// 执行返回结果集的原生sql查询，并转换为泛型实体列表。
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="sql">查询语句</param>
        /// <returns>结果集对象</returns>
        /// 建议先定义"T"参数类。
        public List<T> Retrieve<T>(string sql, params IDataParameter[] parameters) where T : class
        {           
            try
            {
                string SQLQUERY = sql.ToUpper();
                if (SQLQUERY.Contains("DELETE") || SQLQUERY.Contains("TRUNCATE") || SQLQUERY.Contains("DROP") || SQLQUERY.Contains("ALTER"))
                    throw new Exception("can not use 'delete'\\'drop'\\'truncate'\\'alter' query.");

                using (System.Data.IDataReader idr = this.m_tx.ExecuteReader(sql, parameters))
                {
                    List<T> etys = this.FillBy<T>(idr);
                    return etys;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// 执行返回结果集的原生sql查询，并转换为datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable RetrieveDatatable(string sql, params IDataParameter[] parameters)
        {
            System.Data.IDataReader reader = null;
            try
            {
                string SQLQUERY = sql.ToUpper();
                if (SQLQUERY.Contains("DELETE") || SQLQUERY.Contains("TRUNCATE") || SQLQUERY.Contains("DROP"))
                    throw new Exception("can not use 'delete'\\'drop'\\'truncate' query.");
                DataTable dt = this.m_tx.ExecuteDataTable(sql, parameters);
                return dt;
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        public int Create<T>(T ety) where T : class
        {
            try
            {
                int cnt = 0;
                string sql = GenInsertSqlSafer<T>(ety);
                cnt = m_tx.ExecuteNonQuery(sql, paras.ToArray());
                return cnt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        public int Update<T>(T ety) where T : class
        {
            try
            {
                string sql = this.GenUpdateSqlSafer<T>(ety);
                int cnt = this.m_tx.ExecuteNonQuery(sql, this.paras.ToArray());
                return cnt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        public int Delete<T>(T ety) where T : class
        {
            try
            {
                this.GetSchema(ety, DmlType.Delete);
                if (keys.Count == 0)
                    throw new Exception("primary key not exists,can not execute <delete> statement!");

                string tableName = ety.GetType().GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                if (tableName.Length <= 0)
                    throw new Exception("table name not exists!");

                string sql = "delete from " + tableName;
                sql += " where 1=1 ";

                foreach (PropertyInfo pi in keys)
                {
                    switch (Type.GetTypeCode(pi.PropertyType))
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                            sql += " and " + pi.Name + "=" + Aid.DbNull2Str(pi.GetValue(ety, null));
                            break;
                        default:
                            sql += " and " + pi.Name + "='" + Aid.DbNull2Str(pi.GetValue(ety, null)) + "'";
                            break;
                    }
                }
                int cnt = this.m_tx.ExecuteNonQuery(sql);
                return cnt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 构建参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDataParameter BuildParameter(string name, object value)
        {
            return this.m_tx.BuildParameter(name, value);
        }

        /// <summary>
        /// 生成插入语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        private string GenInsertSqlSafer<T>(T ety)
        {
            this.GetSchema(ety, DmlType.Insert);
            paras.Clear(); //清除已有参数

            FieldInfo fi = ety.GetType().GetField(FIELD_TABLE_NAME);
            if (fi == null)
                throw new Exception("table name undefined!");

            string sql = "insert into " + fi.GetValue(ety).ToString();
            string cols = "", vals = "";
            foreach (PropertyInfo pi in fnvs)
            {
                /// 处理DbField
                object[] attrs = pi.GetCustomAttributes(false);
                if (attrs.Length > 0)
                {
                    DbFieldAttribute attr = attrs[0] as DbFieldAttribute;
                    if (attr.AutoIncrease || attr.Ignore)
                        continue;
                }
                /// 处理空值
                if (pi.GetValue(ety, null) == null)
                    continue;
                /// 生成字段列表 
                cols += pi.Name + ",";
                /// 生成值列表                
                vals += Prop2Value(pi.PropertyType, pi.GetValue(ety, null), pi.Name) + ",";
            }
            cols = cols.Substring(0, cols.Length - 1);
            vals = vals.Substring(0, vals.Length - 1);
            sql += " (" + cols + ")" + " values (" + vals + ")";
            return sql;
        }

        /// <summary>
        /// 生成更新语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        private string GenUpdateSqlSafer<T>(T ety)
        {
            this.GetSchema(ety, DmlType.Update);
            if (keys.Count == 0)
                throw new Exception("primary key not exists,can not execute update statement!");

            paras.Clear();
            string sql = "update " + ety.GetType().GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
            sql += " set ";

            foreach (PropertyInfo pif in this.fnvs) //pif->propertyinfo field
            {
                if (pif.GetValue(ety, null) == null) //如果为空则不做处理。
                    continue;
                sql += pif.Name + " = " + Prop2Value(pif.PropertyType, pif.GetValue(ety, null), pif.Name) + ",";
            }
            sql = sql.Substring(0, sql.Length - 1);

            sql += " where 1=1";
            foreach (PropertyInfo pik in keys) //pik->propertyinfo key
            {
                sql += " and " + pik.Name + " = " + Prop2Value(pik.PropertyType, pik.GetValue(ety, null), pik.Name);
            }
            return sql;
        }

        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <param name="tag"></param>
        private void GetSchema<T>(T ety, DmlType tag)
        {
            this.fnvs.Clear();
            this.keys.Clear();

            switch (tag)
            {
                case DmlType.Select:
                    break;
                case DmlType.Update:
                case DmlType.Delete:
                    foreach (PropertyInfo pi in ety.GetType().GetProperties())
                    {
                        object[] attr = pi.GetCustomAttributes(false);
                        if (attr.Length > 0)
                        {
                            DbFieldAttribute fieldAttr = attr[0] as DbFieldAttribute;
                            if (fieldAttr != null)
                            {
                                if (fieldAttr.Ignore)
                                    continue;

                                else if (fieldAttr.PrimaryKey)
                                {
                                    this.keys.Add(pi);
                                    continue;
                                }
                            }
                        }
                        this.fnvs.Add(pi);
                    }
                    break;

                case DmlType.Insert:
                    foreach (PropertyInfo pi in ety.GetType().GetProperties())
                    {
                        object[] attr = pi.GetCustomAttributes(false);
                        if (attr.Length > 0)
                        {
                            DbFieldAttribute fieldAttr = attr[0] as DbFieldAttribute;
                            if (fieldAttr != null)
                            {
                                if (fieldAttr.AutoIncrease || fieldAttr.Ignore)
                                    continue;
                            }
                        }
                        this.fnvs.Add(pi);
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 获取参数前缀
        /// </summary>
        /// <returns></returns>
        private string GetParamPrefix()
        {
            return this.m_tx.GetParamPrefix();
        }

        /// <summary>
        /// Fill entity by reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>entity list</returns>
        private List<T> FillBy<T>(IDataReader reader) where T : class
        {
            PropertyInfo pi_debug = null;
            try
            {
                PropertyInfo[] pis = typeof(T).GetProperties();

                DataTable schema = reader.GetSchemaTable();
                HashSet<string> sc = new HashSet<string>();
                for (int j = 0; j < schema.Rows.Count; j++)
                    sc.Add(schema.Rows[j][0].ToString().Trim().ToLower());

                List<T> etys = new List<T>();
                while (reader.Read())
                {
                    T ety1 = Activator.CreateInstance<T>();
                    foreach (PropertyInfo pi in pis)
                    {
                        pi_debug = pi;
                        /// 处理DbFieldAttribute
                        object[] attrs = pi.GetCustomAttributes(false);
                        if (attrs.Length > 0)
                        {
                            DbFieldAttribute attr = attrs[0] as DbFieldAttribute;

                            if (attr == null)
                            {
                                DbFieldAttribute attr2 = attrs[0] as DbFieldAttribute;
                                if (attr2.Ignore)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (attr.Ignore)
                                    continue;
                            }
                        }

                        // 这里最好补一个字段存在性检查                       
                        if (!sc.Contains(pi.Name.Trim().ToLower()))
                            throw new Exception("Field: '" + pi.Name + "' not exists.");

                        if (reader[pi.Name] == DBNull.Value)
                            continue;

                        switch (Type.GetTypeCode(pi.PropertyType))
                        {
                            case TypeCode.Boolean:
                                break;
                            case TypeCode.Char:
                                pi.SetValue(ety1, Convert.ToChar(reader[pi.Name]), null);
                                break;
                            case TypeCode.DateTime:
                                /// hint:以下需要模拟测试数据库表字段不允许为空情况
                                if (!reader[pi.Name].Equals(DBNull.Value))
                                    pi.SetValue(ety1, Convert.ToDateTime(reader[pi.Name]), null);
                                break;
                            case TypeCode.Double:
                                pi.SetValue(ety1, Convert.ToDouble(reader[pi.Name]), null);
                                break;
                            case TypeCode.Decimal:
                                pi.SetValue(ety1, Convert.ToDecimal(reader[pi.Name]), null);
                                break;
                            case TypeCode.Int16:
                                pi.SetValue(ety1, Convert.ToInt16(reader[pi.Name]), null);
                                break;
                            case TypeCode.Int32:
                                pi.SetValue(ety1, Convert.ToInt32(reader[pi.Name]), null);
                                break;
                            case TypeCode.Int64:
                                pi.SetValue(ety1, Convert.ToInt64(reader[pi.Name]), null);
                                break;
                            case TypeCode.UInt64:
                                pi.SetValue(ety1, Convert.ToUInt64(reader[pi.Name]), null);
                                break;
                            default:
                                if (pi.PropertyType.IsGenericType) //如果是泛型（后续要改进）
                                {
                                    Type t = pi.PropertyType.GetGenericArguments()[0];
                                    if (pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))//如果是可空类型
                                    {
                                        pi.SetValue(ety1, Nullalble2Prop(t, reader[pi.Name]), null);
                                    }
                                    else
                                    {
                                        //暂不实现泛型引用的赋值
                                    }
                                }
                                else if (pi.PropertyType.IsArray && pi.PropertyType.GetElementType() == typeof(byte)) //如果是字节数组,则是blob数据类型
                                {
                                    pi.SetValue(ety1, reader[pi.Name], null);
                                }
                                else
                                {
                                    pi.SetValue(ety1, reader[pi.Name].ToString(), null);
                                }
                                break;
                        }
                    }
                    etys.Add(ety1);
                }
                return etys;
            }
            catch (Exception err)
            {
                reader.Close();
                throw new Exception(err.Message + "\r\n" + err.StackTrace + "\r\n" + pi_debug.Name + " " + pi_debug.PropertyType.Name);
            }
        }

        private object Nullalble2Prop(Type t, object v)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(v);
                case TypeCode.Byte:
                    return Convert.ToByte(v);
                case TypeCode.Char:
                    return Convert.ToChar(v);
                case TypeCode.DBNull:
                    return System.DBNull.Value;
                case TypeCode.DateTime:
                    return Convert.ToDateTime(v);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(v);
                case TypeCode.Double:
                    return Convert.ToDouble(v);
                case TypeCode.Empty:
                    return Convert.DBNull;
                case TypeCode.Int16:
                    return Convert.ToInt16(v);
                case TypeCode.Int32:
                    return Convert.ToInt32(v);
                case TypeCode.Int64:
                    return Convert.ToInt64(v);
                case TypeCode.SByte:
                    return Convert.ToSByte(v);
                case TypeCode.Single:
                    return Convert.ToSingle(v);
                case TypeCode.String:
                    return Convert.ToString(v);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(v);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(v);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(v);
                default:
                    return null;
            }
        }

        private string Prop2Value(Type t, object val, string propName)
        {
            /// 适用于insert ,update
            string rtnVal = "";
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    break;

                case TypeCode.Boolean:
                    if (Convert.ToBoolean(val))
                        rtnVal = "'1'";
                    else
                        rtnVal = "'0'";
                    break;

                case TypeCode.Char:
                    rtnVal = "'" + Aid.Null2Str(val) + "'";
                    break;
                case TypeCode.Byte:
                    rtnVal = Aid.Null2Str(Convert.ToByte(val));
                    break;
                case TypeCode.Decimal:
                    rtnVal = Aid.Null2Str(Convert.ToDecimal(val));
                    break;
                case TypeCode.Double:
                    rtnVal = Aid.Null2Str(Convert.ToDouble(val));
                    break;
                case TypeCode.Int16:
                    rtnVal = Aid.Null2Str(Convert.ToInt16(val));
                    break;
                case TypeCode.Int32:
                    rtnVal = Aid.Null2Str(Convert.ToInt32(val));
                    break;
                case TypeCode.Int64:
                    rtnVal = Aid.Null2Str(Convert.ToInt64(val));
                    break;
                case TypeCode.Single:
                    rtnVal = Aid.Null2Str(Convert.ToSingle(val));
                    break;
                case TypeCode.SByte:
                    rtnVal = Aid.Null2Str(Convert.ToSByte(val));
                    break;
                case TypeCode.UInt16:
                    rtnVal = Aid.Null2Str(Convert.ToUInt16(val));
                    break;
                case TypeCode.UInt32:
                    rtnVal = Aid.Null2Str(Convert.ToUInt32(val));
                    break;
                case TypeCode.UInt64:
                    rtnVal = Aid.Null2Str(Convert.ToUInt64(val));
                    break;

                default: //不是以上任何类型
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) //如果是可空类型，则递归调用
                        rtnVal = Prop2Value(t.GetGenericArguments()[0], val, propName);
                    else//否则就是字符串类型 (日期类型也使用参数化形式)
                    {
                        if (propName.ToUpper() == "LAST_UPDATE")
                            val = DateTime.Now;
                        string qpName = GetParamPrefix() + propName;
                        paras.Add(m_tx.BuildParameter(qpName, val));
                        rtnVal = qpName;
                    }
                    break;
            }
            return rtnVal;
        }
    }

    /// <summary>
    /// 表达式访问器类，用于生成sql条件子句
    /// </summary>
    public class ExprVistor : System.Linq.Expressions.ExpressionVisitor
    {
        DbVendor vdr;
        /// <summary>
        /// 分析表达式并返回子句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public string Parse<T>(Expression<Func<T, bool>> expr,DbVendor vendor= DbVendor.Oracle) where T : class
        {
            vdr = vendor;
            if (expr.Body.ToString() == "True") //如果是True，说明表达式来自predicate子句的初始值。        
                return "1=1";
            else
            {
                string clauses = Involve(expr.Body);
                return clauses;
            }
        }        

        /// <summary>
        /// 解析入口
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private string Involve(Expression expr)
        {
            string name = expr.GetType().Name;
            switch (name)
            {
                case "LogicalBinaryExpression":
                case "MethodBinaryExpression":
                    return InvolveBinary(expr as BinaryExpression);

                case "PropertyExpression":
                case "FieldExpression":
                    return InvolveMember(expr as MemberExpression);

                case "InstanceMethodCallExpressionN":
                case "MethodCallExpressionN":
                    return InvolveMethodCall(expr as MethodCallExpression);

                case "ConstantExpression":
                    return InvolveConstant(expr as ConstantExpression);

                case "InvocationExpression":
                    return InvolveInvocation(expr as InvocationExpression);

                default:
                    break;
            }
            return "";
        }

        /// <summary>
        /// 解析lambda表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string InvolveInvocation(InvocationExpression node)
        {
            LambdaExpression le = node.Expression as LambdaExpression;
            if (le != null)
            {
                return Involve(le.Body);
            }
            else
                return "";
        }

        /// <summary>
        /// 常量解析
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string InvolveConstant(ConstantExpression node)
        {
            return this.Eval(node.Value);
        }

        /// <summary>
        /// 方法调用解析
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string InvolveMethodCall(MethodCallExpression node)
        {
            string txt = "";
            if (node.Method.Name == "Sql_In")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                txt += " IN ";
                txt += this.GetMethodCallInVals(node.Arguments[1] as MemberExpression);
            }
            else if (node.Method.Name == "Sql_Like")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                object result = MethodCall2Value(node.Arguments[1]);
                txt += " LIKE '%" + result.ToString() + "%'";
            }
            else
            {
                object result = MethodCall2Value(node);
                if (result.GetType().Name == "String")
                    txt += "'" + result.ToString() + "'";
                else if (result.GetType().Name == "DateTime")
                {
                    if (vdr == DbVendor.Oracle)
                    {
                        txt += "to_date('" + Aid.Date2Str(DateTime.Parse(result.ToString()), Aid.DateFmt.Wholly) + "','yyyy-mm-dd hh24:mi:ss')";
                    }
                    else
                        txt += "'" + result.ToString() + "'";
                }
                else
                    txt += result.ToString();
            }
            return txt;
        }

        /// <summary>
        /// 二元解析
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string InvolveBinary(BinaryExpression node)
        {
            string clause = " (";
            if (node.Left.ToString() == "True")//处理初始条件为true的情况        
                clause += "1=1";
            else
                clause += Involve(node.Left);

            clause += GetOperStr(node.NodeType);
            clause += Involve(node.Right);
            clause += " )";

            return clause;
        }

        /// <summary>
        /// 成员变量解析
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string InvolveMember(MemberExpression node)
        {
            if (node.ToString().Contains("value")) //如果包含value就是取表达式值
            {
                object result = this.MethodCall2Value(node);
                if (result.GetType().Name == "String")
                {
                    return "'" + result.ToString() + "'";
                }
                else
                {
                    return result.ToString();
                }
            }
            else //否则就是取字段名
            {
                return node.Member.Name;
            }
        }

        /// <summary>
        /// 取“IN”子句表达式（待完善）
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// 后期要考虑各种值类型的情况
        private string GetMethodCallInVals(MemberExpression expr)
        {
            object vals = MethodCall2Value(expr);

            Type t2 = vals.GetType().GetGenericArguments()[0];
            dynamic items = vals as System.Collections.IEnumerable;
            string in_clauses = "(";
            foreach (var item in items)
            {
                if (t2.Name == "String")//如果是字符串（泛型子类）
                {
                    in_clauses += "'" + item + "',";
                }
                else
                {
                    in_clauses += item + ",";
                }
            }
            in_clauses = in_clauses.Substring(0, in_clauses.Length - 1);
            in_clauses += ")";
            return in_clauses;
        }

        private object MethodCall2Value(Expression expr)
        {
            UnaryExpression cast = Expression.Convert(expr, typeof(object));
            object obj = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
            return obj;
        }

        private string Eval(object val)
        {
            Type t = val.GetType();
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return "Null";
                case TypeCode.Boolean:
                    if (Convert.ToBoolean(val))
                        return "'1'";
                    else
                        return "'0'";
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                    return Convert.ToInt16(val).ToString();
                case TypeCode.UInt16:
                    return Convert.ToUInt16(val).ToString();
                case TypeCode.Int32:
                    return Convert.ToInt32(val).ToString();
                case TypeCode.UInt32:
                    return Convert.ToUInt32(val).ToString();
                case TypeCode.Int64:
                    return Convert.ToInt64(val).ToString();
                case TypeCode.UInt64:
                    return Convert.ToUInt64(val).ToString();
                case TypeCode.Single:
                    return Convert.ToSingle(val).ToString();
                case TypeCode.Double:
                    return Convert.ToDouble(val).ToString();
                case TypeCode.Decimal:
                    return Convert.ToDecimal(val).ToString();
                case TypeCode.DateTime:
                    return "'" + Aid.Date2Str(Convert.ToDateTime(val), Aid.DateFmt.WhollyMs) + "'";
                case TypeCode.String:
                    return "'" + val.ToString() + "'";
                default:
                    if (t.IsGenericType) //如果是泛型(这里需要注意,必须是"list"或"enumerable"类型("List`1"))
                    {
                        Type t2 = t.GetGenericArguments()[0];
                        dynamic items = val as System.Collections.IEnumerable;
                        string in_clauses = "(";
                        foreach (var item in items)
                        {
                            if (t2.Name == "String")//如果是字符串（泛型子类）                            
                                in_clauses += "'" + item + "',";
                            else
                                in_clauses += item + ",";
                        }
                        in_clauses = in_clauses.Substring(0, in_clauses.Length - 1);
                        in_clauses += ")";
                        return in_clauses;
                    }
                    else
                    {
                        return "";
                    }
            }
        }

        private string GetOperStr(ExpressionType exprType)
        {
            switch (exprType)
            {
                case ExpressionType.OrElse: return " OR ";
                case ExpressionType.Or: return " | ";
                case ExpressionType.AndAlso: return " AND ";
                case ExpressionType.And: return " & ";
                case ExpressionType.GreaterThan: return " > ";
                case ExpressionType.GreaterThanOrEqual: return " >= ";
                case ExpressionType.LessThan: return " < ";
                case ExpressionType.LessThanOrEqual: return " <= ";
                case ExpressionType.NotEqual: return " <> ";
                case ExpressionType.Add: return " + ";
                case ExpressionType.Subtract: return " - ";
                case ExpressionType.Multiply: return " * ";
                case ExpressionType.Divide: return " / ";
                case ExpressionType.Modulo: return " % ";
                case ExpressionType.Equal: return " = ";
            }
            return " ";
        }
    }

    /// <summary>
    /// sql条件操作符扩展类
    /// </summary>
    public static class SqlOperators
    {
        public static bool Sql_In<T>(this T t, List<T> list)
        {
            return true;
        }

        public static bool Sql_Like<T>(this T t, object v)
        {
            return true;
        }
    }

    /// <summary>
    /// 条件述语拼装生成器
    /// </summary>
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
