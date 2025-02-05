using System;
using System.Threading;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOS.NetworkTheSail;

public class DHCPClientManager
{
    private const int MAX_RETRIES = 3;
    private const int RETRY_DELAY_MS = 1000;

    private DHCPClient _dhcpClient;
    private NetworkDevice _device;
    private Action<string> _statusCallback;

    public DHCPClientManager(Action<string> statusCallback = null)
    {
        _statusCallback = statusCallback;
    }

    public bool Initialize()
    {
        try
        {
            if (!NetworkDeviceManager.HasNetworkDevice())
            {
                LogStatus("No network device found");
                return false;
            }

            _device = NetworkDeviceManager.GetPrimaryDevice();
            if (_device == null)
            {
                LogStatus("Failed to get primary device");
                return false;
            }

            LogStatus($"Using network device: {_device.Name}");
            _dhcpClient = new DHCPClient();

            for (int retry = 0; retry < MAX_RETRIES; retry++)
            {
                LogStatus($"DHCP attempt {retry + 1}/{MAX_RETRIES}...");
            
                try
                {
                    int result = _dhcpClient.SendDiscoverPacket();
                    LogStatus($"DHCP result code: {result}");

                    if (result > 0)
                    {
                        LogStatus("DHCP configuration successful");
                        return true;
                    }

                    if (retry < MAX_RETRIES - 1)
                    {
                        LogStatus($"Retrying in {RETRY_DELAY_MS}ms...");
                        Thread.Sleep(RETRY_DELAY_MS);
                    }
                }
                catch (Exception ex)
                {
                    LogStatus($"DHCP attempt failed: {ex.Message}");
                    if (retry < MAX_RETRIES - 1) continue;
                    return false;
                }
            }

            LogStatus("DHCP configuration failed");
            return false;
        }
        catch (Exception ex)
        {
            LogStatus($"Fatal DHCP error: {ex.Message}");
            return false;
        }
    }

    private void LogStatus(string message)
    {
        _statusCallback?.Invoke($"[DHCP] {message}");
    }
}