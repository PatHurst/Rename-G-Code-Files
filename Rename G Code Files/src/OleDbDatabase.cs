namespace Rename_G_Code_Files.src;

class OleDbDatabase : Database
{
    public OleDbDatabase(string connString, string databasePath)
    {
        connectionString = connString;
        this.databasePath = databasePath;
    }

    private OleDbConnection? _connection = null;
    private readonly string connectionString;
    private readonly string databasePath;

    protected override DbConnection Connection
    {
        get
        {
            _connection ??= new OleDbConnection(connectionString);
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }
    }

    public override DateTime GetLastModifiedDate() => File.GetLastWriteTime(databasePath);

    ~OleDbDatabase()
    {
        base.Dispose(false);
    }
}
