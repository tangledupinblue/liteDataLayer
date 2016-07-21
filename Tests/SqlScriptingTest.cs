using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;
using LiteDataLayer.Formatting;


namespace LiteDataLayer.Tests
{
    public class SqlScriptingTest  
    {
        private IDataLink dataLink = null;
        private SqlScripter scripter = null;

        public SqlScriptingTest (IDataLink dataLink, SqlScripter scripter){
            this.dataLink = dataLink;
            this.scripter = scripter; 
        }

        public void Run() {
            var testies = TestyFactory.GiveMe(1);
            //normal crud ops
            foreach (var testy in testies) {
                DebugScript(testy, "");
                DebugScript(testy, " [ num1 k ] ");
                DebugScript(testy, " [ num1 k a ] ");
                DebugScript(testy, " o [ num1 k ] ");
                DebugScript(testy, " Testy [ num1 k ] ");
                DebugScript(testy, " Testy o [ num1 k, guid1 k ] ");                
                Debug.Assert(((Action)(() => DebugScript(testy, " Testy o [ int1 k ] "))).ExceptionThrown());                
            }
        }

        private void DebugScript(Testy testy, string directive) {
            Console.WriteLine();
            Console.WriteLine(directive);
            scripter.DebugScript(testy, "");
            string sql = scripter.ScriptInsert(testy, directive);
            Console.WriteLine(sql);
            sql = scripter.ScriptLoad(testy, directive);
            Console.WriteLine(sql);
            sql = scripter.ScriptUpdate(testy, directive);
            Console.WriteLine(sql);
            sql = scripter.ScriptDelete(testy, directive);
            Console.WriteLine(sql);
        }
    }
}


