using System;
using Cosmos.System.FileSystem;
using System.IO;
using System.Threading;
using Cosmos.System.FileSystem.VFS;

namespace TheSailOS.FileSystemTheSail;

public class FileTheSail
{
    protected internal CosmosVFS _vfs;
    private bool _isInitialized;

    public FileTheSail()
    {
        try
        {
            this._vfs = new CosmosVFS();

            Thread.Sleep(1000);
            VFSManager.RegisterVFS(_vfs);
            
            var available = _vfs.GetVolumes();
            if (available != null && available.Count > 0)
            {
                _isInitialized = true;
                Console.WriteLine("[INFO] Filesystem initialized successfully");
            }
            else
            {
                throw new IOException("No volumes available");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Filesystem initialization failed: {ex.Message}");
            _isInitialized = false;
            throw;
        }
    }
}