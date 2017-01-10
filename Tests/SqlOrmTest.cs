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

            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("Testing ORM");

            Console.WriteLine("Defaults - no specifications");
            Testy testy = TestyFactory.GiveMe(1)[0];
            TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSql()));

            Console.WriteLine("Identity Insert int");
            testy = TestyFactory.GiveMe(1)[0];
            orm.SetSchema(testy.GetType(), " [ num1 k a ] "); 
            TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSqlIntIdentity()));

            Console.WriteLine("Multiple Keys");
            testy = TestyFactory.GiveMe(1)[0];
            orm.SetSchema(testy.GetType(), " [ num1 k a , guid1 k ] ");
            TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSqlIntIdentity()));

            Console.WriteLine("Test Load with key selector");
            testy = TestyFactory.GiveMe(2).Last();
            orm.SetSchema(typeof(Testy), 
                        new ScriptedSchema(typeof(Testy)).MakeKeyColumns(new string[] { "num1" })
                                    .MakeAutoIDColumn("num1"));
            orm.Insert(testy);            
            testy = orm.Load<Testy>(new { num1 = testy.num1 });            
            Debug.Assert(testy != null, "Expecting testy to load");

            Console.WriteLine("Invalid Directive on Entity");
            testy = TestyFactory.GiveMe(1)[0];
            orm.SetSchema(testy.GetType(), " [ wrongCol1 k a ] ");            
            Debug.Assert(((Action)(() =>  {
                    TestCrud(testy, () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSqlIntIdentity()));
                    }))
                    .ExceptionThrown());
            
            //test selects...
            Console.WriteLine("Multiple Inserts");
            Console.WriteLine("-------------------");
            orm.ClearSchema(typeof(Testy));
            dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSql());
            var testies = TestyFactory.GiveMe(10);
            orm.InsertMany(testies.ToList());

            Console.WriteLine("Testing Select");
            Console.WriteLine("-------------------");
            Console.WriteLine(string.Format("Found {0} records", orm.Select<Testy>().Count()));
            
            Console.WriteLine("Testing Select Where");
            Console.WriteLine("-------------------");
            var selector = new { num1 = 0 };
            Console.WriteLine(selector.GetType());
            Console.WriteLine(string.Format("Found {0} records",
                        orm.SelectWhere<Testy>(selector).Count()));

            //check schema applies....
            Console.WriteLine("Checking Custom Schemas on the ORM");
            Console.WriteLine("----------------------------------");
            orm.SetSchema(typeof(BadTesty), 
                    new ScriptedSchema(typeof(BadTesty))
                            .ChangeTableNameTo("Testy")
                            .ChangeColumnName("badnum1", "num1")
                            .ChangeColumnName("badnum2", "num2")); 

            Console.WriteLine(orm.GetSchema(typeof(BadTesty)));

            TestCrud(new BadTesty { badnum1 = 1,  badnum2 = -1 }, 
                    () => dataLink.ExecuteNonQuery(TestyFactory.GetTestyTableSql()));
        }

        public void TestCrud(Testy testy, Action CreateTable) {
            CreateTable.Invoke();
            Console.WriteLine("-- Generics");
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

            Console.WriteLine("-- Objects");            
            Console.WriteLine("Insert");            
            orm.Insert(testy);   
            Console.WriteLine(testy);
            Console.WriteLine("Load");            
            orm.Load(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Update");            
            orm.Update(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Delete");            
            orm.Delete(testy);
            Console.WriteLine(testy);
            
        }

        public void TestCrud(BadTesty testy, Action CreateTable) {
            CreateTable.Invoke();
            Console.WriteLine("-- Generics");
            Console.WriteLine("Insert");            
            orm.Insert<BadTesty>(testy);   
            Console.WriteLine(testy);
            Console.WriteLine("Load");            
            orm.Load<BadTesty>(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Update");            
            orm.Update<BadTesty>(testy);
            Console.WriteLine(testy);
            Console.WriteLine(orm.Select<BadTesty>());
            Console.WriteLine("Delete");            
            orm.Delete<BadTesty>(testy);

            Console.WriteLine("-- Objects");            
            Console.WriteLine("Insert");            
            orm.Insert(testy);   
            Console.WriteLine(testy);
            Console.WriteLine("Load");            
            orm.Load(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Update");            
            orm.Update(testy);
            Console.WriteLine(testy);
            Console.WriteLine("Delete");            
            Console.WriteLine(orm.Select(typeof(BadTesty)));
            orm.Delete(testy);
            Console.WriteLine(testy);
            
        }
    }
}


