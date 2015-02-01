using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlServerCe;

namespace DatabasePool
{
    public class ConnectionPool : Pool<DbConnection>
    {
        protected ConnectionPool() { }

        private static ConnectionPool instance;

        public static ConnectionPool getInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionPool();
            }
            return instance;
        }

        public static List<string> GetAllDatabases(ConnectionData con)
        {
            var databases = new List<string>();

            if (con.ServerType == ConnectionType.MYSQL)
            {

            }
            else if (con.ServerType == ConnectionType.SQL)
            {

            }
            else if (con.ServerType == ConnectionType.ORACLE)
            {

            }
            else if (con.ServerType == ConnectionType.SQLCE)
            {

            }
            else if (con.ServerType == ConnectionType.SQLITE)
            {

            }
            return databases;
        }

        public static List<string> GetAllServers(ConnectionData con)
        {
            var servers = new List<string>();

            if (con.ServerType == ConnectionType.MYSQL)
            {

            }
            else if (con.ServerType == ConnectionType.SQL)
            {

            }
            else if (con.ServerType == ConnectionType.ORACLE)
            {

            }
            else if (con.ServerType == ConnectionType.SQLCE)
            {

            }
            else if (con.ServerType == ConnectionType.SQLITE)
            {

            }
            return servers;
        }

        public void Create(ConnectionData con, string name = "")
        {
            DbConnection _con = new DbConnection();
            ConnectionMapping mapping = new ConnectionMapping() { Data = con };

            if (con.ServerType == ConnectionType.MYSQL)
            {
                mapping.Server = "Server";
                mapping.Database = "Database";
                mapping.Username = "Uid";
                mapping.Password = "Pwd";

                _con.Connection = new MySqlConnection(mapping.ToString());
            }
            else if (con.ServerType == ConnectionType.SQL)
            {
                mapping.Server = "Server";
                mapping.Database = "Database";
                mapping.Username = "User Id";
                mapping.Password = "Password";

                _con.Connection = new SqlConnection(mapping.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLCE)
            {
                mapping.Database = "Data Source";
                mapping.Password = "Password";
                mapping.Custom = "Encrypt Database = True;";

                _con.Connection = new SqlCeConnection(mapping.ToString());
            }
            else if (con.ServerType == ConnectionType.ORACLE)
            {
                mapping.Server = "Server";
                mapping.Database = "Data Source";
                mapping.Username = "User Id";
                mapping.Password = "Password";
                mapping.IntergratedSecurity = "Integrated Security";
                //
                _con.Connection = new OracleConnection(mapping.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLITE)
            {
                mapping.Database = "Data Source";
                mapping.Password = "Password";
                mapping.Custom = "Version=3;";

                _con.Connection = new SQLiteConnection(mapping.ToString());
            }
            else if (con.ServerType == ConnectionType.ODBC)
            {
                mapping.Server = "Dsn";
                mapping.Username = "Uid";
                mapping.Password = "Pwd";

                _con.Connection = new OdbcConnection(mapping.ToString());
            }

            name = name != String.Empty ? name : con.Name;
            base.Create(name, _con);
        }

        public void Test(ConnectionData con)
        {
            //create connection
            Create(con, "Test");
            using (IDbConnection _con = Checkout("Test").Connection)
            {
                _con.Open();
            }
            //remove connection
            Container.Remove("Test");
        }

    }

}
