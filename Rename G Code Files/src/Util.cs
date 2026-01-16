namespace Rename_G_Code_Files.src;

internal static class Util
{
    public static T GetRegistryValue<T>(string keyPath, string value, T ifFailOrNull)
    {
        try
        {
            return Registry.GetValue(keyPath, value, null) >>>
                (o => o is null
                    ? ifFailOrNull
                    : (T)Convert.ChangeType(o, typeof(T)));
        }
        catch
        {
            return ifFailOrNull;
        }
    }

    public static T GetRegistryValue<T>(string keyPath, string value, Func<T> ifFailOrNull)
    {
        try
        {
            return Registry.GetValue(keyPath, value, null) >>>
                (o => o is null
                    ? ifFailOrNull()
                    : (T)Convert.ChangeType(o, typeof(T)));
        }
        catch
        {
            return ifFailOrNull();
        }
    }

    public static Func<string,string> asDirectory = path =>
        path.EndsWith('\\')
            ? path
            : path + "\\";

    public static string UserSelectFolder(string reason = "")
    {
        if (reason is not "")
        {
            MessageBox.Show(reason);
        }
        var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (Directory.Exists(dialog.SelectedPath))
            {
                return dialog.SelectedPath;
            }
            else
            {
                MessageBox.Show("The directory you selected is invalid!");
                return UserSelectFolder(reason);
            }
        }
        else
        {
            return UserSelectFolder(reason);
        }
    }
}