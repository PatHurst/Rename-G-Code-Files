

using System.Diagnostics;
using Rename_G_Code_Files.src;

namespace Rename_G_Code_Files
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Job job = new();
            FileHandler fileHandler = new(job);
            fileHandler.MoveAllOutputFiles();
            Logger.LogRunData(job);
            Environment.Exit(0);
        }
    }
}

