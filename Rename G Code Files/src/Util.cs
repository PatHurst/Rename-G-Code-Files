using System.CodeDom;
using Microsoft.Win32;
namespace Rename_G_Code_Files.src;

internal static class Util
{
    public static T GetRegistryValue<T>(string keyPath, string value)
    {
        object? read = Registry.GetValue(keyPath, value, null);
        if (read is null)
            return default!;
        if (typeof(T) == typeof(int))
            return (T)(object)Convert.ToInt32(read);
        return read is null ? default! : (T)read!;
    }

    public static string Right(string original, int characters)
    {
        return original[^characters..];
    }
}