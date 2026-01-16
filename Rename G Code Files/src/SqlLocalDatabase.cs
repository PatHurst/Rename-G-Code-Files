namespace Rename_G_Code_Files.src;

class SqlLocalDatabase : Database
{
    public SqlLocalDatabase(string connString, string databasePath)
    {
        connectionString = connString;
        this.databasePath = databasePath;
    }

    private SqlConnection? _connection = null;
    private readonly string connectionString;
    private readonly string databasePath;

    protected override DbConnection Connection
    {
        get
        {
            _connection ??= new SqlConnection(connectionString);
            return _connection;
        }
    }

    public override DateTime GetLastModifiedDate()
    {
        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }
        var lastModified = DateTime.MinValue;
        string sql = """
            SELECT MAX(ius.last_user_update) AS last_user_update
            FROM sys.dm_db_index_usage_stats AS ius WHERE ius.database_id = DB_ID();
            """;

        using var command = Connection.CreateCommand();
        command.CommandText = sql;
        var reader = command.ExecuteReader();
        try
        {
            if (reader.Read())
            {
                lastModified = reader.GetValue(0) is DBNull
                    ? lastModified 
                    : reader.GetDateTime(0);
            }
            return lastModified;
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e);
            return DateTime.MinValue;
        }
        finally
        {
            reader?.Close();
        }
    }

    ~SqlLocalDatabase()
    {
        base.Dispose(false);
    }
}
