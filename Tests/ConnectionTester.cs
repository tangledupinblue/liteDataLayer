using System;
using System.Collections.Generic;
using LiteDataLayer.Dal;
using LiteDataLayer.DataStructures;


namespace LiteDataLayer.Tests
{
    public class ConnectionTester  
    {
        private IDataLink dataLink = null;

        public ConnectionTester (IDataLink dataLink){
            this.dataLink = dataLink; 
        }

        public void Run() {
            Console.WriteLine("Testing Connection String");
            Console.WriteLine("-------------------------");
            string qry = "SELECT GETDATE()";
            Console.WriteLine(dataLink.ExecuteScalar(qry, 30));
        }
    }
}


