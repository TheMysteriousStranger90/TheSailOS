using Cosmos.HAL;
using Cosmos.System.Network.IPv4;

namespace TheSailOS.NetworkTheSail;
public class NetworkStatus
{
    public bool IsConnected { get; set; }
    public string DeviceName { get; set; }
    public CardType DeviceType { get; set; }
    public string MACAddress { get; set; }
    public int AvailableBytes { get; set; }
    public bool SendBufferFull { get; set; }
    public bool ReceiveBufferFull { get; set; }

    public override string ToString()
    {
        return $"Network Device Status:\n" +
               $"Name: {DeviceName}\n" +
               $"Type: {DeviceType}\n" +
               $"MAC Address: {MACAddress}\n" +
               $"Connected: {IsConnected}\n" +
               $"Available Bytes: {AvailableBytes}\n" +
               $"Send Buffer Full: {SendBufferFull}\n" +
               $"Receive Buffer Full: {ReceiveBufferFull}";
    }
}