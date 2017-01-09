using System;
using System.Reflection;
using System.Text;
using LiteDataLayer.Formatting;

namespace LiteDataLayer.Orm
{
    public static class DataObjectExtensions
    {
        public static void UpdateValues(object entity, object[] vals, string[] columnNames)
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

        public static T CopyValues<T>(object original) {
            return (T)CopyValues(original, CreateNewType<T>(new object[] {}, new string[] {}));
        }
        
        public static T CopyToNew<T>(this object self) {
            return CopyValues<T>(self);
        }

        public static object CopyValues(object original, object target) {
            Type thisone = target.GetType();
            var originalProps = original.GetType().GetProperties();
            foreach(var originalProp in originalProps)
            {                
                PropertyInfo pi = thisone.GetProperty(originalProp.Name);
                try
                {
                    if (pi != null)
                    {
                        object originalVal = originalProp.GetValue(original);
                        if (pi.PropertyType.Equals(originalProp.PropertyType))
                        {
                            pi.SetValue(target, originalVal, null);
                        }
                        else
                        {
                            pi.SetValue(target, ChangeType(originalVal, pi.PropertyType), null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        string.Format("Error setting property {0}:\n{1}", pi == null ? "UNIDENTIFIED" : pi.Name, ex.Message), ex);
                }
            }
            return target;
        } 

        public static T CreateNewType<T>(object[] vals, string[] columnNames) {
            return (T)CreateNewType(vals, columnNames, typeof(T));
        }

        public static object CreateNewType(object[] vals, string[] columnNames, Type type) {
            object item = Activator.CreateInstance(type);
            UpdateValues(item, vals, columnNames);
            return item;
        }

        public static string StringifyValues(this object self) {
            StringBuilder sb = new StringBuilder();
            sb.Append(" { ");
            var originalProps =self.GetType().GetProperties();
            foreach(var originalProp in originalProps) {                
                var type = originalProp.PropertyType;
                var val = originalProp.GetValue(self); 
                if (val != (type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null)) {
                    sb.Append(SqlFormatter.Format(" {0}: {1}, ", 
                            new LiteralSqlString(originalProp.Name), val)); 
                }          
            }
            sb.Append(" } ");
            return sb.ToString();
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
