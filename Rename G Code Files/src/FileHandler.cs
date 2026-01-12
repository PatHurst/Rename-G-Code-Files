namespace Rename_G_Code_Files.src;

internal static class FileHandler
{
    const string prefix = "// MATERIAL:";
    
    /// <summary>
    ///     Moves all the output files to the correct directories.
    /// </summary>
    public static Unit MoveAllOutputFiles(in Run run)
    {
        if (Directory.GetFiles(run.GCodeOutputPath, $"{run.RunTag}*.anc").Length > 0)
        {
            _ = MoveAndRenameGCodeFiles(in run) >> MoveJobStateFiles(run);
        }
        else
        {
            Logger.Instance.LogInformation("No files found in " + run.GCodeOutputPath);
            Logger.Instance.LogData(in run);
            Environment.Exit(0);
        }
        return unit;
    }

    private static Unit MoveAndRenameGCodeFiles(in Run run)
    {
        if (!Directory.Exists(run.DestinationPath))
            Directory.CreateDirectory(run.DestinationPath);

        var changeDir = par(changeDirectories, run);
        var addHeader = par(addheader, run);
        
        Directory.GetFiles(run.GCodeOutputPath, $"{run.RunTag}*.anc")
            .Iter(file => 
                File.Move(file, file >>> addHeader >>> addMaterialToFileName >>> changeDir, true));
        return unit;
    }

    private static readonly Func<Run,string,string> changeDirectories =
        (run, original) => $"{run.DestinationPath}\\{Path.GetFileName(original)}";

    private static readonly Func<string, string> addMaterialToFileName =
        filePath =>
        {
            var material = File.ReadLines(filePath)
                .Find(l => l.StartsWith(prefix))
                .Map(l => l[prefix.Length..].Replace("/", "-").Trim())
                .IfNone("Unknown Material");

            return Path.Combine(
                Path.GetDirectoryName(filePath)!,
                $"{Path.GetFileNameWithoutExtension(filePath)} _ {material}.anc"
            );
        };


    private static Unit MoveJobStateFiles(Run run) =>
        WaitForJobStateFile(run.JobStateOutputPath) >>
            (() => Directory.GetFiles(run.JobStateOutputPath, "*.sn?")
                .Map(file => (
                    file,
                    Path.Combine(run.DestinationPath,
                        $"{run.CurrentJob.Match(s => s.Name, () => string.Empty)}_{run.OutputTime:ddd, MMM dd yyyy hh-mm-ss}{Path.GetExtension(file)}")
                ))
                .Iter(t => File.Move(t.file, t.Item2, true)));


    private static readonly Func<Run,string,string> addheader =
        (run, filePath) =>
        {
            try
            {
                string tempText = File.ReadAllText(filePath);
                using StreamWriter writer = File.CreateText(filePath);
                string header = $"%\r\n" +
                                $"//=================== INFO ===================//\r\n" +
                                $"// JOB NAME: {run.CurrentJob.Match(s => s.Name, () => string.Empty)}\r\n" +
                                $"// OUTPUT TIME: {run.OutputTime}\r\n";
                writer.Write(header);
                writer.Write(tempText);
                writer.Flush();
                return filePath;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex);
                return filePath;
            }
        };

    /// <summary>
    /// A blocking operation to wait for the job state file to be created.
    /// Waits for a maximum of 1 minute.
    /// </summary>
    private static Unit WaitForJobStateFile(string path)
    {
        for (int counter = 0; counter < 60; counter++)
        {
            if (Directory.GetFiles(path, "*.sn?").Length > 0)
            {
                return unit;
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
        return unit;
    }
}
