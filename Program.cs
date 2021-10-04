using System;
using LiteDataLayer.Dal;
using LiteDataLayer.Tests;
using LiteDataLayer.Formatting;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            IDataLink dataLink = new SqlDataLink(args[0]);

            new ConnectionTester(dataLink).Run();

            new AdHocApiKey(dataLink, new SqlScripter()).Run();

            new SqlDataLinkTest(dataLink).Run();

            new SqlScriptingTest(dataLink, new SqlScripter()).Run();
            
            new SqlOrmTest(dataLink, new SqlScripter()).Run();
            
            new ScriptedSchemaTests().Run();
        }
    }
}
