
using System;
using System.Collections.Generic;
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
}


