using System.IO;

namespace TheSailOS.FileSystemTheSail;

public static class CurrentPathManager
{
    public static string CurrentDirectory => Kernel.CurrentDirectory;
    public static string UserDirectory { get; private set; } = @"0:\Users\Default\";

    public static bool Set(string dir, out string error)
    {
        error = "none";
        switch (dir)
        {
            case ".." or "back":
                if (Kernel.CurrentDirectory.TrimEnd('\\') == "0:")
                {
                    error = "Already at root directory";
                    return false;
                }

                var parentDir = Path.GetDirectoryName(Kernel.CurrentDirectory.TrimEnd('\\'));
                if (!string.IsNullOrEmpty(parentDir) && Kernel.CurrentFileTheSail._vfs.GetDirectory(parentDir) != null)
                {
                    Kernel.SetCurrentDirectory(parentDir);
                    return true;
                }

                error = "Cannot move further up the directory tree.";
                return false;

            case "~":
                if (Kernel.CurrentFileTheSail._vfs.GetDirectory(UserDirectory) != null)
                {
                    Kernel.SetCurrentDirectory(UserDirectory);
                    return true;
                }

                error = "User directory does not exist!";
                return false;

            default:
                var newPath = Path.Combine(Kernel.CurrentDirectory, dir);
                if (Kernel.CurrentFileTheSail._vfs.GetDirectory(newPath) != null)
                {
                    Kernel.SetCurrentDirectory(newPath);
                    return true;
                }

                error = "This directory doesn't exist!";
                return false;
        }
    }
}