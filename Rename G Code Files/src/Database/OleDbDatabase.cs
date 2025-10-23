using System.Data.OleDb;
using System.Data.Common;
using System.Data;

namespace Rename_G_Code_Files.Database
{
    class OleDbDatabase : Database
    {
        public OleDbDatabase(string connString, string databasePath)
        {
            connectionString = connString;
            this.databasePath = databasePath;
        }

        private OleDbConnection? _Connection = null;
        private OleDbCommand? _Command = null;
        private readonly string connectionString;
        private readonly string databasePath;

        public override DbConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection = new OleDbConnection(connectionString);
                }
                if (_Connection.State != ConnectionState.Open)
                {
                    _Connection.Open();
                }
                return _Connection;
            }
        }

        public override DbCommand Command
        {
            get
            {
                if (_Command == null)
                {
                    _Command = new OleDbCommand
                    {
                        Connection = Connection as OleDbConnection
                    };
                }
                return _Command;
            }
        }

        public override void Execute(string sql)
        {
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        public override DbDataReader Read(string sql)
        {
            Command.CommandText = sql;
            DbDataReader reader = Command.ExecuteReader();
            return reader;
        }

        public override DateTime GetLastModifiedDate()
        {
            return File.GetLastWriteTime(databasePath);
        }
    }
}