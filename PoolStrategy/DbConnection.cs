using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.Text.RegularExpressions;
using System.Linq;

namespace DatabasePool
{
    internal struct BatchDelimiterPattern
    {
        public const string SQL = @"\n\s*GO\s*";
    }

    internal struct PoolExceptionCode
    {
        public const int ROLLBACK = 0;
        public const int SQLEXECUTION = 1;
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
            var batches = Regex.Split(command.CommandText.Trim(), BatchDelimiterPattern.SQL, RegexOptions.IgnoreCase)
                .Where(s => s != String.Empty && s.Length != 0);
            //execute batch
            foreach (string batch in batches)
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
}
