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
        args = ["2025", "C:\\_Cabinet Vision\\S2M Output_temp"]; // for debugging

        Logger.Instance.LogInformation($"Called with args {string.Join(',', args)}");

        // Parse the command line arguments, asking for user input on failure.
        (int version, string outputPath) = ParseArgs(args)
            .IfFail(e =>
            {
                WriteLine(e);
                int version = Prompt("Enter your CV version",
                    s => (int.TryParse(s, out int r) && r is > 2020 and < 2027, r));
                return (version, UserSelectFolder("Select the folder of output G Code files"));
            });

        Run run = DatabaseFactory.GetDatabase(version)
            .GetRunInfo()
            .IfFail(e =>
            {
                return new Run()
                {
                    RunTag = "R00",
                    OutputTime = DateTime.Now
                };
            }) >>> (result => result with
            {
                JobStateOutputPath = GetRegistryValue($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\S2M {version}", "SNCPath", string.Empty),
                GCodeOutputPath = outputPath
            });

        int jobCount = run.Jobs.Count();
        switch (jobCount)
        {
            case 0:
                string jobFolder = UserSelectFolder("No Jobs exist in the database! Please select a Job Folder")
                    >>> (s => s.EndsWith("G Codes") ? s : s + "\\G Codes");
                    var job = new Job(Path.GetFileName(jobFolder.TrimEnd('\\')),jobFolder);
                run.CurrentJob = job;
                break;
            case > 1:
                WriteLine("Multiple Jobs Exist! Select a Job:");
                run.Jobs.Iter((i, j) => WriteLine("{0} - {1}", ++i, j.Name));
                ConsoleKeyInfo key;
                do
                {
                    key = ReadKey();
                }
                while (key.KeyChar < '1' || key.KeyChar > jobCount + '0');
                run.CurrentJob = run.Jobs.ToList()[key.KeyChar - '0' - 1];
                break;
            case 1:
                run.CurrentJob = run.Jobs.First();
                break;
        }
        
        new FileHandler(run).MoveAllOutputFiles();

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

    /// <summary>
    /// Prompt the user to input data through the command line.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prompt"></param>
    /// <returns></returns>
    private static T Prompt<T>(string prompt, Func<string, (bool succeeded, T result)> converter)
    {
        WriteLine(prompt);
        var input = ReadLine();

        if (input is null or "")
            return Prompt(prompt, converter);

        var (succeeded, result) = converter(input);
        return succeeded
            ? result
            : Prompt("Invalid input! Please try again.", converter);
    }

    public static void Die(string reason)
    {
        MessageBox.Show(reason, "Rename G Code Files Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(1);
    }
}

