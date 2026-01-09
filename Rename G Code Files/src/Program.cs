namespace Rename_G_Code_Files.src;

internal class Program
{
    /// <summary>
    ///     Program Entry Point. Called from machine script in CV. 
    /// </summary>
    /// <remarks>
    ///     <list type="bullet"> 
    ///         <item>Arg 0: CV Version</item>
    ///         <item>Arg 1: Output Path</item>
    ///     </list>
    /// </remarks>
    /// <param name="args">
    ///     A list of the command line args
    /// </param>
    [STAThread]
    private static void Main(string[] args)
    {
        //args = ["2023", "C:\\_Cabinet Vision\\S2M Output_temp"];

        Logger.Instance.LogInformation($"Called with args {string.Join(',', args)}");

        // Parse the command line arguments.
        int version = 0;
        string outputPath = string.Empty;
        ParseArgs(args)
            .Match(
                Succ: result => (version, outputPath) = result,
                Fail: e =>
                {
                    string message = e.Message + Environment.NewLine +
                        "Call Program with args[0] = CV version, and arg[1] = CV output path.";
                    Die(message);
                }
            );

        Run run = DatabaseFactory
            .GetDatabase(version)
            .GetRunInfo()
            .IfFail(_ => new Run()
                {
                    RunTag = "R00",
                    OutputTime = DateTime.Now
                }) >>> (result => result with
                {
                    JobStateOutputPath = GetRegistryValue($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\S2M {version}", "SNCPath", string.Empty),
                    GCodeOutputPath = outputPath
                });

        FileHandler.MoveAllOutputFiles(in run);

        Logger.Instance.LogData(run);
        Environment.Exit(0);
    }

    /// <summary>
    /// Parse a <see cref="string[]"/> into the CV version and GCode output path.
    /// </summary>
    /// <param name="args">The command line args.</param>
    /// <returns>A <see cref="Fin{T}"/> indicating the success or failure of the parsing.</returns>
    private static Fin<(int version, string path)> ParseArgs(string[] args) =>
        args.Length < 2
            ? Fin<(int, string)>.Fail("Insufficient arguments provided!")
            : !int.TryParse(args[0], out int v) || !Directory.Exists(args[1])
                ? Fin<(int, string)>.Fail($"Arguments \"{string.Join(" ", args)}\" are invalid!")
                : Fin<(int, string)>.Succ((v, args[1]));

    public static void Die(string reason)
    {
        MessageBox.Show(reason, "Rename G Code Files Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(1);
    }
}

