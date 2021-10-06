using System;
using System.Collections.Generic;
using LiteDataLayer.Formatting;


namespace LiteDataLayer.Tests
{
    public class SqlFormattingTests 
    {
        private static void GenerateLine(string typeName, object value)
        {
            var sqlValue= SqlFormatter.GetSqlString(value);
            Console.WriteLine($"{typeName} ({value.GetType().ToString()}) :{sqlValue}");
        }

        public static void DoFormatting()
        {
            GenerateLine("float", (float)1.01);            
            GenerateLine("float", (double)1.01);            
            GenerateLine("float", (decimal)1.01);            
            GenerateLine("float", (float)1.01);            



        }
    }

}