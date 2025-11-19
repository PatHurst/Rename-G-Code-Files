using Microsoft.Win32;
using System.Text;
using Rename_G_Code_Files.src;

namespace Rename_G_Code_Files.src;

/// <summary>
///     Implements logging information and exceptions.
/// </summary>
/// <remarks>
///     Implemented as a singleton class.
/// </remarks>
internal class Logger
{
    private static readonly Logger logger = new();

    /// <summary>
    ///     The Logger access point.
    /// </summary>
    public static Logger Instance => logger;

    // keep a list of exceptions caught.
    private List<Exception> exceptions = [];

    // keep a list of warning messages
    public List<string> warnings = [];

    // keep a list of information messages.
    public List<string> informationMessages = [];

    /// <summary>
    ///     Method to log caught Exceptions.
    /// </summary>
    /// <param name="e">The exception thrown</param>
    public void LogException(Exception e) => exceptions.Add(e);

    /// <summary>
    ///     Log a warning message.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public void LogWarning(string message) => warnings.Add(message);

    /// <summary>
    ///     Log information.
    /// </summary>
    /// <param name="message">The information message.</param>
    public void LogInformation(string message) => informationMessages.Add(message);


    /// <summary>
    ///     Writes all previous logs to the log file and records additional run data.
    /// </summary>
    /// <param name="run"></param>
    public void LogData(Run run)
    {
        var nl = Environment.NewLine;
        string todayDate = DateTime.Now.Date.ToString("MM/dd/yyyy");
        string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logging", @"DataLog.log");
        try
        {
            if (!File.Exists(logFile))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logging"));
                File.Create(logFile);
            }
        }
        catch
        {
            MessageBox.Show("Log File not Found!");
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"\r\n\r\n\r\n\r\n********************************************* Begin Log {todayDate} {DateTime.Now.TimeOfDay} *********************************************")
            .AppendLine($"Version = {run.CurrentJob.CVVersion};RunTag = {run.RunTag};  RunTime = {run.OutputTime};  OutputPath = {run.GCodeOutputPath};").Append(nl)
            .AppendLine("\r\n****************** Output ******************")
            .AppendLine($"Job output files moved to {run.DestinationPath}").Append(nl);

        if (exceptions.Count > 0)
        {
            stringBuilder.AppendLine(nl).AppendLine("****************** Errors ******************");
            foreach (Exception ex in exceptions)
                stringBuilder.Append(ex.GetType().Name).Append(": ").AppendLine(ex.Message).Append(ex.StackTrace);
        }
        if (warnings.Count > 0)
        {
            stringBuilder.AppendLine(nl).AppendLine("****************** Warnings ******************");
            foreach (var s in warnings)
                stringBuilder.AppendLine(s);
        }
        if (informationMessages.Count > 0)
        {
            stringBuilder.AppendLine(nl).AppendLine("****************** Info ******************");
            foreach (string s in informationMessages)
                stringBuilder.AppendLine(s);
        }

        stringBuilder.AppendLine($"\r\n********************************************* End Log {todayDate} {DateTime.Now.TimeOfDay} *********************************************");

        StreamWriter? writer = null;
        try
        {
            writer = File.AppendText(logFile);
            writer?.Write(stringBuilder.ToString());

        }
        catch { }
        finally
        {
            writer?.Flush();
            writer?.Dispose();
        }
    }
}