using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Dynamic;

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
                            .Join(entity.GetProps(),
                                (left) => left, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
                                                        type = right.PropertyType, 
                                                        val = right.Value })
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
                                .Join(entity.GetProps(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        type = right.PropertyType, 
                                                        val = right.Value });
            if (!whereCols.Any()) { 
                throw new Exception(string.Format("Invalid Columns Specified for {0}\r\n{1}",
                        entity.GetType(), schema.ToString()));
            }
            string insert = string.Format("SELECT {1} FROM {0} WHERE {2}",
                        schema.TableName,
                        string.Join(", ", schema.Columns.Select(p => p.ColumnName + " AS " + p.PropertyName)),                    
                        string.Join(" AND ", whereCols
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
            return insert;                    
        }

        public string ScriptUpdate(object entity, ScriptedSchema schema = null) {
            schema = schema ?? new ScriptedSchema(entity.GetType(), "");
            var cols = schema.Columns  //.Select(p => new { p.ColumnName, p.IsKey })
                            .Join(entity.GetProps(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        isKey = left.IsKey,
                                                        type = right.PropertyType, 
                                                        val = right.Value })
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
                        string.Join(" AND ", cols.Join(entity.GetProps(),
                                (left) => left.PropertyName, (right) => right.Name,
                                (left,right) => new {   name = left.ColumnName,
                                                        type = right.PropertyType, 
                                                        val = right.Value })
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
            return insert;                    
        }

        // public string ScriptSelect(Type type, ScriptedSchema schema = null) {
        //     schema = schema ?? new ScriptedSchema(type, "");
        //     string sql = string.Format("SELECT {1} FROM {0}", schema.TableName,
        //             string.Join(", ", schema.Columns.Select(p => p.ColumnName + " AS " + p.PropertyName)));
        //     return sql;
        // }

        public string ScriptSelect(ScriptedSchema schema) {
            string sql = string.Format("SELECT {1} FROM {0}", schema.TableName,
                    string.Join(", ", schema.Columns.Select(p => p.ColumnName + " AS " + p.PropertyName)));
            return sql;
        }

        public string ScriptSelect(object selector , ScriptedSchema schema) {
            Console.WriteLine(selector);
            var whereCols = from prop in selector.GetProps()
                            join col in schema.Columns
                            on prop.Name equals col.PropertyName
                            select new { name = col.ColumnName,
                                        type = prop.PropertyType,
                                        val = prop.Value };
            if (!whereCols.Any()) { 
                throw new Exception(string.Format("Invalid Columns Specified for {0}\r\n{1}",
                        schema.Type, schema.ToString()));
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

    public struct SimplePropInfo
    {   
        public string Name { get; set; }
        public Type PropertyType { get; set; }
        public object Value { get; set; }
    }

    static class PropertyExtensions
    {

        public static List<SimplePropInfo> GetProps(this object self) {
         
            return self is ExpandoObject
                ? ((ExpandoObject)self).Select(p => 
                        new SimplePropInfo() {
                            Name = p.Key,
                            PropertyType = p.Value.GetType(),
                            Value = p.Value
                        }).ToList()
                : self.GetType().GetProperties().Select(p => 
                        new SimplePropInfo() {
                            Name = p.Name,  
                            PropertyType = p.PropertyType,
                            Value = p.GetValue(self)
                        }).ToList();
        }
    }

}