namespace Rename_G_Code_Files.src;
internal class Program
{
    private static void Main(string[] args)
    {
        Job job = new();
        FileHandler fileHandler = new(job);
        fileHandler.MoveAllOutputFiles();
        Logger.Logger.LogRunData(job);
        Environment.Exit(0);
    }
}

