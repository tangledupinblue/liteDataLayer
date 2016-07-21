using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;


namespace LiteDataLayer.Formatting
{
    public static class SqlFormatter
    {
        public static string ShortDateFormat { get; set; } = "yyyyMMdd";

        public static string Format(string formatString, params object[] parameters)
        {
            return string.Format(formatString, parameters.Select(p => GetSqlString(p))
                                                    .Cast<object>().ToArray());
        }

        public static string GetSqlString(object self) {
            return GetSqlString(self, self.GetType());
        }

        public static string GetSqlString(object valObject, Type dataType)
        {
            try
            {
                string valString = "NULL";
                switch (dataType.ToString())
                {
                    case "System.String":
                        valString = valObject == null ? "NULL" 
                                        : "'" + valObject.ToString().Replace("'", "''") + "'";
                        break;
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        valString = valObject.ToString();
                        break;
                    case "System.DateTime":
                        if ((DateTime)valObject == DateTime.MinValue)
                        {
                            valString = "NULL";
                        }
                        else
                        {
                            DateTimeFormatInfo fi = new DateTimeFormatInfo();
                            fi.ShortDatePattern = ShortDateFormat;
                            //cast as a date because we need to
                            DateTime dt = (DateTime)valObject;
                            //convert it to our stored format
                            valString = "'" + dt.ToString(fi) + "'";
                            //sql = obj.ToString();
                        }
                        break;
                    case "System.Boolean":
                        {
                            if ((bool)valObject == false)
                            {
                                valString = "0";
                            }
                            else { valString = "1"; }
                        }
                        break;
                    case "System.Float":
                        //invariant culture seems to behave differently on ubuntu 
                        valString = ((float)valObject).ToString("N").Replace(",",".");
                        break;
                    case "System.Decimal":
                        valString = ((decimal)valObject).ToString("N").Replace(",",".");
                        break;
                    case "System.Double":
                        valString = ((double)valObject).ToString("N").Replace(",",".");
                        break;
                    case "System.Guid":
                        valString = "'" + valObject.ToString() + "'";
                        break;
                    case "System.Byte[]":
                        valString = "0x" + string.Concat(((byte[])valObject).Select(b => b.ToString("X2")));
                        break;
                    default:
                        valString = "NULL";
                        break;
                }
                return valString;
            }
            catch (Exception ex) {
                throw new Exception(string.Format("Error formatting {0} of type {1} to sql:\n{2}",
                            valObject, dataType.ToString(), ex.Message)); 
            }
        }
    }
}
