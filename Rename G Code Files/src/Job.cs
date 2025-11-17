using System.Data.Common;
using Microsoft.Win32;
using Rename_G_Code_Files.src.Database;

namespace Rename_G_Code_Files.src;

internal class Job(string name, string filepath, int version)
{
    private string _name = name;
    private string _filePath = filepath;
    private int _version = version;

    /// <summary>
    /// The name of the Job.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The filepath to the .cvj file.
    /// </summary>
    public string FilePath => _filePath;

    /// <summary>
    /// The path to where the G Code files should be saved to.
    /// </summary>
    public string GCodePath => string.Concat(_filePath.AsSpan(0, _filePath.LastIndexOf('\\') + 1), "G Codes\\");

    /// <summary>
    /// The version of CV the job is output from.
    /// </summary>
    public int CVVersion => _version;
}
