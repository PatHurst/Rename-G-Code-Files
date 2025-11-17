using System.Data.OleDb;
using System.Data.Common;
using System.Data;

namespace Rename_G_Code_Files.src.Database
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

        protected override DbConnection Connection
        {
            get
            {
                _Connection ??= new OleDbConnection(connectionString);
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
                _Command ??= new OleDbCommand
                    {
                        Connection = Connection as OleDbConnection
                    };
                return _Command;
            }
        }



        public override DateTime GetLastModifiedDate()
        {
            return File.GetLastWriteTime(databasePath);
        }
    }
}