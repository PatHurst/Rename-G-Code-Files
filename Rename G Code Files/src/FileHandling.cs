using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace Rename_G_Code_Files.src;

internal class FileHandler
{
    internal FileHandler(Job job)
    {
        this.job = job;
    }

    private Job job;

    /// <summary>
    /// Moves all the output files to the correct directories.
    /// </summary>
    public void MoveAllOutputFiles()
    {
        if (Directory.GetFiles(job.OutputPath).Length > 0)
        {
            MoveAndRenameGCodeFiles(job.OutputPath);
            MoveJobStateFile(job.OutputPath);
        }
        else
        {
            Environment.Exit(0);
        }
    }

    private void MoveAndRenameGCodeFiles(string gCodeSource)
    {
        if (!Directory.Exists(gCodeSource))
            throw new ArgumentNullException(gCodeSource, $"G Code source {gCodeSource} does not Exist!");
        if (!Directory.Exists(job.GCodePath))
            Directory.CreateDirectory(job.GCodePath);
        foreach (string file in Directory.GetFiles(gCodeSource, "*.anc"))
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
            Logger.ExceptionHandler(ex);
        }
    }

    private string RenameGCodeFileByReadLines(string filePath)
    {
        string matName = string.Empty;
        string newFileName = string.Empty;
        string searchString = "// MATERIAL:";
        foreach (string line in File.ReadLines(filePath))
        {
            if (line.StartsWith(searchString))
            {
                matName = Util.Right(line, line.Length - searchString.Length).Replace("/", "-");
                break;
            }
        }
        // Cuts the path and extension off the file path.
        newFileName = Util.Right(filePath, filePath.Length - filePath
                    .LastIndexOf('\\') - 1).Substring(0, Util.Right(filePath, filePath.Length - filePath
                    .LastIndexOf('\\') - 1).LastIndexOf('.'));
        return $"{job.GCodePath}\\{newFileName} _ {matName}.anc";
    }

    private void MoveJobStateFile(string jobStateSource)
    {
        JobStateFileExists();
        foreach (string file in Directory.GetFiles(jobStateSource, "*.sn?"))
        {
            string ext = Util.Right(file, 4);
            string newFilePath = $"{job.GCodePath}\\{job.Name}_Job State {job.OutputTime}{ext}";
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
                            $"// JOB NAME: {job.Name}\r\n" +
                            $"// OUTPUT TIME: {job.OutputTime}\r\n";
            writer.Write(header);
            writer.Write(tempText);
        }
        catch (Exception ex)
        {
            Logger.ExceptionHandler(ex);
        }
    }

    
    
    private void JobStateFileExists()
    {
        for (int counter = 0; counter < 60; counter++)
        {
            if (Directory.GetFiles(job.JobStateSource, "*.sn?").Length > 0)
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
