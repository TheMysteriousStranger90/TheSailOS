using System;
using Cosmos.System.Network.IPv4;

namespace TheSailOS.NetworkTheSailTwo;

public class PingClient
{
    public static int Ping(string ipAddress)
    {
        try
        {
            Address address = Address.Parse(ipAddress);
            using (var xClient = new ICMPClient())
            {
                xClient.Connect(address);
                xClient.SendEcho();
                EndPoint endpoint = new EndPoint(Address.Zero, 0);
                int time = xClient.Receive(ref endpoint);
                return time;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] ICMP Ping failed: {ex.Message}");
            return -1;
        }
    }
}