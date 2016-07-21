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
            IDataLink dataLink = new SqlDataLink("Server=192.168.0.17;Database=tempdb;User ID=devserver;Password=devserver;");
            SqlDataLinkTest dtst = new SqlDataLinkTest(dataLink);
            //dtst.Run();
            SqlScriptingTest stst = new SqlScriptingTest(
                dataLink,
                new SqlScripter());
            //stst.Run();
            SqlOrmTest otst = new SqlOrmTest(
                dataLink,
                new SqlScripter());
            otst.Run();
        
        }
    }
}
