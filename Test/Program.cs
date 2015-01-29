using DatabasePool;
using System;
using System.Collections.Generic;
using System.Linq;
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
                pool.Test(new ConnectionInfo()
                {
                    ServerType = ConnectionType.MYSQL,
                    Server = "127.0.0.1",
                    Database = "test",
                    Username = "root"
                });
                pool.Create(new ConnectionInfo()
                {
                    Name = ConnectionType.MYSQL,
                    ServerType = ConnectionType.MYSQL,
                    Server = "127.0.0.1",
                    Database = "test",
                    Username = "root"
                });
                //ConnectionType.SQL
                pool.Test(new ConnectionInfo()
                {
                    ServerType = ConnectionType.SQL,
                    Server = "DIMKL-ACER\\SQLEXPRESS",
                    Database = "event_queue",
                    Username = "sa",
                    Password = "sapass"
                });
                //ConnectionType.ORACLE
                //pool.Test(new ConnectionInfo()
                //{
                //    ServerType = ConnectionType.ORACLE,
                //    Server = "Dsn",
                //    Username = "erpsync",
                //    Password = "ERPSYNC"
                //});
                //ConnectionType.ODBC
                pool.Test(new ConnectionInfo()
                {
                    ServerType = ConnectionType.ODBC,
                    Server = "odbc_test",
                    Username = "sa",
                    Password = "sapass"
                });
                //ConnectionType.SQLCE
                //pool.Test(new ConnectionInfo()
                //{
                //    ServerType = ConnectionType.SQLCE,
                //    Database = "Database.sdf"
                //});
                //ConnectionType.SQLITE
                pool.Test(new ConnectionInfo()
                {
                    ServerType = ConnectionType.SQLITE,
                    Database = "test.sqlite3"
                });

                //Test SQL Created Connection --Execute
                var mysqlCon=pool.Checkout(ConnectionType.MYSQL);
                var table = mysqlCon.Execute("Select * from testtable");

                Console.WriteLine("rows:" + table.Rows.Count);
                mysqlCon.isTransaction = true;
                table = mysqlCon.Execute("Select * from testtable");

                Console.WriteLine("rows:" + table.Rows.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
