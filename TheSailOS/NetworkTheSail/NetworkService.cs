using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4;

namespace TheSailOS.NetworkTheSail;

public class NetworkService
{
    private readonly Dictionary<string, IDisposable> _activeConnections;
    private static NetworkService _instance;
    private Action<string> _statusCallback;

    public static NetworkService Instance => _instance ??= new NetworkService();

    private NetworkService()
    {
        _activeConnections = new Dictionary<string, IDisposable>();
    }

    public void SetStatusCallback(Action<string> callback)
    {
        _statusCallback = callback;
    }

    public bool Initialize(bool useDhcp = true)
    {
        try
        {
            NotifyStatus("Starting network initialization...");
        
            if (!NetworkManager.Initialize(NotifyStatus))
            {
                NotifyStatus("Network initialization failed");
                return false;
            }

            if (NetworkManager.IsNetworkAvailable())
            {
                NotifyStatus($"Network initialized, IP: {NetworkManager.GetCurrentIP()}");
                return true;
            }

            NotifyStatus("Network not available after initialization");
            return false;
        }
        catch (Exception ex)
        {
            NotifyStatus($"Fatal error: {ex.Message}");
            return false;
        }
    }

    private void NotifyStatus(string status)
    {
        _statusCallback?.Invoke(status);
    }

    public UserTcpClient CreateTcpConnection(string id, string ip, int port)
    {
        try
        {
            var ipAddress = Address.Parse(ip);
            var client = new UserTcpClient(ipAddress, port);
            _activeConnections.Add(id, client);
            NotifyStatus($"TCP connection created to {ip}:{port}");
            return client;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public UserUdpClient CreateUdpConnection(string id, int localPort, Address remoteAddress, ushort remotePort)
    {
        try
        {
            var client = new UserUdpClient(localPort, remoteAddress, remotePort);
            _activeConnections.Add(id, client);
            return client;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void CloseConnection(string id)
    {
        if (_activeConnections.TryGetValue(id, out var connection))
        {
            connection.Dispose();
            _activeConnections.Remove(id);
        }
    }
}