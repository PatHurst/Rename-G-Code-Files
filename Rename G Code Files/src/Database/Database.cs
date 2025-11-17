using System.Configuration;
using System.Data.Common;
using System.Data;

namespace Rename_G_Code_Files.src.Database;

abstract class Database : IDisposable
{
    protected int _version;
    protected bool _isDisposed;
    
    ~Database()
    {
        Dispose(disposing: false);
    }

    protected abstract DbConnection Connection { get; }
    protected abstract DbCommand Command { get; }

    public virtual void Execute(string sql)
    {
        Command.CommandText = sql;
        Command.ExecuteNonQuery();
    }

    public virtual DbDataReader Read(string sql)
    {
        Command.CommandText = sql;
        DbDataReader reader = Command.ExecuteReader();
        return reader;
    }

    public abstract DateTime GetLastModifiedDate();

    public virtual Run GetRunInfo()
    {
        DbDataReader reader = Read(@"SELECT TOP 1 [RunTag], [RunTime], [OutputPath] FROM [RunInfo]");
        Run run= new();
        if (reader.Read())
        {
            run.RunTag = reader.GetString(0);
            run.OutputTime = reader.GetDateTime(1);
            run.GCodeOutputPath = reader.GetString(2);
        }
        else
        {
            throw new InvalidOperationException("No Run info found in database!");
        }
        reader.Close();
        run.Jobs = GetJobInfo();
        return run;
    }

    private IEnumerable<Job> GetJobInfo()
    {
        DbDataReader reader = Read(@"SELECT TOP 100 [Description], [JobFilePath] FROM [Jobs];");
        List<Job> jobs = [];
        try
        {
            while (reader.Read())
            {
                jobs.Add(new Job(reader.GetString(0), reader.GetString(1), _version));
            }
            reader.Close();

        }
        catch (InvalidOperationException ex)
        {
            Logger.Instance.LogException(ex);
        }
        finally
        {
            reader?.Close();
        }
        return jobs;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Connection?.Close();
                Command?.Dispose();
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
