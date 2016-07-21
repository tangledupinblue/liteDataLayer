using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;
using LiteDataLayer.Orm;

namespace LiteDataLayer.Tests
{
    public class SqlOrmTest  
    {
        private LiteOrm orm = null;
        private IDataLink dataLink;

        public SqlOrmTest (IDataLink dataLink, SqlScripter scripter){
            this.dataLink = dataLink;
            this.orm = new LiteOrm(dataLink, scripter); 
        }
        public void Run() {

            Console.WriteLine("Defaults - no specifications");
            Testy testy = TestyFactory.GiveMe(1)[0];
            TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSql()));
            Console.WriteLine("Identity Insert int");
            testy = TestyFactory.GiveMe(1)[0];
            testy.SchemaDef = "{ [ { num1 k a } ] }";
            Console.WriteLine("Multiple Keys");
            testy = TestyFactory.GiveMe(1)[0];
            testy.SchemaDef = "{ [ { num1 k a }, { guid1 k } ] }";            
            TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSqlIntIdentity()));
            Console.WriteLine("Invalid Directive on Entity");
            testy = TestyFactory.GiveMe(1)[0];
            testy.SchemaDef = "{ [ { int1 k a } ] }";
            Debug.Assert(((Action)(() =>  {
                    TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSqlIntIdentity()));
                    }))
                    .ExceptionThrown());
            //test selects...
            Console.WriteLine("Testing Select");
            Console.WriteLine("-------------------");
            Console.WriteLine(string.Format("Found {0} records", orm.SelectAll<Testy>().Count()));
            Console.WriteLine("Testing Select Where");
            Console.WriteLine("-------------------");
            var selector = new { num1 = 1 };
            Console.WriteLine(selector.GetType());
            Console.WriteLine(string.Format("Found {0} records",
                        orm.SelectWhere<Testy>(selector).Count()));
        }

        public void TestCrud(Testy testy, Action CreateTable) {
            CreateTable.Invoke();
            Console.WriteLine("Insert");            
            orm.Insert<Testy>(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Load");            
            orm.Load<Testy>(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Update");            
            orm.Update<Testy>(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Delete");            
            orm.Delete<Testy>(testy);
            Console.WriteLine(testy);
        }
    }
}


