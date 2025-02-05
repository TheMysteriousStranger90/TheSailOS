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
            var device = NetworkDeviceManager.GetPrimaryDevice();
            if (device == null)
            {
                NotifyStatus("No network device available");
                return false;
            }

            NotifyStatus($"Using device: {device.Name}");

            if (!device.Ready)
            {
                NotifyStatus("Network device not ready");
                return false;
            }

            if (useDhcp)
            {
                if (!NetworkManager.Initialize(NotifyStatus))
                {
                    NotifyStatus("DHCP configuration failed");
                    return false;
                }
            }
            else
            {
                var defaultIp = new Address(192, 168, 1, 100);
                var defaultMask = new Address(255, 255, 255, 0);
                var defaultGateway = new Address(192, 168, 1, 1);
                NetworkManager.ConfigureManual(defaultIp, defaultMask, defaultGateway);
            }

            NotifyStatus("Network initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            NotifyStatus($"Network initialization failed: {ex.Message}");
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