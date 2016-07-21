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
        //protected const string SQL_INSERT = "INSERT <Table> (<Columns>) VALUES (<Values>)";
        //protected const string SQL_LOAD = "SELECT * FROM <Table> WHERE <KeyColumnsAndValues>";
        //protected const string SQL_UPDATE = "UPDATE <Table> SET <NonKeyColumnsAndValues> WHERE <KeyColumnsAndValues>";
        //protected const string SQL_DELETE = "DELETE FROM <Table> WHERE <KeyColumnsAndValues>";
        //protected const string SQL_FIND_ALL = "SELECT * FROM <Table>";
        //protected const string SQL_FIND_BY_NAME = "SELECT * FROM <Table> WHERE <NameCol> = <NameValue> ";
        //protected const string SQL_EXISTS = "SELECT COUNT(*) FROM <Table> WHERE <KeyColumnsAndValues>";
        //protected const string SQL_FIND_WHERE = "SELECT * FROM <Table> WHERE <WhereClause>";

        // private string lastScript = null;
        // private string lastRequest = null;
        // private ScriptedSchema lastSchema = null;
        
        public string ScriptInsert(object entity, string script) {
            var schema = new ScriptedSchema(entity.GetType(), script);
            // var schema = lastRequest == "insert" && lastScript == script    
            //             ? lastSchema
            //             : new ScriptedSchema(entity.GetType(), script);
            string[] cols = schema.Columns.Where(p => !(p.IsAutoID || p.IsReadOnly || p.Ignore))
                                    .Select(p => p.ColumnName).ToArray();
            string insert = string.Format("INSERT {0} ({1}) VALUES ({2}); {3}",
                        schema.TableName,
                        string.Join(", ", cols),
                        string.Join(", ", cols.Join(entity.GetType().GetProperties(),
                                (left) => left, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) })
                            .Select(p => SqlFormatter.GetSqlString(p.val, p.type))),
                        schema.Columns.Any(p => p.IsAutoID)
                            ? string.Format("SELECT SCOPE_IDENTITY() AS {0}", schema.Columns.First().ColumnName)
                            : "");
            // lastRequest = "insert";
            // lastScript = script;
            return insert;                    
        }

        public string ScriptLoad(object entity, string script) {
            var schema = new ScriptedSchema(entity.GetType(), script);
            // var schema = lastRequest == "load" && lastScript == script    
            //             ? lastSchema
            //             : new ScriptedSchema(entity.GetType(), script);
            var whereCols =    schema.Columns.Where(p => p.IsKey)
                                .Select(p => p.ColumnName)
                                .Join(entity.GetType().GetProperties(),
                                (left) => left, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
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
            // lastRequest = "load";
            // lastScript = script;
            return insert;                    
        }

        public string ScriptUpdate(object entity, string script) {
            var schema = new ScriptedSchema(entity.GetType(), script);
            // var schema = lastRequest == "update" && lastScript == script    
            //             ? lastSchema
            //             : new ScriptedSchema(entity.GetType(), script);
            var cols = schema.Columns.Select(p => new { p.ColumnName, p.IsKey })
                            .Join(entity.GetType().GetProperties(),
                                (left) => left.ColumnName, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
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
            // lastRequest = "update";
            // lastScript = script;
            return insert;                    
        }

        public string ScriptDelete(object entity, string script) {
            var schema = new ScriptedSchema(entity.GetType(), script);
            // var schema = lastRequest == "load" && lastScript == script    
            //             ? lastSchema
            //             : new ScriptedSchema(entity.GetType(), script);
            string[] cols = schema.Columns.Where(p => p.IsKey)
                                    .Select(p => p.ColumnName).ToArray();
            string insert = string.Format("DELETE FROM {0} WHERE {1}",
                        schema.TableName,
                        string.Join(" AND ", cols.Join(entity.GetType().GetProperties(),
                                (left) => left, (right) => right.Name,
                                (left,right) => new {   name = right.Name,
                                                        type = right.PropertyType, 
                                                        val = right.GetValue(entity) })
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
            // lastRequest = "load";
            // lastScript = script;
            return insert;                    
        }

        public string ScriptSelect(Type type, string script = "") {
            if (string.IsNullOrEmpty(script)) {
                return "SELECT * FROM " + type.Name;
            }
            var schema = new ScriptedSchema(type, script);
            // var schema = lastRequest == "select" && lastScript == script    
            //             ? lastSchema
            //             : new ScriptedSchema(type, script);
            string sql = string.Format("SELECT * FROM {1}", schema.TableName);
            // lastRequest = "select";
            // lastScript = script;
            return sql;
        }

        public string ScriptSelect(Type type, dynamic selector, string script = "") {
            var schema = new ScriptedSchema(type, selector, script);
            var whereCols =    schema.Columns.Where(p => p.IsKey)
                                .Select(p => p.ColumnName)
                                .Join((selector as IDictionary<string,object>)
                                            .Cast<KeyValuePair<string,object>>()
                                            .Select(p => new { p.Key, p.Value }),
                                (left) => left, (right) => right.Key,
                                (left,right) => new {   name = right.Key,
                                                        type = right.Value.GetType(), 
                                                        val = right.Value });
            if (!whereCols.Any()) { 
                throw new Exception(string.Format("Invalid Columns Specified for {0}\r\n{1}",
                        type, schema.ToString()));
            }
            return string.Format("SELECT * FROM {0} WHERE {1}",
                    schema.TableName,
                        string.Join(" AND ", whereCols
                            .Select(p => p.name + " = " + SqlFormatter.GetSqlString(p.val, p.type))));
        }

        public void DebugScript(object entity, string script) {
            Console.WriteLine(script);
            ScriptedSchema ss = new ScriptedSchema(entity.GetType(), script);
            Console.WriteLine(ss.ToString());
        }

        private class ScriptedColumn {
            public string ColumnName { get; set; }
            public bool IsKey { get; set; }
            public bool IsAutoID { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsWriteOnly { get; set; }
            public bool IsName { get; set; }
            public int Length { get; set; }
            public bool Ignore { get; set; }

            public ScriptedColumn(string script) {
                var nameMatches = new Regex(ScriptedSchema.NameRegex).Matches(script);
                ColumnName = nameMatches[0].Value;

                IsKey = new Regex(string.Format("{0}[k]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                IsAutoID = new Regex(string.Format("{0}[a]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                IsReadOnly = new Regex(string.Format("{0}[r]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                IsWriteOnly = new Regex(string.Format("{0}[w]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                IsName = new Regex(string.Format("{0}[n]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                Ignore = new Regex(string.Format("{0}[i]{0}", ScriptedSchema.NotNameComponent))
                                    .Matches(script).Count > 0;
                var lengthMatches = new Regex(ScriptedSchema.LengthRegex).Matches(script);
                Length = lengthMatches.Count > 0 ? Convert.ToInt32(lengthMatches[0].Value.Replace("l=",""))
                                                : 0;

            }

            public override string ToString() {
                return new StringBuilder().Append(ColumnName).Append(" ")
                                .Append(IsKey ? " key" : "").Append(IsAutoID ? " autoid" : "")
                                .Append(IsReadOnly ? " readonly" : "")
                                .Append(IsWriteOnly ? " writeonly" : "")
                                .Append(IsName ? " name" : "")
                                .Append(Length > 0 ? Length.ToString() : "")
                                .Append(Ignore ? " ingore" : "").ToString();                
            }
        }

        private class ScriptedSchema {
            
            private static string TableRegex = @"^\s*\173(\s*[A-Z,a-z]*)*(\[|\174)";
            internal static string ColumnRegex = @"(\173[^\173\175]*\175)";
            //[^A-Z,a-z,_,0-9][o][^A-Z,a-z,_,0-9]
            internal static string NameRegex = @"[A-Z,a-z,_,0-9]{2,}";
            internal static string NotNameComponent = @"[^A-Z,a-z,_,0-9]"; 

            internal static string LengthRegex = @"[l]\=[0-9]+";
            public string TableName { get; set; }
            public bool IncludeOnly { get; set; }
            public List<ScriptedColumn> Columns { get; set; }

             
            public ScriptedSchema(Type type, string directive = null) {
                string op = "initialising";
                directive = directive ?? "";
                SetTableName(type, directive);
                try {                    
                    op = "read scripted columns";
                    var columns = new Regex(ColumnRegex).Matches(directive);
                    var scriptedCols = (from match in columns.Cast<Match>().ToArray()
                            select new ScriptedColumn(match.Value)).ToList();
                    Columns = new List<ScriptedColumn>(scriptedCols);
                    if (!IncludeOnly) {
                        op = "adding column defaults using reflection";
                        var entityCols = type.GetProperties()
                                    .Where(p => !p.PropertyType.IsArray && !p.PropertyType.IsNested)
                                    .Select(p => p.Name)
                                    .Except(Columns.Select(p => p.ColumnName).Append("SchemaDef"))
                                    .Select(p => new ScriptedColumn(p));
                        Columns.AddRange(entityCols.ToArray());
                    }                           
                    if (!Columns.Any(p => p.IsKey)) {
                        Columns[0].IsKey = true;
                    }
                } catch (Exception ex) {
                    throw new Exception(
                        string.Format("Error reading format directive:\nDirective: {0}\nOperation: {1}\nError: {2}",
                            directive, op, ex.Message));
                }
            }

            public ScriptedSchema(Type t, dynamic selector, string directive) {
                directive = directive ?? "";
                SetTableName(t, directive);
                Columns = new List<ScriptedColumn>();
                Columns.AddRange(((selector as IDictionary<string, object>)
                        .Select(p => new ScriptedColumn(p.Key))).ToArray());

                //selector.GetType().GetProperties().ToList();                                    ;
                //Columns.Add(pis.Select(p => new ScriptedColumn(p.)));
            }

            private void SetTableName(Type type, string directive) {
                string op = "read table matches";
                try
                {
                    TableName = type.Name;
                    var tablePartMatches = new Regex(TableRegex).Matches(directive) ;
                    if (tablePartMatches.Count > 0) { 
                        var nameMatches = new Regex(NameRegex).Matches(tablePartMatches[0].Value);
                        TableName = nameMatches.Count > 0 ? nameMatches[0].Value : TableName;                        
                        op = "read include only";
                        IncludeOnly = new Regex(string.Format("{0}[o]{0}", NotNameComponent))
                                        .Matches(tablePartMatches[0].Value).Count > 0;                    
                    }
                } catch (Exception ex) {
                    throw new Exception(
                        string.Format("Error reading format directive:\nDirective: {0}\nOperation: {1}\nError: {2}",
                            directive, op, ex.Message));
                }
            }

            public override string ToString() {
                return string.Format("{0} {1}\r\n{2}", TableName, IncludeOnly ? " Only " : " ",
                            string.Join("; ", Columns.Select(p => p.ToString())));
            }
        }
    }
}