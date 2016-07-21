using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

using LiteDataLayer.DataStructures;

namespace LiteDataLayer.Dal
{
    //interface for the DataAccess class
    //this allows extensions such as a web service implementation
    //NOTE - do not implement extensions / variations in this project 
    public interface IDataLink
    {
        string DataSource { get; }
        string Database { get; }
        string ConnectionType { get; }

        int ExecuteNonQuery(string cmd, int commandTimeOut = 30);
        object ExecuteScalar(string cmd, int commandTimeOut = 30);
        T ExecuteScalar<T>(string cmd, int commandTimeout = 30);

        LiteTable[] GetTabularSets(string cmd, int commandTimeOut = 30);
    }
}
