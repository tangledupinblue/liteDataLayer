using System;
using System.Data;

namespace LiteDataLayer.Formatting
{
    public class LiteralSqlString : object
    {
        public object Value { get; private set; }
        public LiteralSqlString(object value)
        {
            Value = value;
        }

        public override string ToString()
        {
            if (Value == null) { return "NULL"; }
            else if (Value == DBNull.Value) { return "NULL"; }
            else return Value.ToString();
        }
    }
}