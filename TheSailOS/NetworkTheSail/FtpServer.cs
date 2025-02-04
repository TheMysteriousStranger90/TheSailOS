using System;
using Cosmos.System.FileSystem;

namespace TheSailOS.NetworkTheSail;

public class FtpServer : IDisposable
{
    private readonly FtpServer _ftpServer;
    private readonly string _rootDirectory;
    private bool _isRunning;
    
    public event EventHandler<string> ClientConnected;
    public event EventHandler<string> FileTransferred;
    public event EventHandler<Exception> ErrorOccurred;

    public FtpServer(CosmosVFS fileSystem, string rootDirectory)
    {
        _ftpServer = new FtpServer(fileSystem, rootDirectory);
        _rootDirectory = rootDirectory;
    }

    public void Start(int port = 21)
    {
        try
        {
            _isRunning = true;
            _ftpServer.Start();
            OnStatusChanged($"FTP Server started on port {port}");
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _ftpServer.Dispose();
        OnStatusChanged("FTP Server stopped");
    }

    protected virtual void OnClientConnected(string clientInfo)
    {
        ClientConnected?.Invoke(this, clientInfo);
    }

    protected virtual void OnFileTransferred(string fileInfo)
    {
        FileTransferred?.Invoke(this, fileInfo);
    }

    protected virtual void OnError(Exception ex)
    {
        ErrorOccurred?.Invoke(this, ex);
    }

    protected virtual void OnStatusChanged(string status)
    {
        Console.WriteLine($"[FTP] {status}");
    }

    public void Dispose()
    {
        Stop();
    }
}