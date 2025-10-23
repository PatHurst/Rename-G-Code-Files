using Microsoft.Win32;
namespace Rename_G_Code_Files;

internal static class Util
{
    public static T GetRegistryValue<T>(string keyPath, string value)
    {
        object? regRead = Registry.GetValue(keyPath, value, null);
        if (regRead != null)
        {
            return (T)regRead;
        }
        else
        {
            throw new ArgumentException($"Registry Value {keyPath}\\{value} could not be read!");
        }
    }

    public static string Right(string original, int characters)
    {
        return original[^characters..];
    }
}