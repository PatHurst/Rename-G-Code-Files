using Microsoft.Win32;
using Rename_G_Code_Files.src;

namespace Rename_G_Code_Files;

internal class Logger
{
    private static List<Exception> exceptionLogs = [];

    /// <summary>
    /// Method to log Exceptions.
    /// </summary>
    /// <param name="e">The exception thrown</param>
    /// <param name="additionInfo">Any additional notes</param>
    public static void ExceptionHandler(Exception e)
    {
        exceptionLogs.Add(e);
        MessageBox.Show(e.Message);
    }

    public static void LogRunData(Job job)
    {
        try
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Hurst Software Engineering\G Code Post Processor", "MovePath", $"{job.GCodePath}");
            string todayDate = DateTime.Now.Date.ToString("MM/dd/yyyy");
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Logging", @"DataLog.log");
            if (!File.Exists(logFile))
                File.Create(logFile);
            using StreamWriter writer = File.AppendText(logFile);
            writer.WriteLine($"\r\n\r\n\r\n\r\n********************************************* Begin Log {todayDate} {DateTime.Now.TimeOfDay} *********************************************");
            writer.Write($"RunTag = {job.RunTag};  RunTime = {job.OutputTime};  OutputPath = {job.OutputPath};");
            writer.Write("\r\n");
            writer.WriteLine("\r\n****************** Output ******************");
            writer.WriteLine($"Job output files moved to {job.GCodePath}");
            if (exceptionLogs.Count > 0)
            {
                writer.WriteLine("\r\n****************** Errors ******************");
                foreach (Exception ex in exceptionLogs)
                {
                    writer.WriteLine(ex.Message);
                }
            }

            writer.Write($"\r\n********************************************* End Log {todayDate} {DateTime.Now.TimeOfDay} *********************************************");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}