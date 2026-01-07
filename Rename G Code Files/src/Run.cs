namespace Rename_G_Code_Files.src;

/// <summary>
/// Represents a run from S2M center.
/// </summary>
internal record struct Run
{
    internal Run(string runTag, string gCodeSourcePath, string jobStateSourcePath, DateTime outputTime, IEnumerable<Job> jobs, Job currentJob)
    {
        RunTag = runTag;
        GCodeOutputPath = gCodeSourcePath;
        JobStateOutputPath = jobStateSourcePath;
        OutputTime = outputTime;
        Jobs = jobs;
        CurrentJob = currentJob;
    }

    public string RunTag { get; set; }
    public string GCodeOutputPath { get; set; }
    public string JobStateOutputPath { get; set; }
    public DateTime OutputTime { get; set; }
    public IEnumerable<Job> Jobs { get; set; }
    public Job CurrentJob { get; set; }
    public string DestinationPath => Path.Combine(CurrentJob.GCodePath, RunTag);
}