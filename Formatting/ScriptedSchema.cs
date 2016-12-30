

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
    public class ScriptedColumn {
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }
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
            PropertyName = ColumnName;

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

    public class ScriptedSchema {
        
        //private static string TableRegex = @"^\s*\173(\s*[A-Z,a-z]*)*(\[|\174)";
        private static string TableRegex = @"^\s*(\s*[A-Z,a-z]*)*(\[|\174)";
        //internal static string ColumnRegex = @"(\173[^\173\175]*\175)";
        internal static string ColumnRegex = @"(\[|\,)(\s*[A-Za-z=0-9]*)*";
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
                                //TODO - not sure SchemaDef is relevant anymore
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

        // public ScriptedSchema(Type t, object selector, string directive) {
        //     directive = directive ?? "";
        //     SetTableName(t, directive);
        //     Columns = new List<ScriptedColumn>();
        //     Columns.AddRange((selector.GetType().GetProperties()
        //             .Select(p => new ScriptedColumn(p.Name))).ToArray());
        // }

        public ScriptedSchema ChangeTableNameTo(string tableName) {
            TableName = tableName;
            return this;
        }

        public ScriptedSchema ChangeColumnName(string propName, string columnName) {
            Columns.First(p => p.PropertyName == propName).ColumnName = columnName;
            return this;
        }

        public ScriptedSchema IgnoreColumns(params string[] columnNames) {
            foreach (string columnName in columnNames) {
                Columns.Remove(Columns.First(p => p.ColumnName == columnName));
            }    
            return this;
        }

        public ScriptedSchema UseOnlyColumns(params string[] columnNames) {
            Columns = Columns.Join(columnNames,
                                l => l.ColumnName,
                                r => r,
                                (l,r) => l).ToList();
            return this;
        }

        public ScriptedSchema MakeKeyColumns(params string[] columnNames) {
            foreach (var column in Columns.Join(columnNames,
                                l => l.ColumnName,
                                r => r,
                                (l,r) => l)) {
                column.IsKey = true;
            }
            return this;
        }

        public ScriptedSchema MakeAutoIDColumn(string columnName) {
            Columns.First(p => p.ColumnName == columnName).IsAutoID = true;
            return this;
        }

        public ScriptedSchema MakeReadOnlyColumns(params string[] columnNames) {
            foreach (var column in Columns.Join(columnNames,
                                l => l.ColumnName,
                                r => r,
                                (l,r) => l)) {
                column.IsReadOnly = true;
            }
            return this;
        }
        
        public ScriptedSchema MakeWriteOnlyColumns(params string[] columnNames) {
            foreach (var column in Columns.Join(columnNames,
                                l => l.ColumnName,
                                r => r,
                                (l,r) => l)) {
                column.IsWriteOnly = true;
            }
            return this;
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

