using Cosmos.System.FileSystem;

namespace TheSailOS.FileSystem;

public class FileTheSail
{
    protected internal CosmosVFS _vfs;

    public FileTheSail()
    {
        this._vfs = new CosmosVFS();
    }
}