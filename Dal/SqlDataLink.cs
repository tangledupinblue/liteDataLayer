using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using LiteDataLayer.DataStructures;

namespace LiteDataLayer.Dal
{
    public class SqlDataLink : IDataLink
    {
        //private SqlConnection Connection { get; set; }
        public string ConnectionString { get; private set; } 
        public string ShortDateFormat { get { return "yyyyMMdd"; } }

        public SqlDataLink(string connectionString) {
            ConnectionString = connectionString;
        }
     
        public int ExecuteNonQuery(string cmd, int sqlCommandTimeout = 30) {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand sqlComm = new SqlCommand(cmd, conn))
                {
                    int ret = sqlComm.ExecuteNonQuery();
                    return ret;
                }
                conn.Close();
            }
        }

        public T ExecuteScalar<T>(string cmd, int sqlCommandTimeout = 30)
        {
            object obj = ExecuteScalar(cmd, sqlCommandTimeout);
            if (obj == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }

        public object ExecuteScalar(string cmd, int sqlCommandTimeout = 30)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(cmd, conn))
            {
                conn.Open();
                object obj = command.ExecuteScalar();
                return obj;
                //conn.Close();
            }
        }
  
        public LiteTable[] GetTabularSets(string cmd, int sqlCommandTimeout = 30)
        {
            List<LiteTable> tables = new List<LiteTable>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(cmd, conn))
            {
                conn.Open();
                SqlDataReader reader = command.ExecuteReader();
                tables.Add(ReadResult(reader));
                while (reader.NextResult()) {
                    tables.Add(ReadResult(reader));
                }
                End();
                return tables.ToArray();                    
            }

            // try {
            //     List<LiteTable> tables = new List<LiteTable>();
            //     SqlCommand command = GetCommand(cmd, sqlCommandTimeout);
            //     SqlDataReader reader = command.ExecuteReader();
            //     tables.Add(ReadResult(reader));
            //     while (reader.NextResult()) {
            //         tables.Add(ReadResult(reader));
            //     }
            //     End();
            //     return tables.ToArray();
            // } catch (Exception ex) {
            //     throw new Exception(string.Format("DataLink Error:{0}\r\n{1}",
            //         ex.Message, cmd), ex);
            // } finally {
            //     End();
            // }
        }

        public string DataSource
        {
            get { return new SqlConnection(ConnectionString).DataSource; }
        }

        public string Database
        {
            get { return new SqlConnection(ConnectionString).Database; }
        }

        public string ConnectionType
        {
            get { return "Database"; }
        }

        private void End()
        {
        }

        // private SqlCommand GetCommand(string commandString, int sqlCommandTimeOut)
        // {
        //     SqlCommand comm = new SqlCommand(commandString, Connection as SqlConnection);
        //     comm.CommandTimeout = sqlCommandTimeOut;
        //     if (Connection.State != ConnectionState.Open) {
        //         Connection.Open();
        //     }
        //     return comm;
        // }

        private LiteTable ReadResult(SqlDataReader reader)
        {
            LiteTable lt = new LiteTable();
            //soon to be implemented I think - reader.GetColumnSchema
            //readonly collection DbColumn
            // for (int i = 0; i < reader.FieldCount; i++) {
            //     lt.Columns.Add(new LiteColumn(reader.GetName(i), reader.GetFieldType(i)));
            // }
                        // for (int i = 0; i < reader.FieldCount; i++) {
            //     lt.Columns.Add(new LiteColumn(reader.GetName(i), reader.GetFieldType(i)));
            // }
            for (int i = 0; i < reader.FieldCount; i++) {
                lt.ColumnNames.Add(reader.GetName(i));
                lt.ColumnTypes.Add(reader.GetFieldType(i).FullName);
            }
            while (reader.Read()) {
                object[] next = new object[reader.FieldCount];
                reader.GetValues(next);
                lt.Rows.Add(next);
            }
            return lt;
        }
    }
}

