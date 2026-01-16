using Microsoft.Identity.Client.Extensions.Msal;

namespace Rename_G_Code_Files.src;

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
        string cvPath = string.Empty, ncPath = string.Empty;
        _ = GetDatabasePaths(version)
            .Match(
                Succ: result => (cvPath, ncPath) = result,
                Fail: e => Program.Die(e.Message)
            );

        Database createDatabase(string c, string p) => 
            version <= 2023
                ? new OleDbDatabase(c, p)
                : new SqlLocalDatabase(c, p);

        var psncCV = createDatabase(GetConnectionString(cvPath, version), cvPath);
        var psncNc = createDatabase(GetConnectionString(ncPath, version), ncPath);

        return psncCV.GetLastModifiedDate() > psncNc.GetLastModifiedDate()
            ? psncCV
            : psncNc;
    }

    /// <summary>
    ///     Get the database paths from the registry.
    /// </summary>
    /// <param name="version">The CV version.</param>
    /// <returns>The PSNC-NC and PSNC-CV paths.</returns>
    private static Fin<(string cvPath, string ncPath)> GetDatabasePaths(int version)
    {
        string databasePath = string.Empty;
        int netRights = GetRegistryValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "NetRights", 1);
        switch (netRights == 1)
        {
            case true:
                string userPath = GetRegistryValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {version}", "UserPath", string.Empty);
                string currentUser = GetRegistryValue($"HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION\\Common {version}", "CurrentUser", "Administrator");
                databasePath = userPath + currentUser >>> asDirectory;
                break;
            case false:
                databasePath = GetRegistryValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\S2M {version}", "DBasePath", string.Empty) >>> asDirectory;
                break;
        }
        return !Directory.Exists(databasePath)
            ? Fin<(string,string)>.Fail($"Could not find databases in \"{databasePath}\"!")
            : version <= 2023
                ? (Path.Combine(databasePath, "psnc-cv.accdb"), Path.Combine(databasePath, "psnc-nc.accdb"))
                : (Path.Combine($"S2M-{databasePath}", "psnc-cv.mdf"), Path.Combine($"S2M-{databasePath}", "psnc-nc.mdf"));
    }

    private static string GetConnectionString(string databaseName, int version) => version switch
    {
        <= 2023 => $"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={databaseName};",
        _ => $"Server=(localdb)\\CV{version % 100}; Database={databaseName}; Integrated Security=True;"
    };
}
