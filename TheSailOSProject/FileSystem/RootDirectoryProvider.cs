namespace TheSailOSProject.FileSystem;

public class RootDirectoryProvider : IRootDirectoryProvider
{
    private readonly string _rootDirectory;

    public RootDirectoryProvider(string rootDirectory = @"0:\")
    {
        _rootDirectory = rootDirectory;
    }

    public string GetRootDirectory()
    {
        return _rootDirectory;
    }
}