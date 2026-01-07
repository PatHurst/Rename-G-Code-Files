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

    public virtual Fin<Run> GetRunInfo() =>
        Try(() =>
        {
            var jobList = GetJobInfo();
            
            using var command = Connection.CreateCommand();
            command.CommandText = @"SELECT TOP 1 [RunTag], [RunTime] FROM [RunInfo]";
            using var reader = command.ExecuteReader();

            return reader.Read()
                ? new Run()
                {
                    RunTag = reader.GetString(0),
                    OutputTime = reader.GetDateTime(1),
                    Jobs = jobList,
                    CurrentJob = jobList.FirstOrDefault()
                }
                : Fin<Run>.Fail("Database contained no run info!");
        })
        .IfFail(e =>
        {
            Logger.Instance.LogException(e);
            return Fin<Run>.Fail(e);
        });

    private List<Job> GetJobInfo() =>
        Try(() =>
        {
            using var command = Connection.CreateCommand();
            command.CommandText = @"SELECT TOP 100 [Description], [JobFilePath] FROM [Jobs];";
            using var reader = command.ExecuteReader();

            List<Job> jobs = [];
            while (reader.Read())
            {
                var jobName = reader.GetString(0).Replace('/', '-').Replace('\\', '-');
                jobs.Add(new Job(jobName, reader.GetString(1)));
            }
            return jobs;
        })
        .IfFail(e =>
        {
            Logger.Instance.LogException(e);
            return [];
        });

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
