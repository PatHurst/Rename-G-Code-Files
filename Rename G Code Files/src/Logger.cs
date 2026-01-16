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
    private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DataLog.log");

     private Logger() { }

    /// <summary>
    ///     The Logger access point.
    /// </summary>
    public static Logger Instance => logger;

    /// <summary>
    ///     Method to log caught Exceptions.
    /// </summary>
    /// <param name="e">The exception thrown</param>
    public void LogException(Exception e) =>
        Log($"[{DateTime.Now}] [ERROR] {e.Message}{Environment.NewLine}   [STACKTRACE]{e.StackTrace}");

    /// <summary>
    ///     Log a warning message.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public void LogWarning(string message) =>
        Log($"[{DateTime.Now}] [WARN] {message}");

    /// <summary>
    ///     Log information.
    /// </summary>
    /// <param name="message">The information message.</param>
    public void LogInformation(string message) =>
        Log($"[{DateTime.Now}] [INFO] {message}");
    
    private void Log(string message)
    {
        try
        {
            File.AppendAllText(_path, message + Environment.NewLine + Environment.NewLine);
        }
        catch { }
    }

}