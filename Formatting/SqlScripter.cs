using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LiteDataLayer.Formatting
{
    public class SqlScripter
    {        
        public string ScriptInsert(object entity, ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(entity.GetType(), "");
            // string[] cols = schema.Columns.Where(p => !(p.IsAutoID || p.IsReadOnly || p.Ignore))
            //                         .Select(p => p.ColumnName).ToArray();
            var cols = schema.Columns.Where(p => !(p.IsAutoID || p.IsReadOnly || p.Ignore));
            string insert = string.Format("INSERT {0} ({1}) VALUES ({2}); {3}",
                        schema.TableName,
                        string.Join(", ", cols.Select(p => p.ColumnName)),
                        string.Join(", ", cols.Select(p => p.PropertyName)
                            .Join(entity.GetType().GetProperties(),
                                (left) => left, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) })
                            .Select(p => SqlFormatter.GetSqlString(p.val, p.type))),
                        schema.Columns.Any(p => p.IsAutoID)
                            ? string.Format("SELECT SCOPE_IDENTITY() AS {0}", schema.Columns.First().ColumnName)
                            : "");
            return insert;                    
        }

        public string ScriptLoad(object entity, ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(entity.GetType(), "");
            var whereCols =    schema.Columns.Where(p => p.IsKey)
                                //.Select(p => p.ColumnName)
                                .Join(entity.GetType().GetProperties(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) });
            if (!whereCols.Any()) { 
                throw new Exception(string.Format("Invalid Columns Specified for {0}\r\n{1}",
                        entity.GetType(), schema.ToString()));
            }
            string insert = string.Format("SELECT * FROM {0} WHERE {1}",
                        schema.TableName,
                        string.Join(" AND ", whereCols
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
            return insert;                    
        }

        public string ScriptUpdate(object entity, ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(entity.GetType(), "");
            var cols = schema.Columns  //.Select(p => new { p.ColumnName, p.IsKey })
                            .Join(entity.GetType().GetProperties(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        isKey = left.IsKey,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) })
                            .ToArray();

            string insert = string.Format("UPDATE {0} SET {1} WHERE {2}",
                        schema.TableName,
                        string.Join(", ", cols.Where(p => !p.isKey)
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))),
                        string.Join(" AND ", cols.Where(p => p.isKey)
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));                            
            return insert;                    
        }

        public string ScriptDelete(object entity, ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(entity.GetType(), "");
            var cols = schema.Columns.Where(p => p.IsKey);
            string insert = string.Format("DELETE FROM {0} WHERE {1}",
                        schema.TableName,
                        string.Join(" AND ", cols.Join(entity.GetType().GetProperties(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) })
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
            return insert;                    
        }

        public string ScriptSelect<T>(ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(typeof(T), "");
            string sql = string.Format("SELECT {1} FROM {0}", schema.TableName,
                    string.Join(", ", schema.Columns.Select(p => p.ColumnName + " AS " + p.PropertyName)));
            return sql;
        }

        public string ScriptSelect<T>(object selector , ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(typeof(T), "");
            Console.WriteLine(selector);
            var whereCols = from prop in selector.GetType().GetProperties()
                            join col in schema.Columns
                            on prop.Name equals col.PropertyName
                            select new { name = col.ColumnName,
                                        type = prop.PropertyType,
                                        val = prop.GetValue(selector) };
            if (!whereCols.Any()) { 
                throw new Exception(string.Format("Invalid Columns Specified for {0}\r\n{1}",
                        typeof(T), schema.ToString()));
            }
            return string.Format("SELECT {1} FROM {0} WHERE {2}",
                    schema.TableName,
                    string.Join(", ", schema.Columns.Select(p => p.ColumnName + " AS " + p.PropertyName)),                    
                        string.Join(" AND ", whereCols
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
        }

        public void DebugScript(object entity, string script) {
            Console.WriteLine(script);
            ScriptedSchema ss = new ScriptedSchema(entity.GetType(), script);
            Console.WriteLine(ss.ToString());
        }
    }
}