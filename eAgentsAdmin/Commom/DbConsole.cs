using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using PropertyOneAppWeb.Commom;
using Oracle.ManagedDataAccess.Client;
using Orcl9 = System.Data.OracleClient;
namespace PropertyOneAppWeb.Helper
{
    /// <summary>
    /// 压缩算法（LZ77实验）
    /// </summary>
    public class LZ77
    {
        /*
         * Example
         *                0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 
         * input stream : A  A  A  A  B  A  B  A  A  C  B  A  A  C  A  B  C  B  B  A  B  C
         *                65 65 65 65 66 65 66 65 65 67 66 65 65 67 65 66 67 66 66 65 66 67
         * output:
         * (0,0)A => code posi:0 
         * (1,1)  => code posi:1 A
         * (2,2)  => code posi:2 A A 
         * (0,0)B => code posi:4 
         * (2,2)  => code posi:5 A B
         * (5,2)  => code posi:7 A A
         * (0,0)C => code posi:9 
         * (4,4)  => code posi:10 B A A C
         * ...
         */

        byte[] buffer = null;
        int posi = 0;                           //首字节索引，总是从0开始(code position=0)                
        const int BACKWARD_WINDOW_LENGTH = 512; //后向滑动窗口大小（字典大小），目前是个常量。

        public LZ77() { }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="filePath">文件路径</param>   
        /// 这里是无损压缩
        public void Compress(string filePath)
        {            
            try
            {
                //打开为字节流
                FileInfo fi = new FileInfo(filePath);
                FileStream fs = fi.OpenRead();
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);

                // lookahead            
                int n = 0;  //记录循环次数(调试用)
                int foundPosi = -1;     //匹配的首字索引
                int codePosition = 0;   //当前编码位
                while (posi < buffer.Length)
                {
                    byte[] matched = new byte[0];   //匹配的内容数组(初始时为0个元素)
                    int idx = GetMaxMatch(posi, ref matched); //查找最大匹配值，返回匹配的首字索引。（核心算法，这里的posi是指数组元素的索引下标，matched是可变返回参数，表示找到的匹配数组内容）
                    codePosition = posi;    //记录当前编码位(调试用)
                    if (idx >= 0) //如果找到匹配位
                    {
                        if (posi >= BACKWARD_WINDOW_LENGTH)                     //如果编码位大于等于搜索窗口长度
                            foundPosi = posi - BACKWARD_WINDOW_LENGTH + idx;    //则返回匹配内容在整个字节流中的位置首字索引
                        else
                            foundPosi = idx;                        
                        posi = posi + matched.Length;                           //编码位向前移动n位（匹配长度）到下一个编码位
                    }
                    else
                    {
                        foundPosi = -1; //找不到匹配项，返回-1
                        posi++;         //编码位向前移动1位
                    }

                    /// -- start -- debug
                    string tstr = matched.Length > 0 ? " " : char.ConvertFromUtf32(buffer[codePosition]);
                    Commom.Log.WriteLog(string.Format("({0},{1}){2}{3}", foundPosi < 0 ? "0" : Aid.Null2Str(codePosition - foundPosi), Aid.Null2Str(matched.Length), tstr, Arr2Str(matched)));
                    n++;
                    /// --  end  --

                    /// -- start -- 输出文件
                    
                    /// --  end  --
                }

                /// -- start -- debug
                Commom.Log.WriteLog("Total:" + buffer.Length.ToString() + ";Zipped:" + n.ToString());
                /// --  end  --
            }
            catch (Exception err)
            {
                Commom.Log.WriteLog(err.Message + "\r\n" + err.StackTrace);
            }
        }

        /// <summary>
        /// 解压（未实现）
        /// </summary>
        public void Decompress()
        { }

        /// <summary>
        /// 从指定索引位逐渐递增，并直到获取最大匹配的首字索引，找不到则返回-1
        /// </summary>
        /// <param name="startPosi"></param>
        /// <param name="matched"></param>
        /// <returns></returns>
        private int GetMaxMatch(int startPosi,ref byte[] matched)
        {
            byte[] win = null;
            //获得编码窗口起始索引
            int windowStartPosi = -1;   //搜索窗口起始位
            if (startPosi < BACKWARD_WINDOW_LENGTH) //如果当前编码位小于搜索窗口大小
            {
                win = GetArr(0, startPosi); //获取指定偏移量和长度的搜索窗口数组（内容）
            }
            else
            {
                windowStartPosi = startPosi - BACKWARD_WINDOW_LENGTH;   //计算搜索窗口起始位置
                win = GetArr(windowStartPosi, BACKWARD_WINDOW_LENGTH);  //获取指定偏移量和长度的搜索窗口数组（内容）
            }

            int lastFoundIdx = -1;  //上次循环找到的匹配项首字索引
            int foundIdx = -1;      //本次循环找到的匹配项首字索引
            int len = 1;            //循环增加的数组长度，初始为1
            do
            {
                if (win == null)        //如果搜索窗口数组不存在则跳出循环（首字编码时会出现）
                    break;
                
                if (len > win.Length)   //如果编码长度超过了搜索窗口长度则跳出循环
                    break;

                byte[] attempt = GetArr(startPosi, len);    //获取待编码区指定偏移量及长度的数组
                foundIdx = GetIndexOf(win, attempt);        //尝试找到匹配位
                if (foundIdx >= 0)                          //如果找到就返回匹配数组的首位索引
                {
                    lastFoundIdx = foundIdx;                //记录本次找到的位置，确保下次循环没有找到时可以返回
                    matched = attempt;                      //记录本次找到的匹配数组(内容)
                }                
                len++;                                      //数组长度加1，继续查找。
            } while (foundIdx >= 0);                        //直到找不到匹配时跳出循环
            return lastFoundIdx;                            //返回上次匹配的数组的首位索引（没有任何匹配时返回-1）
        }

        private byte[] GetArr(int offset, int length)
        {
            if (length > 0 && offset + length <= buffer.Length)
            {
                byte[] arr = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = buffer[offset + i];
                }
                return arr;
            }
            else
                return null;
        }

        /// <summary>
        /// 从数组1中找到匹配数组2中所有连续元素的首字索引，没有任何匹配时返回-1
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        private int GetIndexOf(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null)
                return -1;

            int foundIdx = -1;
            for (int i = 0; i < a1.Length; i++)
            {
                // 当数组a1的剩余元素小于数组a2时跳出循环
                if (a1.Length - i < a2.Length)
                    break;              

                for (int j = 0; j < a2.Length; j++)
                {
                    if (a2[j] == a1[i + j])
                        foundIdx = i;
                    else
                    {
                        foundIdx = -1;
                        break;
                    }
                }

                /*
                 * 这里可以修改成“找到最后一个完全匹配的首字索引”再返回，目前只考虑第一个。
                 */
                if (foundIdx >= 0) //或者已经到了结尾 a1.
                    return foundIdx;
            }
            return foundIdx;
        }

        /// <summary>
        /// 调试函数，用来查看匹配的字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private string Arr2Str(byte[] arr)
        {
            if (arr == null)            
                return "";

            string arrOut = "";
            for (int i = 0; i < arr.Length; i++)
            {
                arrOut += char.ConvertFromUtf32(arr[i]) + " ";
            }
            return arrOut;
        }
    }

    /// <summary>
    /// 数据库厂商枚举
    /// </summary>
    public enum DbVendor
    {
        Oracle,
        MySQL,
        Firebird,
        MSSQL
    }

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
    /// Database access with transaction,only support 'Oracle'
    /// </summary>
    /// 待改进
    public class DbTx
    {
        public static int ConnectionCount { get { return connCnt; } }
        private static int connCnt = 0;

        #region declaration
        public int Err_Code { get; set; }
        public string Err_Msg { get; set; }

        IDbConnection connection = null;
        IDbTransaction tranxtion = null;
        DbVendor m_vendor;

        public string Version
        {
            get
            {
                switch (this.m_vendor)
                {
                    case DbVendor.Firebird:
                        //FbConnection fbconn = this.connection as FbConnection;
                        //return fbconn.ServerVersion;
                    case DbVendor.MySQL:
                        //MySqlConnection myconn = this.connection as MySqlConnection;
                        //return myconn.ServerVersion;

                    case DbVendor.MSSQL:
                        //SqlConnection sqlconn = this.connection as SqlConnection;
                        //return sqlconn.ServerVersion;

                    case DbVendor.Oracle:
                        if (this.connection.GetType().FullName == "System.Data.OracleClient.OracleConnection")
                        {
                            System.Data.OracleClient.OracleConnection orcl9Conn = this.connection as Orcl9.OracleConnection;
                            return orcl9Conn.ServerVersion;
                        }
                        else
                        {
                            OracleConnection orclConn = this.connection as OracleConnection;
                            return orclConn.ServerVersion;
                        }                       
                    default:
                        return "";
                }
            }
        }
        /// <summary>
        /// The last integer value of auto increment field after inserting an record to the table.
        /// </summary>
        public int LastIdentity
        {
            get
            {
                int ai_id = 0;
                switch (this.m_vendor)
                {
                    case DbVendor.Oracle:
                        break;

                    case DbVendor.MSSQL:
                    case DbVendor.MySQL:
                        ai_id = Aid.DbNull2Int(this.ExecuteScalar("select @@IDENTITY"));
                        break;

                    case DbVendor.Firebird:
                        break;

                    default:
                        break;
                }
                return ai_id;
            }
        }

        public DbVendor Vendor { get { return this.m_vendor; } }

        /// <summary>
        /// database transaction constructor
        /// </summary>
        /// <param name="vendor">database vendor</param>
        /// <param name="connStr">connection string </param>
        /// <param name="openTx">open transaction,true=yes,false=no.</param>
        /// <param name="obsolete">use obsolete oracle lib default=false </param>
        public DbTx(DbVendor vendor, string connStr, bool openTx, bool obsolete = false)
        {
            try
            {
                this.m_vendor = vendor;
                switch (vendor)
                {
                    case DbVendor.Oracle:
                        if (obsolete)
                            this.connection = new Orcl9.OracleConnection(connStr);
                        else
                            this.connection = new OracleConnection(connStr);                        
                        break;

                    case DbVendor.MySQL:
                        //this.connection = new MySqlConnection(connStr);
                        break;

                    case DbVendor.Firebird:
                        //this.connection = new FbConnection(connStr);
                        break;

                    case DbVendor.MSSQL:
                        //this.connection = new SqlConnection(connStr);
                        break;

                    default:
                        throw new Exception("Database vendor not found!");
                }
                this.connection.Open();
                connCnt++;
                this.tranxtion = openTx ? this.connection.BeginTransaction() : null;               
            }
            catch (Exception err)
            {
                this.Err_Code = -1;
                this.Err_Msg = err.Message;
            }
        }        

        #endregion

        #region instance methods
        public int ExecuteNonQuery(string sql, params QueryParameter[] paras)
        {
            int cnt = 0;
            IDbCommand command = this.GetCommand(this.m_vendor, sql);
            try
            {
                if (paras != null)
                {
                    foreach (QueryParameter item in paras)
                    {
                        IDataParameter ipara = null;
                        switch (this.m_vendor)
                        {
                            case DbVendor.Oracle:                                
                                if (item.Value.GetType() == typeof(byte[])) //如果是字节数组，就用blob
                                    ipara = this.GetParameterBlob(item.Name, item.Value);                                
                                else
                                    ipara = this.GetParameter(item.Name, item.Value);
                                break;
                            case DbVendor.MySQL:
                                break;
                            case DbVendor.Firebird:
                                break;
                            case DbVendor.MSSQL:
                                break;
                            default:
                                break;
                        }
                        command.Parameters.Add(ipara);
                    }
                }
                cnt = command.ExecuteNonQuery();                
                if (command != null)
                    command.Dispose();
            }
            catch (Exception err)
            {
                throw err;
            }
            return cnt;
        }

        public object ExecuteScalar(string sql)
        {
            return this.GetCommand(this.m_vendor, sql).ExecuteScalar();
        }

        /// <summary>
        /// Execute sql statement (unsafe)
        /// </summary>
        /// <param name="sql">sql statement</param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(string sql)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = this.GetCommand(this.Vendor, sql);
                IDataReader ir = cmd.ExecuteReader();
                cmd.Dispose();
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
        /// Execute sql statement with parameters objects (safe)
        /// </summary>
        /// <param name="sql">sql statement</param>
        /// <param name="parameters">parameters</param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(string sql, QueryParameter[] parameters, bool sp = false)
        {
            IDbCommand command = GetCommand(this.m_vendor, sql);
            if (sp)            
                command.CommandType = CommandType.StoredProcedure;
            
            for (int i = 0; i < parameters.Length; i++)
            {
                switch (this.Vendor)
                {
                    case DbVendor.Oracle:
                        if (parameters[i].Value.GetType() == typeof(byte[])) //如果是字节数组，就用blob                        
                            command.Parameters.Add(this.GetParameterBlob(parameters[i].Name, parameters[i].Value));
                        else
                            command.Parameters.Add(this.GetParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.MySQL:
                        //command.Parameters.Add(new MySqlParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.Firebird:
                        //command.Parameters.Add(new FbParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.MSSQL:
                        //command.Parameters.Add(new SqlParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    default:
                        throw new Exception("Database vendor is not found on creating parameter object!");
                }
            }
            IDataReader ir = command.ExecuteReader();
            command.Dispose();
            return ir;
        }

        /// <summary>
        /// 执行“oracle”返回结果集的存储过程（已过时，只为兼容早期的oracle版本，应该避免使用）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        public Orcl9.OracleDataReader ExecuteReader(string sql, params System.Data.OracleClient.OracleParameter[] paras)
        {
            try
            {
                Orcl9.OracleCommand command = null;                
                command = new Orcl9.OracleCommand(sql, this.connection as Orcl9.OracleConnection, this.tranxtion as Orcl9.OracleTransaction);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(paras);
                return command.ExecuteReader();
            }
            catch (Exception)
            {
                throw;
            }                                 
        }

        public DataTable ExecuteDataTable(string sql)
        {
            DataSet ds = new DataSet();
            this.GetDataAdapter(this.m_vendor, sql).Fill(ds);
            if (ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return new DataTable();
        }

        public DataTable ExecuteDataTable(string sql, QueryParameter[] parameters)
        {
            DataSet ds = new DataSet();
            IDbCommand command = this.BuildCommand(sql, parameters);
            this.GetDataAdapter(this.Vendor, command).Fill(ds);
            if (ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return new DataTable();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTrans()
        {
            try
            {
                if (this.tranxtion != null)
                    this.tranxtion.Commit();
            }
            catch
            {
                this.tranxtion.Rollback();
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
                this.tranxtion.Rollback();
            }
            catch
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    if (this.tranxtion != null)
                        this.tranxtion.Rollback();
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

        #endregion

        #region private methods

        private IDbCommand GetCommand(DbVendor vendor, string sql)
        {
            switch (vendor)
            {
                case DbVendor.Oracle:
                    if (this.connection.GetType().FullName == "System.Data.OracleClient.OracleConnection")
                        return new Orcl9.OracleCommand(sql, this.connection as Orcl9.OracleConnection, this.tranxtion as Orcl9.OracleTransaction);
                    else
                        return new OracleCommand(sql, this.connection as OracleConnection);

                case DbVendor.MySQL:
                    //return new MySqlCommand(sql, this.connection as MySqlConnection, this.tranxtion as MySqlTransaction);

                case DbVendor.Firebird:
                    //return new FbCommand(sql, this.connection as FbConnection, this.tranxtion as FbTransaction);

                case DbVendor.MSSQL:
                    //return new SqlCommand(sql, this.connection as SqlConnection);

                default:
                    throw new Exception("Database vendor unknown!");
            }
        }

        private IDataParameter GetParameterBlob(string name, object value)
        {
            IDataParameter idp = null;
            switch (this.Vendor)
            {
                case DbVendor.Oracle:
                    if (this.connection.GetType().FullName == "System.Data.OracleClient.OracleConnection")
                    {
                        Orcl9.OracleParameter op9 = new Orcl9.OracleParameter(name, Orcl9.OracleType.Blob);
                        op9.Value = value;
                        idp = op9;
                    }
                    else
                    {
                        OracleParameter op = new OracleParameter(name, OracleDbType.Blob);
                        op.Value = value;
                        idp = op;
                    }
                    break;
                case DbVendor.MySQL:
                    break;
                case DbVendor.Firebird:
                    break;
                case DbVendor.MSSQL:
                    break;
                default:
                    break;
            }
            return idp;
        }

        private IDataParameter GetParameter(string name,object value)
        {
            IDataParameter idp = null;
            switch (this.Vendor)
            {
                case DbVendor.Oracle:
                    if (this.connection.GetType().FullName == "System.Data.OracleClient.OracleConnection")
                    {
                        idp = new Orcl9.OracleParameter(name, value);                         
                    }
                    else
                        idp = new OracleParameter(name, value);
                    break;
                case DbVendor.MySQL:
                    break;
                case DbVendor.Firebird:
                    break;
                case DbVendor.MSSQL:
                    break;
                default:
                    break;
            }
            return idp;
        }

        private IDbCommand BuildCommand(string sql, QueryParameter[] parameters)
        {
            IDbCommand command = null;
            switch (this.Vendor)
            {
                case DbVendor.Oracle:
                    //command = new OracleCommand(sql, this.connection as OracleConnection);
                    command = this.GetCommand(this.Vendor, sql);
                    break;

                case DbVendor.MySQL:
                    //command = new MySqlCommand(sql, this.connection as MySqlConnection);

                    break;
                case DbVendor.Firebird:
                    //command = new FbCommand(sql, this.connection as FbConnection);

                    break;
                case DbVendor.MSSQL:
                    //command = new SqlCommand(sql, this.connection as SqlConnection);
                    break;

                default:
                    throw new Exception("Database vendor unknown!");
            }
            BindParameters(command, parameters);
            return command;
        }

        private void BindParameters(IDbCommand command, QueryParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                switch (this.Vendor)
                {
                    case DbVendor.Oracle:
                        //command.Parameters.Add(new OracleParameter(parameters[i].Name, parameters[i].Value));
                        command.Parameters.Add(this.GetParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.MySQL:
                        //command.Parameters.Add(new MySqlParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.Firebird:
                        //command.Parameters.Add(new FbParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    case DbVendor.MSSQL:
                        //command.Parameters.Add(new SqlParameter(parameters[i].Name, parameters[i].Value));
                        break;
                    default:
                        throw new Exception("Database vendor not found on creating parameter object!");
                }
            }
        }

        private IDataAdapter GetDataAdapter(DbVendor vendor, string sql)
        {
            switch (vendor)
            {
                case DbVendor.Oracle:
                    IDbCommand cmd = GetCommand(vendor, sql);
                    if (cmd.GetType().FullName == "System.Data.OracleClient.OracleCommand")
                        return new Orcl9.OracleDataAdapter(cmd as Orcl9.OracleCommand);
                    else
                        return new OracleDataAdapter(cmd as OracleCommand);

                case DbVendor.MySQL:
                    //MySqlCommand mysqlcmd = GetCommand(vendor, sql) as MySqlCommand;
                    //return new MySqlDataAdapter(mysqlcmd);

                case DbVendor.Firebird:
                    //FbCommand fbcmd = GetCommand(vendor, sql) as FbCommand;
                    //return new FbDataAdapter(fbcmd);

                case DbVendor.MSSQL:
                    //SqlCommand sqlcmd = GetCommand(vendor, sql) as SqlCommand;
                    //return new SqlDataAdapter(sqlcmd);

                default:
                    throw new Exception("Database vendor unknown!");
            }
        }

        private IDataAdapter GetDataAdapter(DbVendor vendor, IDbCommand command)
        {
            switch (vendor)
            {
                case DbVendor.Oracle:
                    if (this.connection.GetType().FullName == "System.Data.OracleClient.OracleConnection")                    
                        return new Orcl9.OracleDataAdapter(command as Orcl9.OracleCommand);                    
                    else
                        return new OracleDataAdapter(command as OracleCommand);
                case DbVendor.MySQL:
                    //return new MySqlDataAdapter(command as MySqlCommand);
                case DbVendor.Firebird:
                    //return new FbDataAdapter(command as FbCommand);
                case DbVendor.MSSQL:
                    //return new SqlDataAdapter(command as SqlCommand);
                default:
                    throw new Exception("Database vendor unknown!");
            }
        }

        #endregion
    }    

    /// <summary>
    /// Sql relation operator
    /// </summary>    
    public enum SqlOperator // sqloptr
    {

        /// <summary>
        /// not in
        /// </summary>
        NI,

        /// <summary>
        /// in 
        /// </summary>
        IN,

        /// <summary>
        /// like '%%'
        /// </summary>
        LIKE,

        /// <summary>
        /// equal to 
        /// </summary>
        EQ,

        /// <summary>
        /// greater than 
        /// </summary>
        GT,

        /// <summary>
        /// less than 
        /// </summary>
        LT,

        /// <summary>
        /// greater than and equal 
        /// </summary>
        GTE,

        /// <summary>
        /// less than & equal 
        /// </summary>
        LTE,

        /// <summary>
        /// not equal
        /// </summary>
        NE,

        /// <summary>
        /// between 
        /// </summary>
        BT
    }  

    /// <summary>
    /// sql condition clause evaluation
    /// </summary>
    public class Evaluation
    {
        SqlOperator m_op;
        object[] values;
        public string Relation_Operator
        {
            get
            {
                switch (this.m_op)
                {
                    case SqlOperator.EQ:
                        return " = ";
                    case SqlOperator.GT:
                        return " > ";
                    case SqlOperator.LT:
                        return " < ";
                    case SqlOperator.GTE:
                        return " >= ";
                    case SqlOperator.LTE:
                        return " <= ";
                    case SqlOperator.NE:
                        return " <> ";
                    case SqlOperator.BT:
                        return " between ";
                    case SqlOperator.LIKE:
                        return " like ";
                    case SqlOperator.IN:
                        return " in ";                       
                    case SqlOperator.NI:
                        return " not in ";
                    default:
                        throw new Exception("Missing relation operator!");
                }
            }
        }
        public object[] Vals { get { return values; } }
        public Evaluation(SqlOperator op, params object[] vals)
        {
            this.m_op = op;
            this.values = vals;
        }
    }

    /// <summary>
    /// Query parameter 
    /// 1. oracle prefix is ":"
    /// 2. sqlserver and mysql prefix is "@"
    /// </summary>
    public class QueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public QueryParameter(string name, object val)
        {
            this.Name = name;
            this.Value = val;
        }
    }

    public class OrderByClause
    {
        public string OrderKey { get; set; }
        public Order Direction { get; set; }

        public OrderByClause(string key, Order o= Order.ASC)
        {
            this.OrderKey = key;
            this.Direction = o;
        }
    }

    public enum Order
    {
        ASC,
        DESC
    }
        
    /// <summary>
    /// DML manipulate interface
    /// </summary>
    interface IDML
    {
        int Create<T>(T ety) where T : class;
        List<T> Retrieve<T>() where T : class;
        System.Data.DataTable RetrieveDatatable<T>() where T : class;
        int Update<T>(T ety) where T : class;
        int Delete<T>(T ety) where T : class;
    }

    /// <summary>
    /// Lightweight Database Manipulator;
    /// </summary>
    public class DbConsole : IDML
    {
        #region declaration

        public int LastId { get { return m_last_insert_id; } } int m_last_insert_id = 0; // LastIdentityId

        /// <summary>
        /// Sql "where" clauses list
        /// </summary>
        public Dictionary<string, Evaluation> WhereClauses { get; set; }
        public List<OrderByClause> OrderClauses { get; set; }

        /// <summary>
        /// Database Manipulate Language Type 
        /// </summary>
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
        List<QueryParameter> qps = new List<QueryParameter>();  // query parameters
        string sqlwhere = "";

        DbTx m_tx = null;

        #endregion

        #region constructor

        public DbConsole(DbTx tx)
        {
            if (tx.Err_Code < 0)
                throw new Exception(tx.Err_Msg);

            this.m_tx = tx;
            this.WhereClauses = new Dictionary<string, Evaluation>();
            this.OrderClauses = new List<OrderByClause>();
        }

        #endregion

        #region instance methods

        /// <summary>
        /// Create entity,insert into 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        public int Create<T>(T ety) where T : class
        {
            try
            {
                string sql = "";
                int cnt;

                sql = GenInsertSqlSafer<T>(ety);
                cnt = this.m_tx.ExecuteNonQuery(sql, qps.ToArray());

                string sqlStr = sql;
                for (int i = 0; i < qps.Count; i++)
                {
                    sqlStr = sqlStr.Replace(qps[i].Name, "'" + qps[i].Value.ToString() + "'");
                }
                this.GetLastInsertId();
                return cnt;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// 使用表达式树作为条件查询（推荐使用）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// 不支持.NET 2.0
        public List<T> Retrieve<T>(Expression<Func<T, bool>> expr, string orderBy = "") where T : class
        {
            System.Data.IDataReader reader = null;
            try
            {
                T ety = Activator.CreateInstance<T>();
                string sql = " select * from " + typeof(T).GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                ExprVistor ev = new ExprVistor();
                string clauses = ev.Parse(expr);
                sql += " where " + (clauses.Length > 0 ? clauses : "1=1");
                if (orderBy!="")                
                    sql += orderBy;
                
                reader = this.m_tx.ExecuteReader(sql);
                List<T> etys = this.FillBy<T>(reader);
                this.WhereClauses.Clear();
                this.OrderClauses.Clear();
                return etys;
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

        public DataTable RetrieveDatatable<T>(Expression<Func<T, bool>> expr, string orderBy = "") where T : class
        {
            DataTable dt = new DataTable();

            try
            {
                T ety = Activator.CreateInstance<T>();
                string sql = " select * from " + typeof(T).GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                ExprVistor ev = new ExprVistor();
                string clauses = ev.Parse(expr);
                sql += " where " + (clauses.Length > 0 ? clauses : "1=1");
                if (orderBy != "")
                    sql += orderBy;

                dt = this.m_tx.ExecuteDataTable(sql);

                this.WhereClauses.Clear();
                this.OrderClauses.Clear();
                return dt;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        
        /// <summary>
        /// Retrieve to List and clear clauses (不推荐使用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>        
        public List<T> Retrieve<T>() where T : class
        {
            System.Data.IDataReader reader = null;
            try
            {
                T ety = Activator.CreateInstance<T>();
                string sql = " select * from " + typeof(T).GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                this.GenSelectSqlSafer();

                string sqlOrderBy = "";
                if (this.OrderClauses.Count>0)
                {
                    sqlOrderBy += " Order by ";
                    for (int j = 0; j < this.OrderClauses.Count; j++)                    
                       sqlOrderBy += this.OrderClauses[j].OrderKey + " " + this.OrderClauses[j].Direction.ToString() + ",";                    
                    sqlOrderBy = sqlOrderBy.Substring(0, sqlOrderBy.Length - 1);
                }
                reader = this.m_tx.ExecuteReader(sql + sqlwhere + sqlOrderBy, qps.ToArray());
                this.WhereClauses.Clear();
                this.OrderClauses.Clear();
                List<T> etys = this.FillBy<T>(reader);
                return etys;
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
        /// 执行返回结果集的原生sql查询，并转换为泛型实体列表。(不安全，待改进，最好支持参数化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<T> Retrieve<T>(string sql) where T : class
        {
            System.Data.IDataReader reader = null;
            try
            {
                T ety = Activator.CreateInstance<T>();
                reader = this.m_tx.ExecuteReader(sql);
                List<T> etys = this.FillBy<T>(reader);
                this.WhereClauses.Clear();
                return etys;
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
        /// 未实现的方法（安全）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> Retrieve<T>(string sql, params IDataParameter[] parameters) where T : class
        {
            throw new NotImplementedException();
        }

       
        /// <summary>
        /// 执行返回结果集的oracle存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public List<T> Retrieve9iSp<T>(string sql, params System.Data.OracleClient.OracleParameter[] paras) where T:class
        {
            System.Data.IDataReader reader = null;
            try
            {
                T ety = Activator.CreateInstance<T>();                
                reader = this.m_tx.ExecuteReader(sql, paras);
                List<T> etys = this.FillBy<T>(reader);
                this.WhereClauses.Clear();
                return etys;
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
        /// Retrieve to datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public System.Data.DataTable RetrieveDatatable<T>() where T : class
        {
            try
            {
                T ety = Activator.CreateInstance<T>();
                string sql = " select * from " + typeof(T).GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
                GenSelectSql();
                return this.m_tx.ExecuteDataTable(sql + sqlwhere, qps.ToArray());
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public System.Data.DataTable RetrieveDataTable(string sql)
        {
            try
            {
               return this.m_tx.ExecuteDataTable(sql + sqlwhere, qps.ToArray());
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public System.Data.DataTable Query2dt(string sql)
        {
            try
            {
                return this.m_tx.ExecuteDataTable(sql);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// Update Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>        
        public int Update<T>(T ety) where T : class
        {
            string sql = this.GenUpdateSqlSafer<T>(ety);
            int cnt = this.m_tx.ExecuteNonQuery(sql, this.qps.ToArray());
            return cnt;
        }

        /// <summary>
        /// Delete entiry
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
                            sql += " and " + pi.Name + "=" + Aid.DbNull2Str(pi.GetValue(ety, null)) ;
                            break;
                        default:
                            sql += " and " + pi.Name + "='" + Aid.DbNull2Str(pi.GetValue(ety, null)) + "'";
                            break;
                    }
                }
                //sql = sql.Substring(0, sql.Length - 1);
                int cnt = this.m_tx.ExecuteNonQuery(sql);
                return cnt;
            }
            catch (Exception err)
            {
                throw err;
            }            
        }

        public int Delete<T>(List<T> etys) where T : class
        {
            try
            {
                int cnt = 0;
                for (int i = 0; i < etys.Count; i++)
                {
                    cnt += Delete(etys[i]);
                }
                return cnt;
            }
            catch (Exception)
            {                
                throw;
            }            
        }       

        #endregion

        #region private methods
        /// <summary>
        /// get last insert id which belong to mysql or mssql
        /// </summary>
        private void GetLastInsertId()
        {
            string sql = "";
            if (this.m_tx.Vendor == DbVendor.MySQL)
                sql = "SELECT LAST_INSERT_ID()";
            else if (this.m_tx.Vendor == DbVendor.Oracle)
            { }
            else if (this.m_tx.Vendor == DbVendor.MSSQL)
                sql = "select @@identity";

            if (sql.Length > 0)
            {
                this.m_last_insert_id = Convert.ToInt32(this.m_tx.ExecuteScalar(sql));
            }
        }

        /// <summary>
        /// convert prop into string value as sql statement parttern.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="val"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        /// 仍有改进空间
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
                    rtnVal = Aid.Null2Str(Convert.ToChar(val));
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
                        qps.Add(new QueryParameter(qpName, val));
                        rtnVal = qpName;
                    }
                    break;
            }
            return rtnVal;
        }

        /// <summary>
        /// Generate safe insert sql statement by properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        private string GenInsertSqlSafer<T>(T ety)
        {
            this.GetSchema(ety, DmlType.Insert);
            qps.Clear(); //清除已有参数
            
            /// 准备语句
            FieldInfo fi = ety.GetType().GetField(FIELD_TABLE_NAME);
            if (fi == null)
                throw new Exception("table name undefined!");

            string sql = "insert into " + fi.GetValue(ety).ToString();
            string cols = "", vals = "";
            foreach (PropertyInfo pi in fnvs)
            {
                /// 处理DbFieldAttribute
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
        /// Generate safe update sql statement by properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <returns></returns>
        private string GenUpdateSqlSafer<T>(T ety)
        {
            this.GetSchema(ety, DmlType.Update);
            if (keys.Count == 0)
                throw new Exception("primary key not exists,can not execute update statement!");

            qps.Clear();
            string sql = "update " + ety.GetType().GetField(FIELD_TABLE_NAME).GetValue(ety).ToString();
            sql += " set ";

            foreach (PropertyInfo pif in this.fnvs) //pif->propertyinfo field
            {
                if (pif.GetValue(ety, null) == null) //如果为空则字段置为null                
                    sql += pif.Name + " = null,";
                else
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
        /// Generate select statement and build parameters
        /// </summary>
        private void GenSelectSqlSafer()
        {
            this.sqlwhere = " where 1=1 ";
            qps.Clear(); //清除已有参数
            string PARAM_PREFIX = this.GetParamPrefix();

            if (this.WhereClauses == null)
                return;

            int serial = 0;
            foreach (var item in this.WhereClauses)
            {
                string paraName = "";
                string relation_operator = item.Value.Relation_Operator;


                if (relation_operator.Contains("not in"))
                {
                    if (item.Value.Vals.Length == 0)
                        throw new Exception(" parameter counts of clause 'not in' can not be zero! ");

                    sqlwhere += " and " + item.Key + relation_operator + " ("; // and x in (
                    for (int i = 0; i < item.Value.Vals.Length; i++)
                    {
                        paraName = PARAM_PREFIX + serial.ToString();
                        sqlwhere += paraName + ",";
                        qps.Add(new QueryParameter(paraName, item.Value.Vals[i]));
                        serial++;
                    }
                    sqlwhere = sqlwhere.Substring(0, sqlwhere.Length - 1);
                    sqlwhere += ")";
                }
                else if (relation_operator.Contains("in"))
                {
                    if (item.Value.Vals.Length == 0)
                        throw new Exception(" parameter counts of clause 'in' can not be zero! ");

                    sqlwhere += " and " + item.Key + relation_operator + " ("; // and x in (
                    for (int i = 0; i < item.Value.Vals.Length; i++)
                    {
                        paraName = PARAM_PREFIX + serial.ToString();
                        sqlwhere += paraName + ",";
                        qps.Add(new QueryParameter(paraName, item.Value.Vals[i]));
                        serial++;
                    }
                    sqlwhere = sqlwhere.Substring(0, sqlwhere.Length - 1);
                    sqlwhere += ")";
                }                
                else if (relation_operator.Contains("between"))
                {
                    if (item.Value.Vals.Length != 2)
                        throw new Exception(" clause 'between' must contain 2 parameters only! ");

                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName + " and ";
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[0]));
                    serial++;

                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += paraName;
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[1]));
                    serial++;
                }
                else if (relation_operator.Contains("like"))
                {
                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName;
                    qps.Add(new QueryParameter(paraName, "%" + item.Value.Vals[0] + "%"));
                    serial++;
                }
                else if (item.Value.Vals.Length == 1)
                {
                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName;
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[0]));
                    serial++;
                }
            }
        }
                   
        /// <summary>
        /// Generate select statement and build parameters
        /// </summary>
        private void GenSelectSql()
        {
            sqlwhere = " where 1=1 ";
            qps.Clear(); //清除已有参数
            string PARAM_PREFIX = this.GetParamPrefix();

            if (this.WhereClauses == null)
            {
                return;
            }
            int serial = 0;
            foreach (var item in this.WhereClauses)
            {
                string paraName = "";
                string relation_operator = item.Value.Relation_Operator;

                if (relation_operator.Contains("in"))
                {
                    
                    sqlwhere += " and " + item.Key + relation_operator + " ("; // and x in (
         
                    for (int i = 0; i < item.Value.Vals.Length; i++)
                    {
                        sqlwhere += paraName = PARAM_PREFIX + serial.ToString() + ","; // and x in (:p1,
                        qps.Add(new QueryParameter(paraName, item.Value.Vals[i]));
                        serial++;
                    }
                    sqlwhere = sqlwhere.Substring(0, sqlwhere.Length - 1);
                    sqlwhere += ")";                    
                }
                else if (relation_operator.Contains("between"))
                {
                    if (item.Value.Vals.Length != 2)
                        throw new Exception(" clause 'between' must contain 2 parameters only! ");

                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName + " and ";
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[0]));
                    serial++;

                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += paraName;
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[1]));
                    serial++;
                }
                else if (relation_operator.Contains("like"))
                {
                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName;
                    qps.Add(new QueryParameter(paraName, "%" + item.Value.Vals[0] + "%"));
                    serial++;
                }
                else if (item.Value.Vals.Length == 1)
                {
                    paraName = PARAM_PREFIX + serial.ToString();
                    sqlwhere += " and " + item.Key + relation_operator + paraName;
                    qps.Add(new QueryParameter(paraName, item.Value.Vals[0]));
                    serial++;
                }
            }
        }

        /// <summary>
        /// Get parameter prefix by database vendor
        /// </summary>
        /// <returns></returns>
        private string GetParamPrefix()
        {
            switch (this.m_tx.Vendor)
            {
                case DbVendor.Oracle:
                    return ":p";

                case DbVendor.Firebird:
                case DbVendor.MySQL:
                case DbVendor.MSSQL:
                    return "@p";

                default:
                    throw new Exception("Database vendor not found for parameter prefix!");
            }
        }

        /// <summary>
        /// Fill entity by reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>entity list</returns>
        private List<T> FillBy<T>(System.Data.IDataReader reader) where T : class
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
                            case TypeCode.DateTime:
                                /// hint:以下需要模拟测试数据库表字段不允许为空情况
                                if (!reader[pi.Name].Equals(DBNull.Value))
                                    pi.SetValue(ety1, reader[pi.Name], null);
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
                                    pi.SetValue(ety1,reader[pi.Name],null);
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

        /// <summary>
        /// Remove schema info
        /// </summary>
        private void Cleanup()
        {
            this.fnvs.Clear();
            this.keys.Clear();
        }

        /// <summary>
        /// Get ORM relection with properties for field,values and primary keys. Usually used to insert,update,delete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ety"></param>
        /// <param name="tag"></param>
        private void GetSchema<T>(T ety, DmlType tag)
        {
            this.Cleanup();

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

        #endregion        

        //获取城市（不应该放在这里，有机会移出此类）
        public static string GetDbCity()
        {
            string t = Aid.Null2Str(System.Configuration.ConfigurationManager.AppSettings["docTitle"]);
            return t != "" ? t : "eAgents Admin";            
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
            Dmy=6
        }

        #endregion

        #region 静态属性

        public static string Status4EFM_NTC { get; set; }
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

        public static string Stream2Str(Stream s)
        {
            StreamReader sr = new StreamReader(s);
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
        #endregion
    }

    #region IQuryable Expression

    public class DbSet<T> :IEnumerable<T>, IQueryable where T : class
    {

        List<T> etysNew = new List<T>();
        List<T> etysDel = new List<T>();
        List<T> etysEdt = new List<T>();

        public void Add(T ety)
        {
            HashSet<T> hst = new HashSet<T>();

            this.etysNew.Add(ety);
        }

        public Type ElementType
        {
            get { throw new NotImplementedException(); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            
            get { throw new NotImplementedException(); }
        }

        public IQueryProvider Provider
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DbConsoleExam
    {
        public static void Demo()
        {
            Task task = new Task(() =>
            {
                Thread.Sleep(100);
                Commom.Log.WriteLog(@"task1 thread id:" + Thread.CurrentThread.ManagedThreadId.ToString());
            });
            task.Start();

            Commom.Log.WriteLog("main thread.");
        }

        public DbConsoleExam() 
        {         
        }
        
        public DbConsoleExam(string connStr) { }

        public void SaveChange()
        {
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {

            }

            DbSet<Model.Sys_Login_Account> loginaccount = new DbSet<Model.Sys_Login_Account>();
            
            string loginname="";
            string pwd="md5pwd";
            loginaccount.Where(x => x.LoginName == loginname && x.Password == pwd).ToList();
            //ExpressionVisitor 
        }
    }

    #endregion     

    /// <summary>
    /// 表达式访问器类，用于生成sql条件子句
    /// </summary>
    /// 待改进
    public class ExprVistor : System.Linq.Expressions.ExpressionVisitor
    {
        DbVendor vdr;
        public string Clause
        {
            get
            {
                string outClause = string.Concat(this.clause.ToArray());
                return outClause;
            }
        }
        readonly Stack<string> clause = new Stack<string>();

        /// <summary>
        /// 分析表达式并返回子句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public string Parse<T>(Expression<Func<T, bool>> expr, DbVendor vendor = DbVendor.Oracle) where T : class
        {
            vdr = vendor;
            if (expr.Body.ToString() == "True") //如果是True，说明表达式来自predicate子句的初始值。        
                return "1=1";
            else if (expr == null)
            {
                return "";
            }
            else
            {
                string clauses = Involve(expr.Body);
                return clauses;
            }
        }

        private string Involve(Expression expr, bool eval = false)
        {
            string name = expr.GetType().Name;
            switch (name)
            {
                case "LogicalBinaryExpression":
                case "MethodBinaryExpression":
                    return InvolveBinary(expr as BinaryExpression);
               
                case "PropertyExpression":
                case "FieldExpression":
                    return InvolveMember(expr as MemberExpression, eval);

                case "InstanceMethodCallExpressionN":
                case "MethodCallExpressionN":
                    return InvolveMethodCall(expr as MethodCallExpression);

                case "ConstantExpression":
                    return InvolveConstant(expr as ConstantExpression);

                case "InvocationExpression":
                    return InvolveInvocation(expr as InvocationExpression);
                
                case "FullConditionalExpression":   //三目运算表达式                                        
                    return InvolveConditional(expr as ConditionalExpression);
                    
                default:
                    break;
            }
            return "";
        }

        private string InvolveConditional(ConditionalExpression node)
        {
            string typeName = node.Type.Name;
            if (typeName == typeof(int).Name)
            {
                return this.MethodCall2Value(node).ToString();
            }
            else
            {
                return "'" + this.MethodCall2Value(node).ToString() + "'";
            }            
        }

        private string InvolveInvocation(InvocationExpression node)
        {
            LambdaExpression lambda = node.Expression as LambdaExpression;
            if (lambda != null)
            {
                return Involve(lambda.Body);
            }
            else
                return "";
        }

        private string InvolveConstant(ConstantExpression node)
        {
            return this.Eval(node.Value);
        }

        private string InvolveMethodCall(MethodCallExpression node)
        {
            string txt = "";
            if (node.Method.Name == "Sql_In")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                txt += " IN ";
                txt += this.GetMethodCallInVals(node.Arguments[1] as MemberExpression);
            }
            else if (node.Method.Name == "Sql_NotIn")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                txt += " NOT IN ";
                txt += this.GetMethodCallInVals(node.Arguments[1] as MemberExpression);
            }
            else if (node.Method.Name == "Sql_Like")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                object result = MethodCall2Value(node.Arguments[1]);
                txt += " LIKE '%" + result.ToString() + "%'";
            }
            else if (node.Method.Name == "Sql_IsNull")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                txt += " is null ";
            }
            else if (node.Method.Name == "Sql_IsNotNull")
            {
                txt += InvolveMember(node.Arguments[0] as MemberExpression);
                txt += " is not null ";
            }
            else
            {
                object result = MethodCall2Value(node);
                if (result.GetType().Name == "String")
                    txt += "'" + result.ToString() + "'";
                else if (result.GetType().Name == "DateTime")
                {
                    if (vdr == DbVendor.Oracle) //如果是oracle数据库，则日期类型的值需要转化一下。
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

        private string InvolveBinary(BinaryExpression node)
        {
            string clause = " (";
            if (node.Left.ToString() == "True")//处理初始条件为true的情况        
                clause += "1=1";
            else
                clause += Involve(node.Left);

            clause += GetOperStr(node.NodeType);
            clause += Involve(node.Right, true);
            clause += " )";

            return clause;
        }

        private string InvolveMember(MemberExpression node, bool eval = false)
        {
            if (eval)
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
            else if (node.ToString().Contains("value")) //如果包含value就是取表达式值
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

        public Expression Parse(Expression expr)
        {
            return this.Visit(expr);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.ToString().Contains("value")) //如果包含value就是取表达式值
            {
                object result = this.MethodCall2Value(node);
                if (result.GetType().Name == "String")
                {
                    clause.Push("'" + result.ToString() + "'");
                }
                else
                {
                    clause.Push(result.ToString());
                }
            }
            else //否则就是取字段名
            {
                clause.Push(node.Member.Name);
            }

            return node;
        }               

        private string GetMethodCallInVals(MemberExpression expr)
        {
            object vals = MethodCall2Value(expr);

            Type t2 = vals.GetType().GetGenericArguments()[0];
            dynamic items = vals as System.Collections.IEnumerable;
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
    public static class SqlOperators //sql条件操作符扩展类,lambda表达式查询只允许使用如下操作，如：x=>x.Name.Sql_In(...)
    {
        public static bool Sql_In<T>(this T t, List<T> list)
        {
            return true;
        }

        public static bool Sql_Like<T>(this T t, object v)
        {
            return true;
        }

        public static bool Sql_NotIn<T>(this T t, List<T> list)
        {
            return true;
        }

        public static bool Sql_IsNull<T>(this T t)
        {
            return true;
        }

        public static bool Sql_IsNotNull<T>(this T t)
        {
            return true;
        }

        public static bool Sql_Lower<T>(this T t)
        {
            return true;
        }
    }

    /// <summary>
    /// 条件述语拼装生成器
    /// </summary>
    /// 使用方法：
    /// 在需要动态组合条件的情况下
    /// 1.先用类静态方法Ture<T>初始化，如：
    ///    var predicate = PredicateBuilder.True<Employee>();此语句表示若直接解析将返回 1=1，用来接在where之后，完成解析
    /// 2.使用And或Or拼接其他条件,如：
    ///    var predicate = predicate.And(x=>x.Name=="derek");此语句表示在原来的基础上，拼接一个And子句，将返回
    ///     1=1 AND Name='derek'，接在where 之后
    /// 说明：“Employee”是实体类，包含属性Name
    /// 具体使用案例参考引用中的相关代码。
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

#region 表达式分析参考类

public class ExpressionToSql
{
    public string GetSql<T>(Expression<Func<T, T>> exp)
    {
        return DealExpression(exp.Body);
    }
    public string GetSql<T>(Expression<Func<T, bool>> exp)
    {
        return DealExpression(exp.Body);
    }
    private object Eval(MemberExpression member)
    {
        var cast = Expression.Convert(member, typeof(object));
        object c = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
        return GetValueFormat(c);
    }
    private string DealExpression(Expression exp, bool need = false)
    {
        string name = exp.GetType().Name;
        switch (name)
        {
            case "BinaryExpression":
            case "LogicalBinaryExpression":
            case "MethodBinaryExpression":
            case "SimpleBinaryExpression":
                {
                    BinaryExpression b_exp = exp as BinaryExpression;
                    if (exp.NodeType == ExpressionType.Add
                        || exp.NodeType == ExpressionType.Subtract
                        //|| exp.NodeType == ExpressionType.Multiply
                        //|| exp.NodeType == ExpressionType.Divide
                        //|| exp.NodeType == ExpressionType.Modulo
                        )
                    {
                        return "(" + DealBinary(b_exp) + ")";
                    }

                    if (!need) return DealBinary(b_exp);
                    BinaryExpression b_left = b_exp.Left as BinaryExpression;
                    BinaryExpression b_right = b_exp.Right as BinaryExpression;
                    if (b_left != null && b_right != null)
                    {
                        return "(" + DealBinary(b_exp) + ")";
                    }
                    return DealBinary(b_exp);
                }
            case "MemberExpression":
            case "PropertyExpression":
            case "FieldExpression":
                return DealMember(exp as MemberExpression);
            case "ConstantExpression": return DealConstant(exp as ConstantExpression);
            case "MemberInitExpression":
                return DealMemberInit(exp as MemberInitExpression);
            case "UnaryExpression": return DealUnary(exp as UnaryExpression);


            case "MethodCallExpressionN":
                {
                    return DealMethodsCall(exp as MethodCallExpression);
                }

            default:
                Console.WriteLine("error:" + name);

                return "";
        }

    }
    private string DealFieldAccess(FieldAccessException f_exp)
    {
        var c = f_exp;
        return "";
    }
    private string DealMethodsCall(MethodCallExpression m_exp)
    {
        var k = m_exp;
        var g = k.Arguments[0];
        /// 控制函数所在类名。
        if (k.Method.DeclaringType != typeof(SQLMethods))
        {
            throw new Exception("无法识别函数");
        }
        switch (k.Method.Name)
        {
            case "DB_Length":
                {
                    var exp = k.Arguments[0];
                    return "LEN(" + DealExpression(exp) + ")";
                }
            case "DB_In":
            case "DB_NotIn":
                {
                    var exp1 = k.Arguments[0];
                    var exp2 = k.Arguments[1];
                    string methods = string.Empty;
                    if (k.Method.Name == "In")
                    {
                        methods = " IN ";
                    }
                    else
                    {
                        methods = " NOT IN ";
                    }
                    return DealExpression(exp1) + methods + DealExpression(exp2);
                }
            case "DB_Like":
            case "DB_NotLike":
                {
                    var exp1 = k.Arguments[0];
                    var exp2 = k.Arguments[1];
                    string methods = string.Empty;
                    if (k.Method.Name == "DB_Like")
                    {
                        methods = " LIKE ";
                    }
                    else
                    {
                        methods = " NOT LIKE ";
                    }
                    return DealExpression(exp1) + methods + DealExpression(exp2);

                }
        }
        ///   未知的函数
        throw new Exception("意外的函数");
    }
    private string DealUnary(UnaryExpression u_exp)
    {
        var m = u_exp;
        return DealExpression(u_exp.Operand);

    }
    private string DealMemberInit(MemberInitExpression mi_exp)
    {
        var i = 0;
        string exp_str = string.Empty;
        foreach (var item in mi_exp.Bindings)
        {
            MemberAssignment c = item as MemberAssignment;
            if (i == 0)
            {
                exp_str += c.Member.Name.ToUpper() + "=" + DealExpression(c.Expression);
            }
            else
            {
                exp_str += "," + c.Member.Name.ToUpper() + "=" + DealExpression(c.Expression);
            }
            i++;
        }
        return exp_str;

    }
    private string DealBinary(BinaryExpression exp)
    {
        return DealExpression(exp.Left) + NullValueDeal(exp.NodeType, DealExpression(exp.Right, true));// GetOperStr(exp.NodeType) + DealExpression(exp.Right, true);
    }
    private string GetOperStr(ExpressionType e_type)
    {
        switch (e_type)
        {
            case ExpressionType.OrElse: return " OR ";
            case ExpressionType.Or: return "|";
            case ExpressionType.AndAlso: return " AND ";
            case ExpressionType.And: return "&";
            case ExpressionType.GreaterThan: return ">";
            case ExpressionType.GreaterThanOrEqual: return ">=";
            case ExpressionType.LessThan: return "<";
            case ExpressionType.LessThanOrEqual: return "<=";
            case ExpressionType.NotEqual: return "<>";
            case ExpressionType.Add: return "+";
            case ExpressionType.Subtract: return "-";
            case ExpressionType.Multiply: return "*";
            case ExpressionType.Divide: return "/";
            case ExpressionType.Modulo: return "%";
            case ExpressionType.Equal: return "=";
        }
        return "";
    }

    private string DealField(MemberExpression exp)
    {
        return Eval(exp).ToString();
    }
    private string DealMember(MemberExpression exp)
    {
        if (exp.Expression != null)
        {
            if (exp.Expression.GetType().Name == "TypedParameterExpression")
            {
                return exp.Member.Name;
            }
            return Eval(exp).ToString();

        }

        Type type = exp.Member.ReflectedType;
        PropertyInfo propertyInfo = type.GetProperty(exp.Member.Name, BindingFlags.Static | BindingFlags.Public);
        object o;
        if (propertyInfo != null)
        {
            o = propertyInfo.GetValue(type, null);
        }
        else
        {
            FieldInfo field = type.GetField(exp.Member.Name, BindingFlags.Static | BindingFlags.Public);
            o = field.GetValue(null);
        }
        return GetValueFormat(o);

    }
    private string DealConstant(ConstantExpression exp)
    {
        var ccc = exp.Value.GetType();

        if (exp.Value == null)
        {
            return "NULL";
        }
        return GetValueFormat(exp.Value);
    }
    private string NullValueDeal(ExpressionType NodeType, string value)
    {
        if (value.ToUpper() != "NULL")
        {
            return GetOperStr(NodeType) + value;
        }

        switch (NodeType)
        {
            case ExpressionType.NotEqual:
                {
                    return " IS NOT NULL ";
                }
            case ExpressionType.Equal:
                {
                    return " IS NULL ";
                }
            default: return GetOperStr(NodeType) + value;
        }
    }
    private string GetValueFormat(object obj)
    {
        var type = obj.GetType();

        if (type.Name == "List`1") //list集合
        {
            List<string> data = new List<string>();
            var list = obj as System.Collections.IEnumerable;
            string sql = string.Empty;
            foreach (var item in list)
            {
                data.Add(GetValueFormat(item));
            }
            sql = "(" + string.Join(",", data) + ")";
            return sql;
        }

        if (type == typeof(string))// 
        {
            return string.Format("'{0}'", obj.ToString());
        }
        if (type == typeof(DateTime))
        {
            DateTime dt = (DateTime)obj;
            return string.Format("'{0}'", dt.ToString("yyyy-MM-dd HH:mm:ss fff"));
        }
        return obj.ToString();
    }


}

public static class SQLMethods
{
    public static bool DB_In<T>(this T t, List<T> list)  // in
    {
        return true;
    }
    public static Boolean DB_NotIn<T>(this T t, List<T> list) // not in
    {
        return true;
    }
    public static int DB_Length(this string t)  // len();
    {
        return 0;
    }
    public static bool DB_Like(this string t, string str) // like
    {
        return true;
    }
    public static bool DB_NotLike(this string t, string str) // not like 
    {
        return true;
    }
}

#endregion

/*
 * DDL:
 * -- 2020-12-25
 * alter table t_feedback add Last_Update date;
 * 
 * -- 2021-01-11
 * alter table t_feedback add Update_By varchar2(50);
 */