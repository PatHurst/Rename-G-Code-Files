using Microsoft.Identity.Client;

namespace Rename_G_Code_Files.src;

/// <summary>
/// Represents a run from S2M center.
/// </summary>
internal class Run()
{
    public string RunTag { get; set; }
    public string GCodeOutputPath { get; set; }
    public string JobStateOutputPath { get; set; }
    public string DestinationPath { get; set; }
    public DateTime OutputTime { get; set; }
    public IEnumerable<Job> Jobs { get; set; }
    public Job CurrentJob { get; set; }
}