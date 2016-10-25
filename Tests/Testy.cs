
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteDataLayer.Formatting;

namespace LiteDataLayer.Tests
{
	public class Testy
	{	
		public    int num1 {get; set; }	
		public    int? num2 {get; set; }
		public    string val1 {get; set; }
		public    string val2 {get; set; }
		public    Guid guid1 {get; set; }
		public    Guid? guid2 {get; set; }
		public    decimal money1 {get; set; }
		public    decimal? money2 {get; set; }
		public    double money3 {get; set; }
		public    double? money4 {get; set; }
		public    decimal float1 { get; set; }
		public    decimal? float2 {get; set; }
		public    double float3 { get; set; }
		public    double? float4 {get; set; }
		public    DateTime datetime1 {get; set; }
		public    DateTime? datetime2 {get; set; }

        public override string ToString() {
            return string.Format("Testy: {0}",
                string.Join(";", this.GetType().GetProperties()
                            .Select(p => p.Name + "=" + Convert.ToString(p.GetValue(this) ?? "null"))
                            .ToArray()));
        }
    }

	public static class TestyFactory
	{
		public static List<Testy> GiveMe(int howMany) {
			var testies = new List<Testy>();
            DateTime start = DateTime.Now;

            for (int i = 0; i < howMany; i++) {
                Testy testy = new Testy() {
                    num1 = i,
                    num2 = null,
                    val1 = string.Format("number{0}", i),
                    val2 = null,
                    guid1 = Guid.NewGuid(),
                    guid2 = null,
                    money1 = Convert.ToDecimal(string.Format("{0:0.00}",i)),
                    money2 = null,
                    money3 = Convert.ToDouble(string.Format("{0:0.00}",i)),
                    money4 = null,
                    float1 = Convert.ToDecimal(string.Format("{0:0.00}",i)),
                    float2 = null,
                    float3 = Convert.ToDouble(string.Format("{0:0.00}",i)),
                    float4 = null,
                    datetime1 = DateTime.Now,
                    datetime2 = null
                };
				testies.Add(testy);
            }
			return testies;
		}

		internal static string GetTestyTableSql() {
            return @"
IF OBJECT_ID('Testy', 'U') IS NOT NULL 
  DROP TABLE Testy; 

CREATE TABLE Testy (
    num1                        int,
    num2                        int,    
    val1                        varchar(50),
    val2                        varchar(50),
    guid1                       uniqueidentifier,
    guid2                       uniqueidentifier,
    money1                      money,
    money2                      money,
    money3                      money,
    money4                      money,
    float1                      float,
    float2                      float,
    float3                      float,
    float4                      float,
    datetime1                   datetime,
    datetime2                   datetime
)";
		}

        		internal static string GetTestyTableSqlIntIdentity() {
            return @"
IF OBJECT_ID('Testy', 'U') IS NOT NULL 
  DROP TABLE Testy; 

CREATE TABLE Testy (
    num1                        int IDENTITY(1,1),
    num2                        int,    
    val1                        varchar(50),
    val2                        varchar(50),
    guid1                       uniqueidentifier,
    guid2                       uniqueidentifier,
    money1                      money,
    money2                      money,
    money3                      money,
    money4                      money,
    float1                      float,
    float2                      float,
    float3                      float,
    float4                      float,
    datetime1                   datetime,
    datetime2                   datetime
)
        ";}
	}
}
