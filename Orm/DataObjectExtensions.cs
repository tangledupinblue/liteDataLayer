using System;
using System.Reflection;

namespace LiteDataLayer.Orm
{
    public static class DataObjectExtensions
    {
        public static void UpdateValues<T>(T entity, object[] vals, string[] columnNames)
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
