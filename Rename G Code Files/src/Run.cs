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

    public string RunTag { get; init; }
    public string GCodeOutputPath { get; init; }
    public string JobStateOutputPath { get; init; }
    public DateTime OutputTime { get; init; }
    public Option<Job> CurrentJob { get; init; }
    public string DestinationPath { get; }
}