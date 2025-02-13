using System;
using Cosmos.System.Network.IPv4;

namespace TheSailOSProject.Network;

public class PingClient
{
    private const int PING_TIMEOUT = 5000;
    private const int PING_COUNT = 4;

    public static PingResult[] Ping(string ipAddress)
    {
        var results = new PingResult[PING_COUNT];

        try
        {
            Console.WriteLine($"Pinging {ipAddress} with 32 bytes of data:");
            Address address = Address.Parse(ipAddress);

            using (var icmpClient = new ICMPClient())
            {
                icmpClient.Connect(address);

                for (int i = 0; i < PING_COUNT; i++)
                {
                    try
                    {
                        var startTime = DateTime.Now;
                        icmpClient.SendEcho();
                        
                        EndPoint endpoint = new EndPoint(Address.Zero, 0);
                        int time = icmpClient.Receive(ref endpoint);

                        results[i] = new PingResult
                        {
                            Success = time >= 0,
                            Time = time,
                            Error = null
                        };
                    }
                    catch (Exception ex)
                    {
                        results[i] = new PingResult
                        {
                            Success = false,
                            Time = -1,
                            Error = ex.Message
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize ping: {ex.Message}");
            for (int i = 0; i < PING_COUNT; i++)
            {
                results[i] = new PingResult
                {
                    Success = false,
                    Time = -1,
                    Error = ex.Message
                };
            }
        }

        return results;
    }
}

public struct PingResult
{
    public bool Success;
    public int Time;
    public string Error;
}