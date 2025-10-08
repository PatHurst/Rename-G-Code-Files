using System;
using System.Data.Common;
using Microsoft.Win32;
using Rename_G_Code_Files.src;

namespace Rename_G_Code_Files.Database
{
    internal class DatabaseFactory
    {
        public static Database GetDatabase(Job job)
        {
            string cvPath = GetDatabasePaths(job.CVVersion).cvPath;
            string ncPath = GetDatabasePaths(job.CVVersion).ncPath;
            Database cvConnection;
            Database ncConnection;
            if (job.CVVersion == 2023)
            {
                cvConnection = new OleDbDatabase(GenerateConnString(cvPath, job.CVVersion), cvPath);
                ncConnection = new OleDbDatabase(GenerateConnString(ncPath, job.CVVersion), ncPath);
            }
            else
            {
                cvConnection = new SqlLocalDatabase(GenerateConnString(cvPath, job.CVVersion), cvPath);
                ncConnection = new SqlLocalDatabase(GenerateConnString(ncPath, job.CVVersion), ncPath);
            }
            if (cvConnection.GetLastModifiedDate() > ncConnection.GetLastModifiedDate())
                return cvConnection;
            else
                return ncConnection;
        }

        private static string GenerateConnString(string databaseName, int version)
        {
            return version switch
            {
                2023 => $"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={databaseName};",
                > 2023 => $"Server=(localdb)\\CV{version % 100}; Database={databaseName}; Trusted_Connection=True;",
                _ => $"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={databaseName};",
            };
        }

        private static (string cvPath, string ncPath) GetDatabasePaths(int version)
        {
            string userPath = Util.GetRegistryValue<string>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "UserPath");
            string currentUser = Util.GetRegistryValue<string>($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\Common {version}", "CurrentUser");
            string dBasePath = Util.GetRegistryValue<string>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\S2M {version}", "DBasePath");
            int netRights = Util.GetRegistryValue<int>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "NetRights");
            string databasePath = (netRights == 1) switch
            {
                false => dBasePath,
                true => userPath + currentUser + (currentUser.EndsWith('\\') ? string.Empty : '\\')
            };
            string psncCvPath = version < 2024 ? $"{databasePath}psnc-cv.accdb" : $"S2M-{databasePath}psnc-cv.mdf";
            string psncNcPath = version < 2024 ? $"{databasePath}psnc-nc.accdb" : $"S2M-{databasePath}psnc-nc.mdf";
            return (psncCvPath, psncNcPath);
        }
    }
}