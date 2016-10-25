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
            SqlDataLinkTest dtst = new SqlDataLinkTest(dataLink);
            dtst.Run();
            SqlScriptingTest stst = new SqlScriptingTest(
                dataLink,
                new SqlScripter());
            stst.Run();
            SqlOrmTest otst = new SqlOrmTest(
                dataLink,
                new SqlScripter());
            otst.Run();
            new ScriptedSchemaTests().Run();
        }
    }
}
