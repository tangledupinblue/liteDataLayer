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
        private SqlConnection Connection { get; set; }
        public string ShortDateFormat { get { return "yyyyMMdd"; } }

        public SqlDataLink(string ConnectionString) {
            Connection = new SqlConnection(ConnectionString);
        }
     
        public int ExecuteNonQuery(string cmd, int sqlCommandTimeout = 30) {
            try {
                SqlCommand command = GetCommand(cmd, sqlCommandTimeout);
                int ret = command.ExecuteNonQuery();
                End();
                return ret;
            } catch (Exception ex) {
                throw new Exception(string.Format("DataLink Error:{0}\r\n{1}",
                    ex.Message, cmd), ex);
            }
            finally {
                End();
            }
        }

        public object ExecuteScalar(string cmd, int sqlCommandTimeout = 30) {
            try {
                SqlCommand command = GetCommand(cmd, sqlCommandTimeout);
                object obj = command.ExecuteScalar();
                End();
                return obj; 
            } catch (Exception ex) {
                throw new Exception(string.Format("DataLink Error:{0}\r\n{1}",
                    ex.Message, cmd), ex);
            } finally {
                End();
            }
        }

        public T ExecuteScalar<T>(string cmd, int sqlCommandTimeout = 30)
        {
            object obj = ExecuteScalar(cmd, sqlCommandTimeout);
            return obj == DBNull.Value ? default(T) : (T)obj; 
        }
  
        public LiteTable[] GetTabularSets(string cmd, int sqlCommandTimeout = 30)
        {
            try {
                List<LiteTable> tables = new List<LiteTable>();
                SqlCommand command = GetCommand(cmd, sqlCommandTimeout);
                SqlDataReader reader = command.ExecuteReader();
                tables.Add(ReadResult(reader));
                while (reader.NextResult()) {
                    tables.Add(ReadResult(reader));
                }
                End();
                return tables.ToArray();
            } catch (Exception ex) {
                throw new Exception(string.Format("DataLink Error:{0}\r\n{1}",
                    ex.Message, cmd), ex);
            } finally {
                End();
            }
        }

        public string DataSource
        {
            get { return (Connection == null) ? "None" : Connection.DataSource; }
        }

        public string Database
        {
            get { return (Connection == null) ? "None" : Connection.Database; }
        }

        public string ConnectionType
        {
            get { return "Database"; }
        }

        private void End()
        {
            if (Connection.State == ConnectionState.Open) {
                Connection.Close();
            }
        }

        private SqlCommand GetCommand(string commandString, int sqlCommandTimeOut)
        {
            SqlCommand comm = new SqlCommand(commandString, Connection as SqlConnection);
            comm.CommandTimeout = sqlCommandTimeOut;
            if (Connection.State != ConnectionState.Open) {
                Connection.Open();
            }
            return comm;
        }

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

