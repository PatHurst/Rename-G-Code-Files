using Rename_G_Code_Files.src.Database;

namespace Rename_G_Code_Files.src;

internal class Program
{
    /// <summary>
    ///     Program Entry Point.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet"> 
    ///         <item>Arg 0: Executing file's name</item>
    ///         <item>Arg 1: CV Version</item>
    ///         <item>Arg 2: Output Path</item>
    ///     </list>
    /// </remarks>
    /// <param name="args">
    ///     A list of the command line args
    /// </param>
    private static void Main(string[] args)
    {
        //args = ["2025", "C:\\_Cabinet Vision\\S2M Output_temp"];
        int version = 0;
        string outputPath = string.Empty;
        try
        {
            (version, outputPath) = ParseArgs(args);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadKey();
            Environment.Exit(1);
        }
        
        Database.Database database = DatabaseFactory.GetDatabase(version);
        Run run = database.GetRunInfo();
        run.JobStateOutputPath = Util.GetRegistryValue<string>($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\S2M {version}", "SNCPath");

        if (!run.Jobs.Any())
        {
            MessageBox.Show("No Jobs were found in database!\r\nSelect a job Folder.");
            FolderBrowserDialog fb = new();
            var result = fb.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = fb.SelectedPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!path.EndsWith("G Codes"))
                {
                    path += "\\G Codes";
                }
                run.DestinationPath = path + run.RunTag;
            }
            else
            {
                MessageBox.Show("You did not select a folder! System will now exit...");
                Logger.Instance.LogWarning("User did not select a desination folder.");
            }
        }
        else if (run.Jobs.Count() > 1)
        {
            int i = 1;
            Console.WriteLine("Select a Job:");
            foreach (var job in run.Jobs)
                Console.WriteLine("{0} - {1}", i++, job.Name);
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            }
            while (key.KeyChar < 1 || key.KeyChar > run.Jobs.Count());
            run.DestinationPath = run.Jobs.ToList()[key.KeyChar].GCodePath + run.RunTag;
            run.CurrentJob = run.Jobs.ToList()[key.KeyChar];
        }
        else
        {
            run.CurrentJob = run.Jobs.First();
            run.DestinationPath = run.Jobs.First().GCodePath + run.RunTag;
        }

        FileHandler fileHandler= new(run);
        fileHandler.MoveAllOutputFiles();

        Logger.Instance.LogData(run);
        Environment.Exit(0);
    }

    private static (int version, string path) ParseArgs(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Insufficient arguments provided!");
        }
        if (int.TryParse(args[0], out int v) && !string.IsNullOrEmpty(args[1]))
        {
            return (v, args[1]);
        }
        else
        {
            throw new ArgumentException(string.Format("Could not parse arguments! {0} {1}" + Environment.NewLine + "Expected 0:int 1:string", args[0], args[1]));
        }
    }
}

