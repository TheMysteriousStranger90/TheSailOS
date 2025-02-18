using System;
using Cosmos.System.Network.IPv4;

namespace TheSailOSProject.Network;

public class PingClient
{
    private const int PING_COUNT = 4;

    public void PingIP(string ipAddress)
    {
        float successful = 0;

        if (Cosmos.HAL.NetworkDevice.Devices.Count == 0)
        {
            Console.WriteLine("There aren't any usable network devices installed!");
            return;
        }

        try
        {
            Console.WriteLine($"Pinging \"{ipAddress}\"...");
            EndPoint endPoint = new EndPoint(Address.Zero, 0);

            using (var icmpClient = new ICMPClient())
            {
                icmpClient.Connect(Address.Parse(ipAddress));

                for (int i = 0; i < PING_COUNT; i++)
                {
                    try
                    {
                        icmpClient.SendEcho();
                        int time = icmpClient.Receive(ref endPoint);
                        if (time >= 0)
                        {
                            Console.WriteLine($"Response received in {time} millisecond(s)");
                            successful++;
                        }
                        else
                        {
                            Console.WriteLine("Ping failed.");
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Ping failed.");
                    }
                }
            }

            Console.WriteLine(
                $"Success rate: {(successful / PING_COUNT * 100):F0} percent. ({successful}/{PING_COUNT})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ping operation failed: {ex.Message}");
        }
    }
}