
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;


namespace LiteDataLayer.Orm
{
        
    //Scripting Shorthand... This is the shorthand for scripting objects....
    //set the schema for a type using SetSchema
        //eg. ' TableName o [ ColumnName k a u l=50 r/w i, ColumnName k ] } '
        //default tablename - classname
        //o - use only specified cols
        //k - key
        //a - auto id / identity insert
        //u - is unique index that can be looked up
        //l=50 - length
        //r/w - r:read only, w:write only
        //i - ignore completely

    public class LiteOrm
    {
        private IDataLink dataLink;
        private SqlScripter scripter; 

        // <typename, schemaDef>
        private SortedList<string, ScriptedSchema> _schemaDefs = new SortedList<string, ScriptedSchema>();

        public LiteOrm(IDataLink dataLink, SqlScripter scripter)
        {
            this.dataLink = dataLink;
            this.scripter = scripter;
        }

        public LiteOrm SetSchema(Type type, string schemaDef) {
            if (_schemaDefs.Keys.Contains(type.FullName)) {
                _schemaDefs[type.FullName] = new ScriptedSchema(type,schemaDef);
            } else {
                _schemaDefs.Add(type.FullName, new ScriptedSchema(type,schemaDef));
            }
            return this;
        }

        public LiteOrm SetSchema(Type type, ScriptedSchema schema) {
            if (_schemaDefs.Keys.Contains(type.FullName)) {
                _schemaDefs[type.FullName] = schema;
            } else {
                _schemaDefs.Add(type.FullName, schema);
            }
            return this;
        }
        
        public LiteOrm ClearSchema(Type type) {
            if (_schemaDefs.Keys.Contains(type.FullName)) {
                _schemaDefs.Remove(type.FullName);
            }
            return this;            
        }

        public List<dynamic> SelectToDynamic(string sql)
        {
            LiteTable[] lts = dataLink.GetTabularSets(sql,30);
            return lts[0].ToDynamic();
        }

        public void Insert(object entity) {
            Type type = entity.GetType();
            InsertAs(entity, _schemaDefs.ContainsKey(type.FullName)
                                            ? _schemaDefs[type.FullName]
                                            : new ScriptedSchema(type));            
        }

        public void Insert<T>(T entity) {
            InsertAs(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void InsertAs(object entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptInsert(entity, schemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            if (lts[0].Rows.Count >0) {
                DataObjectExtensions.UpdateValues(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
        }

        public void InsertMany<T>(List<T> entities) {
            ScriptedSchema schemaDef = _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T));
            foreach (var entity in entities) {
                InsertAs(entity, schemaDef);
            }
        }

        public T Load<T>(object selector) {
            return Load(DataObjectExtensions.CopyValues<T>(selector));
        }

        public T Load<T>(T entity) {
            return (T)LoadAs(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));                        
        }

        public object Load(object entity) {
            Type type = entity.GetType();
            return LoadAs(entity, _schemaDefs.ContainsKey(type.FullName)
                                            ? _schemaDefs[type.FullName]
                                            : new ScriptedSchema(type));
        }

        public object LoadAs(object entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptLoad(entity, schemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            Console.WriteLine(lts[0].ToTextResults("|"));
            if (lts[0].Rows.Count == 0) { 
                new LiteOrmException("Cannot load selected object " + entity.StringifyValues());
            }
            DataObjectExtensions.UpdateValues(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            return entity;
        }

        public void Update<T>(T entity) {
            UpdateAs(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void Update(object entity) {
            var type = entity.GetType();
            UpdateAs(entity,  _schemaDefs.ContainsKey(type.FullName)
                                            ? _schemaDefs[type.FullName]
                                            : new ScriptedSchema(type));
        }

        public void UpdateAs(object entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptUpdate(entity, schemaDef);
            dataLink.ExecuteNonQuery(sql);
        }

        public void Delete<T>(T entity)  {
            DeleteAs(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                    ? _schemaDefs[typeof(T).FullName]
                    : new ScriptedSchema(typeof(T)));
        }

        public void Delete(object entity) {
            var type = entity.GetType();
            DeleteAs(entity, _schemaDefs.ContainsKey(type.FullName)
                    ? _schemaDefs[type.FullName]
                    : new ScriptedSchema(type));
        }

        public void DeleteAs(object entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptDelete(entity, schemaDef);
            dataLink.ExecuteNonQuery(sql);
        }

        public List<T> Select<T>(string sql)
        {
            LiteTable[] lts = dataLink.GetTabularSets(sql, 30);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();
        }

        public List<object> Select(string sql, Type type) {
            LiteTable[] lts = dataLink.GetTabularSets(sql, 30);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType(row, lts[0].ColumnNames.ToArray(), type));
            return itms.ToList();            
        }

        public List<T> Select<T>()
        {            
            string sql = scripter.ScriptSelect<T>();
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();
        }

        public List<object> Select(Type type)
        {            
            string sql = scripter.ScriptSelect(new ScriptedSchema(type));
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType(row, lts[0].ColumnNames.ToArray(), type));
            return itms.ToList();
        }
        
        //TODO - doesn't handle schema def!!
        public List<T> SelectWhere<T>(object selector) {
            string sql = scripter.ScriptSelect<T>(selector);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();            
        }

        public T FirstOrDefault<T>(string sql) {
            LiteTable[] lts = dataLink.GetTabularSets(sql,30);
            if (lts[0].Rows.Count  > 0)
            {
                return DataObjectExtensions.CreateNewType<T>(lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
            return default(T);
        }
    }   
}    

