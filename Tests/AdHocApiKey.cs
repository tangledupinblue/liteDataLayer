using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;
using LiteDataLayer.Orm;

namespace LiteDataLayer.Tests
{
    public class ApiKey
    {
        public Guid ApiKeyGuid { get; set; }
        public string Password { get; set; }
        // public bool Enabled { get; set; }

        public override string ToString() {
            return ApiKeyGuid + ":" + Password;
        }
    }

    public class AdHocApiKey
    {
        private LiteOrm orm = null;
        private IDataLink dataLink;
        private SqlScripter _scripter;

        public AdHocApiKey (IDataLink dataLink, SqlScripter scripter){
            this.dataLink = dataLink;
            this.orm = new LiteOrm(dataLink, scripter); 
            orm.SetSchema(typeof(ApiKey), new ScriptedSchema(typeof(ApiKey))
                            .MakeKeyColumns("ApiKeyGuid", "Pasword"));
            _scripter = scripter;
        }

        public void Run() {
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("Testing ORM");

            Console.WriteLine("Testing Select Where");
            Console.WriteLine("-------------------");

            // var selector = new { ApiKeyGuid = Guid.NewGuid().ToString(), Password = "bob" };
            dynamic selector = new ExpandoObject();
            selector.ApiKeyGuid = Guid.NewGuid();
            selector.Pasword = "bob";

            Console.WriteLine(_scripter.ScriptSelect(orm.GetSchema(typeof(ApiKey))));
            Console.WriteLine(_scripter.ScriptSelect(selector, orm.GetSchema(typeof(ApiKey))));

            Console.WriteLine(_scripter.ScriptSelect(orm.GetSchema(typeof(ApiKey))));

            var results = orm.SelectWhere<ApiKey>(selector);
            Console.WriteLine(results.GetType());
            Console.WriteLine(results.ToArray().Length);
            
            Console.WriteLine(string.Format("Found {0} records",
                        orm.SelectWhere<ApiKey>(selector).ToArray().Length));
            Console.WriteLine(string.Format("Found {0} records",
                        orm.SelectWhere(selector, typeof(ApiKey)).ToArray().Length));

        }
    }
}
