using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Rename_G_Code_Files.src;

namespace Rename_G_Code_Files.src;

internal class FileHandler
{
    internal FileHandler(Run run)
    {
        _run = run;
    }

    private readonly Run _run;

    /// <summary>
    ///     Moves all the output files to the correct directories.
    /// </summary>
    public void MoveAllOutputFiles()
    {
        if (Directory.GetFiles(_run.GCodeOutputPath).Length > 0)
        {
            MoveAndRenameGCodeFiles();
            MoveJobStateFile();
        }
        else
        {
            Logger.Instance.LogInformation("No files found in " + _run.GCodeOutputPath);
            Logger.Instance.LogData(_run);
            Environment.Exit(0);
        }
    }

    private void MoveAndRenameGCodeFiles()
    {
        if (!Directory.Exists(_run.GCodeOutputPath))
            throw new ArgumentNullException(_run.GCodeOutputPath, $"G Code source {_run.GCodeOutputPath} does not Exist!");
        if (!Directory.Exists(_run.DestinationPath))
            Directory.CreateDirectory(_run.DestinationPath);
        foreach (string file in Directory.GetFiles(_run.GCodeOutputPath, "*.anc"))
        {
            AddSummaryToCNCFile(file);
            MoveFile(file, RenameGCodeFileByReadLines(file));
        }
    }

    private void MoveFile(string oldFilePath, string newFilePath)
    {
        try
        {
            File.Move(oldFilePath, newFilePath,true);
        }
        catch (Exception ex)
        {
            Logger.Instance.LogException(ex);
        }
    }

    private string RenameGCodeFileByReadLines(string filePath)
    {
        string matName = string.Empty;
        string newFileName;
        string searchString = "// MATERIAL:";
        foreach (string line in File.ReadLines(filePath))
        {
            if (line.StartsWith(searchString))
            {
                matName = Util.Right(line, line.Length - searchString.Length).Replace("/", "-").Trim();
                break;
            }
        }
        // Cuts the path and extension off the file path.
        newFileName = Util.Right(filePath, filePath.Length - filePath
                    .LastIndexOf('\\') - 1)[..Util.Right(filePath, filePath.Length - filePath
                    .LastIndexOf('\\') - 1).LastIndexOf('.')];
        return $"{_run.DestinationPath}\\{newFileName} _ {matName}.anc";
    }

    private void MoveJobStateFile()
    {
        WaitForJobStateFile();
        foreach (string file in Directory.GetFiles(_run.JobStateOutputPath, "*.sn?"))
        {
            string ext = Util.Right(file, 4);
            string newFilePath = $"{_run.DestinationPath}\\{_run.CurrentJob.Name}_{_run.OutputTime:ddd, MMM dd yyyy hh-mm-ss}{ext}";
            MoveFile(file, newFilePath);
        }
    }

    private void AddSummaryToCNCFile(string filePath)
    {
        try
        {
            string tempText = File.ReadAllText(filePath);
            using StreamWriter writer = File.CreateText(filePath);
            string header = $"%\r\n" +
                            $"//=================== INFO ===================//\r\n" +
                            $"// JOB NAME: {_run.CurrentJob.Name}\r\n" +
                            $"// OUTPUT TIME: {_run.OutputTime}\r\n";
            writer.Write(header);
            writer.Write(tempText);
        }
        catch (Exception ex)
        {
            Logger.Instance.LogException(ex);
        }
    }

    
    
    private void WaitForJobStateFile()
    {
        for (int counter = 0; counter < 60; counter++)
        {
            if (Directory.GetFiles(_run.JobStateOutputPath, "*.sn?").Length > 0)
            {
                break;
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }
}
