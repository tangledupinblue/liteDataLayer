using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiteDataLayer.DataStructures
{
    public class LiteTable 
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<object[]> Rows { get; set; }

        public LiteTable() {
            ColumnNames = new List<string>();
            ColumnTypes = new List<string>();
            Rows = new List<object[]>();
        }

        public string ToTextResults(string delimiter) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Join(delimiter, ColumnNames));
            var rows = Rows.Select(p => string.Join(delimiter, p.Select(q => (q == DBNull.Value) ? "NULL" : q.ToString())));
            foreach (var row in rows) {
                sb.AppendLine(row);                
            }
            return sb.ToString();
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
}
