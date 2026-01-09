namespace Rename_G_Code_Files.src;

/// <summary>
/// Represents a run from S2M center.
/// </summary>
internal record struct Run
{
    internal Run(string runTag, DateTime outputTime, Option<Job> currentJob)
    {
        RunTag = runTag;
        OutputTime = outputTime;
        CurrentJob = currentJob;

        DestinationPath = CurrentJob
            .Match(
                j => j.GCodePath + "\\" + runTag,
                () => UserSelectFolder("No job exists! Please select a job folder") >>>
                    (s => s.EndsWith("G Codes") ? s : s + "\\G Codes")
            );
    }

    public string RunTag { get; set; }
    public string GCodeOutputPath { get; set; }
    public string JobStateOutputPath { get; set; }
    public DateTime OutputTime { get; set; }
    public Option<Job> CurrentJob { get; set; }
    public string DestinationPath { get; }
}