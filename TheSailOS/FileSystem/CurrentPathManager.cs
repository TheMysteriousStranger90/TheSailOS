namespace TheSailOS.FileSystem;

public static class CurrentPathManager
{
    public static string CurrentDirectory => Kernel.CurrentDirectory;
    public static string UserDirectory { get; private set; } = @"L:\Users\Default\";

    public static bool Set(string dir, out string error)
    {
        error = "none";
        switch (dir)
        {
            case "..":
                var parentDir = System.IO.Path.GetDirectoryName(Kernel.CurrentDirectory.TrimEnd('\\'));
                if (!string.IsNullOrEmpty(parentDir) && Kernel._fileTheSail._vfs.GetDirectory(parentDir) != null)
                {
                    Kernel.SetCurrentDirectory(parentDir);
                }
                else
                {
                    error = "Cannot move further up the directory tree.";
                    return false;
                }
                break;
            case "~":
                if (Kernel._fileTheSail._vfs.GetDirectory(UserDirectory) != null)
                {
                    Kernel.SetCurrentDirectory(UserDirectory);
                }
                else
                {
                    error = "User directory does not exist!";
                    return false;
                }
                break;
            default:
                var newPath = System.IO.Path.Combine(Kernel.CurrentDirectory, dir);
                if (Kernel._fileTheSail._vfs.GetDirectory(newPath) != null)
                {
                    Kernel.SetCurrentDirectory(newPath);
                }
                else
                {
                    error = "This directory doesn't exist!";
                    return false;
                }
                break;
        }
        return true;
    }
}