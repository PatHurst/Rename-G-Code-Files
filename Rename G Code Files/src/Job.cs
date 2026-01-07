namespace Rename_G_Code_Files.src;

internal readonly record struct Job
{
    internal Job(string name, string filePath) => (Name, FilePath) = (name, filePath);

    /// <summary>
    /// The name of the Job.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The filepath to the .cvj file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The path to where the G Code files should be saved to.
    /// </summary>
    public string GCodePath => Path.Combine(Path.GetDirectoryName(FilePath) ?? "", "G Codes");
}
