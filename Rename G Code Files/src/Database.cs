namespace Rename_G_Code_Files.src;

abstract class Database : IDisposable
{
    private bool _isDisposed;

    ~Database()
    {
        Dispose(disposing: false);
    }

    protected abstract DbConnection Connection { get; }
    
    public abstract DateTime GetLastModifiedDate();

    public virtual Fin<Run> GetRunInfo()
    {
        DbCommand command = null!;
        DbDataReader reader = null!;
        try
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
            var jobInfo = GetJobInfo();

            command = Connection.CreateCommand();
            command.CommandText = @"SELECT TOP 1 [RunTag], [RunTime] FROM [RunInfo]";
            reader = command.ExecuteReader();

            return reader.Read()
                ? new Run(reader.GetString(0),reader.GetDateTime(1), jobInfo)
                : Fin<Run>.Fail("Database contained no run info!");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e);
            return Fin<Run>.Fail(e);
        }
        finally
        {
            command?.Dispose();
            reader?.Close();
            reader?.Dispose();
        }
    }

    private Option<Job> GetJobInfo()
    {
        DbCommand command = null!;
        DbDataReader reader = null!;
        try
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
            command = Connection.CreateCommand();
            command.CommandText = @"SELECT TOP 1 [Description], [JobFilePath] FROM [Jobs];";
            reader = command.ExecuteReader();

            if (reader.Read())
            {
                var jobName = reader.GetString(0).Replace('/', '-').Replace('\\', '-');
                return new Job(jobName, reader.GetString(1));
            }
            return None;
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e);
            return None;
        }
        finally
        {
            command?.Dispose();
            reader?.Close();
            reader?.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Connection?.Close();
                Connection?.Dispose();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
