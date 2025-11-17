using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Rename_G_Code_Files.src.Database
{
    class SqlLocalDatabase : Database
    {
        public SqlLocalDatabase(string connString, string databasePath)
        {
            connectionString = connString;
            this.databasePath = databasePath;
        }

        private SqlConnection? _Connection = null;
        private SqlCommand? _Command = null;
        private readonly string connectionString;
        private readonly string databasePath;

        protected override DbConnection Connection
        {
            get
            {
                _Connection ??= new SqlConnection(connectionString);
                if (_Connection.State != ConnectionState.Open)
                {
                    _Connection.Open();
                }
                return _Connection;
            }
        }

        protected override DbCommand Command
        {
            get
            {
                _Command ??= new SqlCommand
                    {
                        Connection = Connection as SqlConnection
                    };
                return _Command;
            }
        }

        public override DateTime GetLastModifiedDate()
        {
            DateTime lastModified = DateTime.MinValue;
            string sql = """
            SELECT MAX(ius.last_user_update) AS last_user_update
            FROM sys.dm_db_index_usage_stats AS ius WHERE ius.database_id = DB_ID();
            """;
            using DbDataReader reader = Read(sql);
            if (reader.Read())
                lastModified = reader.GetValue(0) is DBNull ? lastModified : reader.GetDateTime(0);
            return lastModified;
        }
    }
}