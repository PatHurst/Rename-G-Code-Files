using System.Data.Common;
using Microsoft.Win32;
using Rename_G_Code_Files.src.Database;

namespace Rename_G_Code_Files.src;

internal class Job
{
    public Job()
    {
        database = DatabaseFactory.GetDatabase(this);
        GetJobInfo();
        _JobStateSource = Util.GetRegistryValue<string>($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\S2M {CVVersion}", "SNCPath");
    }

    /// <summary>
    /// The name of the Job.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The filepath to the .cvj file.
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>
    /// The path to the files output from S2M.
    /// </summary>
    public string OutputPath
    {
        get { return _OutputPath; }
    }

    /// <summary>
    /// The path to the job state file output from S2M.
    /// </summary>
    public string JobStateSource
    {
        get { return _JobStateSource; }
    }

    /// <summary>
    /// The run tag of the Job.
    /// </summary>
    public string RunTag { get; private set; } = string.Empty;
    /// <summary>
    /// The time of the output run.
    /// </summary>

    public string OutputTime { get; private set; } = string.Empty;
    /// <summary>
    /// The path to where the G Code files should be saved to.
    /// </summary>

    public string GCodePath
    {
        get
        {
            return string.Concat(FilePath.AsSpan(0, FilePath.LastIndexOf('\\') + 1), "G Codes\\", RunTag);
        }
    }

    /// <summary>
    /// The version of CV the job is output from.
    /// </summary>
    public int CVVersion
    {
        get { return _CVVersion; }
    }

    // Private fields.
    private Database.Database database;
    private int _CVVersion = Util.GetRegistryValue<int>(@"HKEY_CURRENT_USER\Software\Hurst Software Engineering\G Code Post Processor", "RunningVersion");
    private string _OutputPath = Util.GetRegistryValue<string>(@"HKEY_CURRENT_USER\Software\Hurst Software Engineering\G Code Post Processor", "OutputPath");
    private string _JobStateSource;

    private void GetJobInfo()
    {
        try
        {
            DbDataReader reader = database.Read(@"SELECT TOP 1 [Description], [JobFilePath] FROM [Jobs];");
            while (reader.Read())
            {
                Name = reader.GetString(0);
                FilePath = reader.GetString(1);
            }
            reader.Close();
            reader = database.Read(@"SELECT TOP 1 [RunTag], [RunTime], [OutputPath] FROM [RunInfo]");
            while (reader.Read())
            {
                RunTag = reader.GetString(0);
                OutputTime = reader.GetDateTime(1).ToString("MM-dd-yy hh-mm-ss");
            }
            reader.Close();
        }
        catch (DbException ex)
        {
            Logger.Logger.ExceptionHandler(ex);
        }
        catch (InvalidOperationException ex)
        {
            Logger.Logger.ExceptionHandler(ex);
        }
    }
}
