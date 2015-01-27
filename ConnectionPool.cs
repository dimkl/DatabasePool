using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DatabasePool
{
    static class ConnectionType
    {
        public const string MYSQL = "Mysql Server";
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
        public string ServerMapping;
        public string Database;
        public string DatabaseMapping;
        public string Username;
        public string UsernameMapping;
        public string Password;
        public string PasswordMapping;
        public string IntergratedSecurity;
        public string IntergratedSecurityMapping;
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

            return builder.ToString();
        }
    }

    public class ConnectionPool : Pool<DbConnection>
    {
        protected ConnectionPool()
        {
        }

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

        public static List<string> GetAllServers(ConnectionInfo con)
        {
            return new List<string>();
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
                //  _con.DbConnection = new SqlCeConnection();
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
                //_con.DbConnection = new SQLLiteConnection();
            }
            else if (con.ServerType == ConnectionType.ODBC)
            {
                //παρε τις μη κενες τιμές του ConnectionInfo και βάλ' τες με βαση τις παραμτερους για Odbc 
                con.ServerMapping = "";
                con.DatabaseMapping = "";
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

    public class DbConnection
    {
        public IDbConnection Connection;

        public DataTable ExecuteProcedure(string procedureName, Dictionary<string, string> parameters = null)
        {
            return Execute(procedureName, true, parameters);
        }

        public DataTable ExecuteQuery(string sql, Dictionary<string, string> parameters = null)
        {
            return Execute(sql, false, parameters);
        }

        private DataTable Execute(string sql, bool isProcedure, Dictionary<string, string> parameters = null)
        {
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
            //add parameters
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value;

                command.Parameters.Add(parameter);
            }
            //
            if (Connection is SqlConnection)
            {
                return Execute<SqlDataAdapter>(command as SqlCommand);
            }
            else if (Connection is MySqlConnection)
            {
                return Execute<MySqlDataAdapter>(command as MySqlCommand);
            }
            //else if (DbConnection is SQLCEconnection)
            //{
            //    //  _con = new SqlCeConnection();
            //}
            else if (Connection is OracleConnection)
            {
                return Execute<OracleDataAdapter>(command as OracleCommand);
            }
            //else if (DbConnection is SqliteConnection)
            //{
            //    //_con = new SQLLiteConnection();
            //}
            else if (Connection is OdbcConnection)
            {
                return Execute<OdbcDataAdapter>(command as OdbcCommand);
            }

            return new DataTable();
        }

        private DataTable Execute<T>(IDbCommand command)
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
}
