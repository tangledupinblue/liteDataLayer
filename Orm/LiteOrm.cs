
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

        public void Insert<T>(T entity) {
            InsertAs<T>(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void InsertAs<T>(T entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptInsert(entity, schemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            if (lts[0].Rows.Count >0) {
                DataObjectExtensions.UpdateValues<T>(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
        }

        public void InsertMany<T>(List<T> entities) {
            ScriptedSchema schemaDef = _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T));
            foreach (var entity in entities) {
                InsertAs<T>(entity, schemaDef);
            }
        }

        public void Load<T>(T entity) {
            LoadAs(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void LoadAs<T>(T entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptLoad(entity, schemaDef);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            Console.WriteLine(lts[0].ToTextResults("|"));
            DataObjectExtensions.UpdateValues<T>(entity, lts[0].Rows[0], lts[0].ColumnNames.ToArray());
        }

        public void Update<T>(T entity) {
            UpdateAs<T>(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void UpdateAs<T>(T entity, ScriptedSchema schemaDef) {
            string sql = scripter.ScriptUpdate(entity, schemaDef);
            dataLink.ExecuteNonQuery(sql);
        }

        public void Delete<T>(T entity)  {
            string sql = scripter.ScriptDelete(entity, _schemaDefs.ContainsKey(typeof(T).FullName)
                                            ? _schemaDefs[typeof(T).FullName]
                                            : new ScriptedSchema(typeof(T)));
        }

        public void DeleteAs<T>(T entity, ScriptedSchema schemaDef) {
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

        public List<T> SelectAll<T>()
        {            
            string sql = scripter.ScriptSelect<T>();
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();
        }

        public List<T> SelectWhere<T>(object selector) {
            string sql = scripter.ScriptSelect<T>(selector);
            LiteTable[] lts = dataLink.GetTabularSets(sql);
            var itms = (from row in lts[0].Rows
                        select DataObjectExtensions.CreateNewType<T>(row, lts[0].ColumnNames.ToArray()));
            return itms.ToList();            
        }

        public T First<T>(string sql) {
            LiteTable[] lts = dataLink.GetTabularSets(sql,30);
            if (lts[0].Rows.Count  > 0)
            {
                return DataObjectExtensions.CreateNewType<T>(lts[0].Rows[0], lts[0].ColumnNames.ToArray());
            }
            return default(T);
        }
    }   
}    

