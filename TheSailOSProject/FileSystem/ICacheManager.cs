namespace TheSailOSProject.FileSystem;

public interface ICacheManager
{
    byte[] GetCachedFile(string path);
    void CacheFile(string path, byte[] content);
}