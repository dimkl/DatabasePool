using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace DatabasePool
{
    static class ConnectionType
    {
        public const string MYSQL = "Mysql";
        public const string SQL = "SQL Server";
        public const string SQLCE = "SQL Server Compact";
        public const string ORACLE = "Oracle";
        public const string SQLITE = "Sqlite";
        public const string ODBC = "Odbc";

        public static List<string> GetAll()
        {
            return new List<string> { 
            MYSQL,SQL,SQLCE,ORACLE,SQLITE,ODBC
            };
        }
    }

    public class ConnectionInfo
    {
        public string Name;
        public string ServerType;

        public string Server;
        public string Database;
        public string Username;
        public string Password;
        public string IntergratedSecurity;

        public string ServerMapping;
        public string DatabaseMapping;
        public string UsernameMapping;
        public string PasswordMapping;
        public string IntergratedSecurityMapping;

        public string Custom;
        //public string ...
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (ServerType != null && ServerMapping != null)
            {
                builder.AppendFormat("{0}={1};", ServerMapping, Server);
            }
            if (Database != null && DatabaseMapping != null)
            {
                builder.AppendFormat("{0}={1};", DatabaseMapping, Database);
            }
            if (Username != null && UsernameMapping != null)
            {
                builder.AppendFormat("{0}={1};", UsernameMapping, Username);
            }
            if (Password != null && PasswordMapping != null)
            {
                builder.AppendFormat("{0}={1};", PasswordMapping, Password);
            }
            if (IntergratedSecurity != null && IntergratedSecurityMapping != null)
            {
                builder.AppendFormat("{0}={1};", IntergratedSecurityMapping, IntergratedSecurity);
            }
            if (Custom != null)
            {
                builder.AppendFormat(Custom);
            }

            return builder.ToString();
        }
    }

    public class DbConnection
    {
        public IDbConnection Connection;
        public bool isProcedure;
        public bool isTransaction;

        public DataTable Execute(string sql, Dictionary<string, string> parameters = null)
        {
            IDbTransaction transaction = null;
            IDbCommand command = Connection.CreateCommand();
            command.CommandText = sql;

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            if (isProcedure)
            {
                command.CommandType = CommandType.StoredProcedure;
            }

            if (isTransaction)
            {
                transaction = Connection.BeginTransaction();
                command.Transaction = transaction;
            }
            //add parameters
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value;

                command.Parameters.Add(parameter);
            }
            //execute query and fill table
            var table = new DataTable();
            try
            {
                //
                if (Connection is SqlConnection)
                {
                    table = ExecuteAdapter<SqlDataAdapter>(command as SqlCommand);
                }
                else if (Connection is MySqlConnection)
                {
                    table = ExecuteAdapter<MySqlDataAdapter>(command as MySqlCommand);
                }
                //else if (Connection is SQLCEconnection)
                //{
                //    return ExecuteAdapter< SQLCEDataAdapter>(command as  SQLCECommand);
                //}
                else if (Connection is OracleConnection)
                {
                    table = ExecuteAdapter<OracleDataAdapter>(command as OracleCommand);
                }
                else if (Connection is SQLiteConnection)
                {
                    table = ExecuteAdapter<SQLiteDataAdapter>(command as SQLiteCommand);
                }
                else if (Connection is OdbcConnection)
                {
                    table = ExecuteAdapter<OdbcDataAdapter>(command as OdbcCommand);
                }

                if (isTransaction)
                {
                    transaction.Commit();
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return table;
        }

        private DataTable ExecuteAdapter<T>(IDbCommand command)
            where T : IDbDataAdapter, new()
        {
            var table = new DataTable();
            string sql = command.CommandText;
            //add command to adapter
            var _adapter = new T();

            if (sql.ToLower().StartsWith("update"))
            {
                _adapter.UpdateCommand = command;
            }
            else if (sql.ToLower().StartsWith("insert"))
            {
                _adapter.InsertCommand = command;
            }
            else if (sql.ToLower().StartsWith("delete"))
            {
                _adapter.DeleteCommand = command;
            }
            else
            {
                _adapter.SelectCommand = command;
            }
            //fill adapter table
            _adapter.Fill(table.DataSet);

            return table;
        }
    }

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

        public static List<string> GetAllDatabases(ConnectionInfo con)
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

        public static List<string> GetAllServers(ConnectionInfo con)
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

        public void Create(ConnectionInfo con, string name = "")
        {
            DbConnection _con = null;
            if (con.ServerType == ConnectionType.MYSQL)
            {
                //παρε τις μη κενες τιμές του ConnectionInfo και βάλ' τες με βαση τις παραμτερους για Mysql 
                con.ServerMapping = "Server";
                con.DatabaseMapping = "Database";
                con.UsernameMapping = "Uid";
                con.PasswordMapping = "Pwd";
                //
                _con.Connection = new MySqlConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQL)
            {
                //παρε τις μη κενες τιμές του ConnectionInfo και βάλ' τες με βαση τις παραμτερους για SQL 
                con.ServerMapping = "Server";
                con.DatabaseMapping = "Database";
                con.UsernameMapping = "User Id";
                con.PasswordMapping = "Password";
                //
                _con.Connection = new SqlConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLCE)
            {
                con.DatabaseMapping = "Data Source";
                con.PasswordMapping = "Password";
                con.Custom = "Encrypt Database = True;";

                //_con.Connection = new SqlCeConnection();
            }
            else if (con.ServerType == ConnectionType.ORACLE)
            {
                //παρε τις μη κενες τιμές του ConnectionInfo και βάλ' τες με βαση τις παραμτερους για Oracle 
                con.ServerMapping = "Server";
                con.DatabaseMapping = "Data Source";
                con.UsernameMapping = "User Id";
                con.PasswordMapping = "Password";
                con.IntergratedSecurityMapping = "Integrated Security";
                //
                _con.Connection = new OracleConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLITE)
            {
                con.DatabaseMapping = "Data Source";
                con.PasswordMapping = "Password";
                con.Custom = "Version=3;";

                _con.Connection = new SQLiteConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.ODBC)
            {
                //παρε τις μη κενες τιμές του ConnectionInfo και βάλ' τες με βαση τις παραμτερους για Odbc 
                con.ServerMapping = "Dsn";
                con.UsernameMapping = "Uid";
                con.PasswordMapping = "Pwd";
                //
                _con.Connection = new OdbcConnection(con.ToString());
            }

            name = name != String.Empty ? name : con.Name;
            base.Create(name, _con);
        }

        public void Test(ConnectionInfo con)
        {
            //create connection
            Create(con, "Test");
            //get connection
            using (IDbConnection _con = Checkout("Test").Connection)
            {
                _con.Open();
            }
            //remove connection
            Container.Remove(con.Name);
        }

    }

}
