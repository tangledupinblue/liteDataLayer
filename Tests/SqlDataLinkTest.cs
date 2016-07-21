using System;
using System.Collections.Generic;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;


namespace LiteDataLayer.Tests
{
    public class SqlDataLinkTest  
    {
        private IDataLink dataLink = null;

        public SqlDataLinkTest (IDataLink dataLink){
            this.dataLink = dataLink; 
        }

        public void Run() {
            string qry = TestyFactory.GetTestyTableSql();
            Console.WriteLine(qry);
            dataLink.ExecuteNonQuery(qry, 30);
            for (int i = 0; i < 10; i++) {
                qry = string.Format("INSERT Testy (num1, val1) VALUES ({0},'{0}')", i);
                Console.WriteLine(qry);
                dataLink.ExecuteNonQuery(qry,30);
            }
            for (int i = 0; i < 10; i++) {
                qry = string.Format("SELECT val1 FROM Testy WHERE num1 = {0}", i);
                Console.WriteLine(qry);
                dataLink.ExecuteScalar<string>(qry,30);
            }
            qry = "SELECT * FROM Testy";
            Console.WriteLine(qry);
            LiteTable[] tables = dataLink.GetTabularSets(qry,30);
            foreach (var table in tables) {
                Console.WriteLine(table.ToTextResults("|"));                
            }
        }
    }
}


