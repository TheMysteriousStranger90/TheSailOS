namespace TheSailOSProject.FileSystem;

public interface IVFSManager
{
    long GetAvailableFreeSpaceTheSail(string drive);
    string GetFileSystemTypeTheSail(string drive);
}