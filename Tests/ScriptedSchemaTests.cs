using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;


namespace LiteDataLayer.Tests
{
    public class ScriptedSchemaTests
    {
        private SqlScripter _sqlScripter = new SqlScripter();

        public ScriptedSchemaTests() {
        }    

        public void Run() {
            Console.WriteLine("Scripted Schema Tests");
            Console.WriteLine("------------------------------");

            Console.WriteLine("Default");        
            ScriptedSchema schema = new ScriptedSchema(typeof(Testy));
            Testy testy = TestyFactory.GiveMe(1)[0];
            Console.WriteLine(_sqlScripter.ScriptInsert(testy, schema));
            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("INSERT Testy"));

            Console.WriteLine("Change Table Name");        
            schema.ChangeTableNameTo("SomethingElse");
            Console.WriteLine(_sqlScripter.ScriptInsert(testy, schema));
            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("INSERT SomethingElse"));

            Console.WriteLine("AutoID column and ignore columns");        
            schema.MakeAutoIDColumn("num1").IgnoreColumns("money1", "money2", "money3", "money4")
                                    .MakeReadOnlyColumns("datetime1")
                                    .MakeWriteOnlyColumns("datetime2");

            Console.WriteLine(_sqlScripter.ScriptInsert(testy, schema));

            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("SELECT SCOPE_IDENTITY() AS num1"));
            Debug.Assert(!_sqlScripter.ScriptInsert(testy, schema).Contains("money1"));
            Debug.Assert(!_sqlScripter.ScriptInsert(testy, schema).Contains("datetime1"));
            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("datetime2"));
        }
    }
}
