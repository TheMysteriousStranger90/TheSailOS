namespace TheSailOSProject.FileSystem;

public interface IVFSManager
{
    long GetAvailableFreeSpace(string drive);
    string GetFileSystemType(string drive);
}