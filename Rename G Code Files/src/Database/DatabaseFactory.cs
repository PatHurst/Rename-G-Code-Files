

namespace Rename_G_Code_Files.src.Database
{
    /// <summary>
    ///     Factory for creating database instances. 
    /// </summary>
    /// <remarks>
    ///     Creates connections to both the PSNC-NC and PSNC-CV databases. Compares the last modified dates and return the 
    ///     last modified database.
    /// </remarks>
    internal static class DatabaseFactory
    {
        /// <summary>
        ///     Return a database object.
        /// </summary>
        /// <param name="version">The CV version.</param>
        /// <returns>A <see cref="Database">Database</see> object</returns>
        public static Database GetDatabase(int version)
        {
            string cvPath = GetDatabasePaths(version).cvPath;
            string ncPath = GetDatabasePaths(version).ncPath;
            Database cvConnection;
            Database ncConnection;
            if (version <= 2023)
            {
                cvConnection = new OleDbDatabase(GenerateConnString(cvPath, version), cvPath);
                ncConnection = new OleDbDatabase(GenerateConnString(ncPath, version), ncPath);
            }
            else
            {
                cvConnection = new SqlLocalDatabase(GenerateConnString(cvPath, version), cvPath);
                ncConnection = new SqlLocalDatabase(GenerateConnString(ncPath, version), ncPath);
            }
            return cvConnection.GetLastModifiedDate() > ncConnection.GetLastModifiedDate() ? cvConnection : ncConnection;
        }

        /// <summary>
        ///     Generate the connection string for the database.
        /// </summary>
        /// <param name="databaseName">The path to the database file.</param>
        /// <param name="version">The CV version.</param>
        /// <returns>The path to the database file.</returns>
        private static string GenerateConnString(string databaseName, int version)
        {
            return version switch
            {
                <= 2023 => $"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={databaseName};",
                _ => $"Server=(localdb)\\CV{version % 100}; Database={databaseName}; Trusted_Connection=True;"
            };
        }

        /// <summary>
        ///     Get the database paths from the registry.
        /// </summary>
        /// <param name="version">The CV version.</param>
        /// <returns>The PSNC-NC and PSNC-CV paths.</returns>
        private static (string cvPath, string ncPath) GetDatabasePaths(int version)
        {
            string userPath = Util.GetRegistryValue<string>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "UserPath");
            string currentUser = Util.GetRegistryValue<string>($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\Common {version}", "CurrentUser");
            int netRights = Util.GetRegistryValue<int>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "NetRights");
            string databasePath = (netRights == 1) switch
            {
                false => Util.GetRegistryValue<string>($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\S2M {version}", "DBasePath"),
                true => userPath + currentUser + (currentUser.EndsWith('\\') ? string.Empty : '\\')
            };

            string psncCvPath = version < 2024 ? $"{databasePath}psnc-cv.accdb" : $"S2M-{databasePath}psnc-cv.mdf";
            string psncNcPath = version < 2024 ? $"{databasePath}psnc-nc.accdb" : $"S2M-{databasePath}psnc-nc.mdf";
            return (psncCvPath, psncNcPath);
        }
    }
}