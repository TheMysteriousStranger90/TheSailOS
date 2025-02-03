using Cosmos.System.FileSystem;

namespace TheSailOS.FileSystemTheSail;

public class FileTheSail
{
    protected internal CosmosVFS _vfs;

    public FileTheSail()
    {
        this._vfs = new CosmosVFS();
    }
}