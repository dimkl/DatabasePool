using DatabasePool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var pool = ConnectionPool.getInstance();
            try
            {
                //ConnectionType.MYSQL
                var info = new ConnectionInfoData()
                {
                    ServerType = ConnectionType.MYSQL,
                    Server = "127.0.0.1",
                    Database = "test",
                    Username = "root"
                };
                pool.Test(info);
                info.Name = ConnectionType.MYSQL;
                pool.Create(info);
                //ConnectionType.SQL
                info = new ConnectionInfoData()
                {
                    ServerType = ConnectionType.SQL,
                    Server = "DIMKL-ACER\\SQLEXPRESS",
                    Database = "event_queue",
                    Username = "sa",
                    Password = "sapass"
                };
                pool.Test(info);
                info.Name = ConnectionType.SQL;
                pool.Create(info);
                //ConnectionType.ORACLE
                //pool.Test(new ConnectionInfoData()
                //{
                //    ServerType = ConnectionType.ORACLE,
                //    Server = "Dsn",
                //    Username = "erpsync",
                //    Password = "ERPSYNC"
                //});
                //ConnectionType.ODBC
                info = new ConnectionInfoData()
                {
                    ServerType = ConnectionType.ODBC,
                    Server = "odbc_test",
                    Username = "sa",
                    Password = "sapass"
                };
                pool.Test(info);
                //ConnectionType.SQLCE
                //pool.Test(new ConnectionInfoData()
                //{
                //    ServerType = ConnectionType.SQLCE,
                //    Database = "Database.sdf"
                //});
                //ConnectionType.SQLITE
                info = new ConnectionInfoData()
                {
                    ServerType = ConnectionType.SQLITE,
                    Database = "test.sqlite3"
                };
                pool.Test(info);

                //Test SQL Created Connection --Execute
                var mysqlCon = pool.Checkout(ConnectionType.MYSQL);
                var table = mysqlCon.Execute("Select * from testtable");

                Console.WriteLine("rows:" + table.Rows.Count);
                mysqlCon.IsTransaction = true;
                table = mysqlCon.Execute("Select * from testtable");

                Console.WriteLine("rows:" + table.Rows.Count);

                //exequte transact-sql file SQL Server
                var con = pool.Checkout(ConnectionType.SQL);
                con.IsBatch = true;
                //
                string sql = ReadSQLFile();
                con.IsTransaction = true;
                con.Execute(sql);
                var errors = con.GetErrors();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ReadSQLFile()
        {
            string sqlfilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "test_sql.sql";

            try
            {
                return File.ReadAllText(sqlfilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
    }
}
