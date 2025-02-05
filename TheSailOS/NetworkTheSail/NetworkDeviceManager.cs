using System;
using System.Collections.Generic;
using Cosmos.HAL;

namespace TheSailOS.NetworkTheSail;

public static class NetworkDeviceManager
{
    private const string DEFAULT_ADAPTER = "eth0";
    
    private static readonly string[] KnownAdapters = 
    {
        "wlan0",
        "eth0",
        "vmnet1",   
        "vmnet8",
        "veth0"
    };

    public static bool HasNetworkDevice()
    {
        return Cosmos.HAL.NetworkDevice.Devices.Count > 0;
    }

    public static NetworkDevice GetPrimaryDevice()
    {
        if (!HasNetworkDevice())
            throw new Exception("No network card detected");
        
        var wlanDevice = NetworkDevice.GetDeviceByName(DEFAULT_ADAPTER);
        if (wlanDevice != null && wlanDevice.Ready)
        {
            return wlanDevice;
        }
        
        foreach (var adapter in KnownAdapters)
        {
            var device = NetworkDevice.GetDeviceByName(adapter);
            if (device != null && device.Ready)
            {
                return device;
            }
        }
        
        var devices = GetAllDevices();
        foreach (var device in devices)
        {
            if (device.Ready)
            {
                return device;
            }
        }

        throw new Exception("No active network adapters found");
    }

    public static NetworkDevice GetWirelessDevice()
    {
        return GetDeviceByType(DEFAULT_ADAPTER) ?? 
               throw new Exception("No wireless adapter found");
    }

    public static NetworkDevice GetDeviceByType(string adapterType)
    {
        if (!HasNetworkDevice())
            throw new Exception("No network card detected");

        foreach (var device in GetAllDevices())
        {
            if (device.Name.Contains(adapterType) && device.Ready)
            {
                return device;
            }
        }

        return null;
    }

    public static List<NetworkDevice> GetAllDevices()
    {
        return Cosmos.HAL.NetworkDevice.Devices;
    }

    public static NetworkStatus GetDeviceStatus(NetworkDevice device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        return new NetworkStatus
        {
            IsConnected = device.Ready,
            DeviceName = device.Name,
            DeviceType = device.CardType,
            MACAddress = device.MACAddress.ToString(),
            AvailableBytes = device.BytesAvailable(),
            SendBufferFull = device.IsSendBufferFull(),
            ReceiveBufferFull = device.IsReceiveBufferFull()
        };
    }
}