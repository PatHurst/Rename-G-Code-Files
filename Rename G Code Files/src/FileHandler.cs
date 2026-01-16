using Microsoft.VisualBasic.ApplicationServices;

namespace Rename_G_Code_Files.src;

internal static class FileHandler
{
    const string material_prefix = "// MATERIAL:";
    const string size_prefix = "// SHEET SIZE:";
    
    /// <summary>
    ///     Moves all the output files to the correct directories.
    /// </summary>
    public static Unit MoveAllOutputFiles(in Run run)
    {
        if (Directory.GetFiles(run.GCodeOutputPath, $"{run.RunTag}*.anc").Length > 0)
        {
            _ = MoveAndRenameGCodeFiles(in run);
            _ = MoveJobStateFiles(run);
            Logger.Instance.LogInformation($"Files moved from {run.GCodeOutputPath} to {run.DestinationPath}");
        }
        else
        {
            Logger.Instance.LogInformation("No files found in " + run.GCodeOutputPath);
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
            {
                safeFileCopy(file, file >>> addMaterialToFileName >>> changeDir)
                    .Bind(addHeader)
                    .Match(
                        _ => File.Delete(file),
                        _ => Logger.Instance.LogWarning($"The source file \"{file}\" was not deleted.")
                    );

            });
        return unit;
    }

    private static readonly Func<Run,string,string> changeDirectories =
        (run, original) => Path.Combine(run.DestinationPath, Path.GetFileName(original));

    private static Fin<string> safeFileCopy(string from, string to)
    {
        try
        {
            File.Copy(from, to, true);
            return to;
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e);
            return Fin<string>.Fail(e);
        }
    }

    private static readonly Func<string, string> addMaterialToFileName =
        filePath =>
        {
            var lines = File.ReadLines(filePath);
            var material = lines
                .Find(l => l.StartsWith(material_prefix))
                .Map(l => l[material_prefix.Length..].Replace("/", "-").Trim())
                .IfNone("Unknown Material");
            var sheetSize = lines
                .Find(l => l.StartsWith(size_prefix))
                .Map(l => l[size_prefix.Length..].Trim())
                .IfNone(string.Empty);

            return Path.Combine(
                Path.GetDirectoryName(filePath)!,
                $"{Path.GetFileNameWithoutExtension(filePath)} _ ({material}) ({sheetSize}).anc"
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
                .Iter(t => safeFileCopy(t.file, t.Item2)));


    private static readonly Func<Run,string,Fin<string>> addheader =
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
                return Fin<string>.Fail(ex);
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
                // wait to make sure CV is done writing to the file.
                Thread.Sleep(500);
                return unit;
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
        Logger.Instance.LogInformation("Timed out waiting for Job State file to be created!");
        return unit;
    }
}
