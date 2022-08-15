using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace PropertyOneAppWeb.Commom
{
    public class Oracle9i
    {
        private string connString;
        private System.Data.OracleClient.OracleCommand cmd;

        public Oracle9i(string connectString)
        {
            connString = connectString;
            cmd = new System.Data.OracleClient.OracleCommand();
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        public void AddParameter(string name, object value)
        {
            if (value == null)
            {
                cmd.Parameters.AddWithValue(name, System.DBNull.Value);
            }
            else
                cmd.Parameters.AddWithValue(name, value);
        }

        /// <summary>
        /// 清除参数
        /// </summary>
        public void ClearParameter()
        {
            cmd.Parameters.Clear();
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="direct"></param>
        /// <param name="value"></param>
        public void AddParameter(string name, System.Data.OracleClient.OracleType type, ParameterDirection direct, int size = 0, object value = null)
        {
            System.Data.OracleClient.OracleParameter op = new System.Data.OracleClient.OracleParameter();
            op.Direction = direct;
            op.ParameterName = name;
            op.OracleType = type;
            if (size > 0) op.Size = size;
            if (value != null) op.Value = value;

            this.cmd.Parameters.Add(op);
        }

        /// <summary>
        /// 添加参数(静态版本)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="direct"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static System.Data.OracleClient.OracleParameter BuildParameter(string name, System.Data.OracleClient.OracleType type, ParameterDirection direct, int size = 0, object value = null)
        {
            System.Data.OracleClient.OracleParameter op = new System.Data.OracleClient.OracleParameter();
            op.Direction = direct;
            op.ParameterName = name;
            op.OracleType = type;
            if (size > 0) op.Size = size;
            if (value != null) op.Value = value;

            return op;
        }

        public object ExecuteStoredProcedure(string commandText)
        {
            System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connString);
            try
            {
                cmd.Connection = conn;
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.StoredProcedure;

                //判断返回的类型
                System.Data.OracleClient.OracleType retType = System.Data.OracleClient.OracleType.VarChar;
                foreach (System.Data.OracleClient.OracleParameter op in cmd.Parameters)
                {
                    if (op.OracleType == System.Data.OracleClient.OracleType.Cursor)
                    {
                        retType = System.Data.OracleClient.OracleType.Cursor;
                    }
                }
                conn.Open();
                if (retType == System.Data.OracleClient.OracleType.Cursor)
                {
                    System.Data.OracleClient.OracleDataAdapter oda = new System.Data.OracleClient.OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);
                    return dt;
                }
                else
                {
                    cmd.ExecuteNonQuery();
                    string result = "";
                    foreach (System.Data.OracleClient.OracleParameter op in cmd.Parameters)
                    {
                        if (op.Direction == ParameterDirection.Output || op.Direction == ParameterDirection.ReturnValue)
                        {
                            result = op.Value.ToString();
                            break;
                        }
                    }
                    return result;
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
