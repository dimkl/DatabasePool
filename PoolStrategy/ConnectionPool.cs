using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

namespace DatabasePool
{
    public static class ConnectionType
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

    internal struct BatchDelimiter
    {
        public const string SQL = "GO";
    }

    internal struct PoolExceptionCode
    {
        public const int ROLLBACK = 0;
        public const int SQLEXECUTION = 1;

    }

    public class ConnectionInfo
    {
        public string Server;
        public string Database;
        public string Username;
        public string Password;
        public string IntergratedSecurity;
    }

    public class ConnectionInfoMapping : ConnectionInfo
    {
        public string Server;
        public string Database;
        public string Username;
        public string Password;
        public string IntergratedSecurity;
        public string Custom;
    }

    public class ConnectionInfoData : ConnectionInfo
    {
        public string Name;
        public string ServerType;

        public ConnectionInfoMapping Mapping = new ConnectionInfoMapping();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.GetType().GetFields().ToList<FieldInfo>().ForEach(delegate(FieldInfo field) 
            {
                // check if Mapping for this Property exists
                var mappingField = Mapping.GetType().GetField(field.Name);
                if (mappingField == null)
                {
                    return;
                }
                // add Mapping value and value of Property if they are not null
                var value = field.GetValue(this);
                var mappingValue = mappingField.GetValue(Mapping);
                if (value != null && mappingValue != null)
                {
                    builder.AppendFormat("{0}={1};", mappingValue, value);
                }
            });
            if (Mapping.Custom != null)
            {
                builder.AppendFormat(Mapping.Custom);
            }

            return builder.ToString();
        }
    }

    public class DbConnection
    {
        public IDbConnection Connection;
        public bool IsProcedure = false;
        public bool IsTransaction = false;
        public bool IsBatch = false;

        private Dictionary<int, string> Errors = new Dictionary<int, string>();

        public DataTable Execute(string sql, Dictionary<string, string> parameters = null)
        {
            //reset Errors
            Errors = new Dictionary<int, string>();
            // 
            IDbCommand command = Connection.CreateCommand();
            command.CommandText = sql;
            //
            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
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
                Connection.Open();
                command = AddCommandOptions(command);

                if (Connection is SqlConnection)
                {
                    table = ExecuteAdapter<SqlDataAdapter>(command as SqlCommand);
                }
                else if (Connection is MySqlConnection)
                {
                    table = ExecuteAdapter<MySqlDataAdapter>(command as MySqlCommand);
                }
                else if (Connection is SqlCeConnection)
                {
                    return ExecuteAdapter<SqlCeDataAdapter>(command as SqlCeCommand);
                }
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

                if (IsTransaction)
                {
                    command.Transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Errors.Add(PoolExceptionCode.SQLEXECUTION, ex.Message);
                try
                {
                    command.Transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    Errors.Add(PoolExceptionCode.ROLLBACK, rollbackEx.Message);
                }
            }
            finally
            {
                if (Connection != null)
                    Connection.Close();
            }

            return table;
        }

        private DataTable ExecuteAdapter<T>(IDbCommand command)
            where T : IDbDataAdapter, new()
        {
            if (IsBatch)
            {
                ExecuteBatch(command);
                return new DataTable();
            }
            var ds = new DataSet();

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
            _adapter.Fill(ds);

            if (ds.Tables.Count == 0)
            {
                return new DataTable();
            }

            return ds.Tables[0];
        }

        private void ExecuteBatch(IDbCommand command)
        {
            //split command.CommandText to batches
            string[] batches = command.CommandText
                .Trim()
                .Split(new string[] { BatchDelimiter.SQL }, StringSplitOptions.RemoveEmptyEntries);
            //execute batch
            foreach (var batch in batches)
            {
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }
        }

        private IDbCommand AddCommandOptions(IDbCommand command)
        {
            //force transaction if sql containes batches
            if (IsBatch)
            {
                IsTransaction = true;
            }
            if (IsProcedure)
            {
                command.CommandType = CommandType.StoredProcedure;
            }

            if (IsTransaction)
            {
                command.Transaction = Connection.BeginTransaction();
            }

            return command;
        }

        public Dictionary<int, string> GetErrors()
        {
            return Errors;
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

        public static List<string> GetAllDatabases(ConnectionInfoData con)
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

        public static List<string> GetAllServers(ConnectionInfoData con)
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

        public void Create(ConnectionInfoData con, string name = "")
        {
            DbConnection _con = new DbConnection();

            if (con.ServerType == ConnectionType.MYSQL)
            {
                con.Mapping.Server = "Server";
                con.Mapping.Database = "Database";
                con.Mapping.Username = "Uid";
                con.Mapping.Password = "Pwd";
                _con.Connection = new MySqlConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQL)
            {
                con.Mapping.Server = "Server";
                con.Mapping.Database = "Database";
                con.Mapping.Username = "User Id";
                con.Mapping.Password = "Password";

                _con.Connection = new SqlConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLCE)
            {
                con.Mapping.Database = "Data Source";
                con.Mapping.Password = "Password";
                con.Mapping.Custom = "Encrypt Database = True;";

                _con.Connection = new SqlCeConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.ORACLE)
            {
                con.Mapping.Server = "Server";
                con.Mapping.Database = "Data Source";
                con.Mapping.Username = "User Id";
                con.Mapping.Password = "Password";
                con.Mapping.IntergratedSecurity = "Integrated Security";
                //
                _con.Connection = new OracleConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.SQLITE)
            {
                con.Mapping.Database = "Data Source";
                con.Mapping.Password = "Password";
                con.Mapping.Custom = "Version=3;";

                _con.Connection = new SQLiteConnection(con.ToString());
            }
            else if (con.ServerType == ConnectionType.ODBC)
            {
                con.Mapping.Server = "Dsn";
                con.Mapping.Username = "Uid";
                con.Mapping.Password = "Pwd";

                _con.Connection = new OdbcConnection(con.ToString());
            }

            name = name != String.Empty ? name : con.Name;
            base.Create(name, _con);
        }

        public void Test(ConnectionInfoData con)
        {
            //create connection
            Create(con, "Test");
            //get connection
            using (IDbConnection _con = Checkout("Test").Connection)
            {
                _con.Open();
            }
            //remove connection
            Container.Remove("Test");
        }

    }

}
