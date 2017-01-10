using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            schema.MakeAutoIDColumn("num1")
                                    .IgnoreColumns("money1", "money2", "money3", "money4")
                                    .MakeReadOnlyColumns("datetime1")
                                    .MakeWriteOnlyColumns("datetime2");

            Console.WriteLine(_sqlScripter.ScriptInsert(testy, schema));

            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("SELECT SCOPE_IDENTITY() AS num1"));
            Debug.Assert(!_sqlScripter.ScriptInsert(testy, schema).Contains("money1"));
            Debug.Assert(!_sqlScripter.ScriptInsert(testy, schema).Contains("datetime1"));
            Debug.Assert(_sqlScripter.ScriptInsert(testy, schema).Contains("datetime2"));

            TestNameMapping();
        }

        private void TestNameMapping() {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Testing changing column and table names");
            BadTesty badTesty = new BadTesty()  {
                badnum1 = -1,
                badnum2 = -1
            };
 
            ScriptedSchema badSchema = new ScriptedSchema(typeof(BadTesty));
            badSchema.ChangeTableNameTo("Testy")
                            .ChangeColumnName("badnum1", "num1")
                            .ChangeColumnName("badnum2", "num2");

            StringBuilder badBuilder = new StringBuilder();
            badBuilder.AppendLine(_sqlScripter.ScriptInsert(badTesty, badSchema));
            Console.WriteLine(badBuilder.ToString());
            Debug.Assert(_sqlScripter.ScriptLoad(badTesty, badSchema).Contains("AS bad"));
            Console.WriteLine(_sqlScripter.ScriptLoad(badTesty, badSchema));
            badBuilder.AppendLine(_sqlScripter.ScriptUpdate(badTesty, badSchema));
            Console.WriteLine(badBuilder.ToString());
            badBuilder.AppendLine(_sqlScripter.ScriptDelete(badTesty, badSchema));
            Console.WriteLine(badBuilder.ToString());
            Debug.Assert(!badBuilder.ToString().Contains("bad"));
            badBuilder.Clear();
            badBuilder.AppendLine(_sqlScripter.ScriptSelect(badSchema));
            Console.WriteLine(badBuilder.ToString());
            Debug.Assert(badBuilder.ToString().Contains("AS bad"));
            badBuilder.Clear();
            badBuilder.AppendLine(_sqlScripter.ScriptSelect(new { badnum1 = -1 }, badSchema));
            Console.WriteLine(badBuilder.ToString());
            Debug.Assert(badBuilder.ToString().Contains("AS bad"));
        }
    }
}
