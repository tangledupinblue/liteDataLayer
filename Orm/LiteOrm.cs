
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;


namespace LiteDataLayer.Orm
{
        
    public class LiteOrm
    {
        private IDataLink dataLink;
        private SqlScripter scripter;

        public LiteOrm(IDataLink dataLink, SqlScripter scripter)
        {
            this.dataLink = dataLink;
            this.scripter = scripter;
        }
        //nvm merged from stockmodule
        public List<dynamic> SelectToDynamic(string sql)
        {
            LiteTable[] lts = dataLink.GetTabularSets(sql,30);
            return lts[0].ToDynamic();
        }

        public void Insert<T>(T entity) where T : IScriptable {
            string sql = scripter.ScriptInsert(entity, entity.SchemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            if (lts[0].Rows.Count >0) {
                DataObjectExtensions.UpdateValues<T>(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
        }

        public void Insert<T>(List<T> entities) where T : IScriptable {
            foreach (var entity in entities) {
                Insert<T>(entity);
            }
        }

        public void Load<T>(T entity) where T : IScriptable {
            string sql = scripter.ScriptLoad(entity, entity.SchemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            Console.WriteLine(lts[0].ToTextResults("|"));
            DataObjectExtensions.UpdateValues<T>(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
        }
        public void Update<T>(T entity) where T : IScriptable {
            string sql = scripter.ScriptUpdate(entity, entity.SchemaDef);
            dataLink.ExecuteNonQuery(sql);
        }
        public void Delete<T>(T entity) where T : IScriptable {
            string sql = scripter.ScriptDelete(entity, entity.SchemaDef);
            dataLink.ExecuteNonQuery(sql);
        }

        public List<T> Select<T>(string sql)
        {
            LiteTable[] lts = dataLink.GetTabularSets(sql, 30);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();
        }

        public List<T> SelectAll<T>()
        {            
            string sql = scripter.ScriptSelect(typeof(T));
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();
        }

        public List<T> SelectWhere<T>(object selector) {
            string sql = scripter.ScriptSelect(typeof(T), selector);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();            
        }

        public T First<T>(string sql)
        {
            LiteTable[] lts = dataLink.GetTabularSets(sql,30);
            if (lts[0].Rows.Count  > 0)
            {
                return DataObjectExtensions.CreateNewType<T>(lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
            return default(T);
        }
    }   
    public static class DataTableExtensions
    {
        public static List<dynamic> ToDynamic(this LiteTable lt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (object[] row in lt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                //foreach (DataColumn column in dt.Columns)
                for (int i = 0; i < lt.ColumnNames.Count; i++) 
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[lt.ColumnNames[i]] = row[i] == DBNull.Value
                                        ? null
                                        : row[i];
                }
                dynamicDt.Add(dyn);
            }
            return dynamicDt;
        }
    }
    internal static class DataObjectExtensions
    {
        internal static void UpdateValues<T>(T entity, object[] vals, string[] columnNames)
        {
            Type thisone = entity.GetType();
            for (int i = 0; i < columnNames.Length; i++)
            {                
                PropertyInfo pi = thisone.GetProperty(columnNames[i]);
                try
                {
                    if (pi != null)
                    {
                        if (vals[i] != DBNull.Value)
                        {
                            if (pi.PropertyType.Equals(vals[i].GetType()))
                            {
                                pi.SetValue(entity, vals[i], null);
                            }
                            else
                            {
                                object val = vals[i];
                                if (pi.PropertyType == typeof(Guid) && val is string)
                                {
                                    if (val == null)
                                    {
                                        val = Guid.Empty;
                                    }
                                    else
                                    {
                                        val = Guid.Parse((string)val);
                                    }
                                }
                                pi.SetValue(entity, ChangeType(val, pi.PropertyType), null);
                            }
            //                pi.SetValue(item,
            //propType == dc.DataType
            //    ? self[dc.ColumnName]
            //    : Convert.ChangeType(self[dc.ColumnName], pi.PropertyType), null);

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        string.Format("Error setting property {0}:\n{1}", pi == null ? "UNIDENTIFIED" : pi.Name, ex.Message), ex);
                }
            }
        } 

        public static T CreateNewType<T>(object[] vals, string[] columnNames)
        {
            T item = Activator.CreateInstance<T>();
            UpdateValues<T>(item, vals, columnNames);
            return item;
        }

        private static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
    }
}    

