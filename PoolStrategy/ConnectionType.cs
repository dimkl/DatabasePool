using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        private static List<string> Types;

        public static List<string> GetAll()
        {
            if (Types == null)
            {
                Types = new List<string>();
                typeof(ConnectionType).GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public)
                    .ToList<FieldInfo>().ForEach(delegate(FieldInfo field)
                {
                    Types.Add(field.GetValue(null).ToString());
                });
            }
            return Types;
        }
    }
}
