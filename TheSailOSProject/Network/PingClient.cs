using System;
using Cosmos.System.Network.IPv4;

namespace TheSailOSProject.Network;

public class PingClient
{
    private const int PING_COUNT = 4;
    private const int TIMEOUT_MS = 5000;

    public bool Ping(string target, int count = PING_COUNT)
    {
        if (!NetworkManager.IsInitialized)
        {
            Console.WriteLine("[ERROR] Network is not initialized.");
            return false;
        }

        if (Cosmos.HAL.NetworkDevice.Devices.Count == 0)
        {
            Console.WriteLine("[ERROR] No usable network devices found!");
            return false;
        }

        Address targetAddress;

        if (IsIPAddress(target))
        {
            try
            {
                targetAddress = Address.Parse(target);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Invalid IP address: {ex.Message}");
                return false;
            }
        }
        else
        {
            Console.WriteLine($"[INFO] Resolving hostname '{target}'...");
            targetAddress = DnsResolver.GetAddress(target);

            if (targetAddress == null)
            {
                Console.WriteLine($"[ERROR] Could not resolve hostname '{target}'");
                return false;
            }
        }

        return PingIP(targetAddress, count);
    }

    private bool PingIP(Address address, int count)
    {
        int successful = 0;
        int failed = 0;
        int totalTime = 0;
        int minTime = int.MaxValue;
        int maxTime = 0;

        Console.WriteLine($"\nPinging {address} with 32 bytes of data:\n");

        ICMPClient icmpClient = null;

        try
        {
            icmpClient = new ICMPClient();
            icmpClient.Connect(address);

            for (int i = 0; i < count; i++)
            {
                try
                {
                    icmpClient.SendEcho();

                    EndPoint endPoint = new EndPoint(Address.Zero, 0);
                    int responseTime = icmpClient.Receive(ref endPoint, TIMEOUT_MS);

                    if (responseTime >= 0 && responseTime < TIMEOUT_MS)
                    {
                        Console.WriteLine($"Reply from {address}: bytes=32 time={responseTime}ms TTL=128");

                        successful++;
                        totalTime += responseTime;

                        if (responseTime < minTime) minTime = responseTime;
                        if (responseTime > maxTime) maxTime = responseTime;
                    }
                    else
                    {
                        Console.WriteLine($"Request timed out.");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Request failed: {ex.Message}");
                    failed++;
                }

                if (i < count - 1)
                {
                    Cosmos.HAL.Global.PIT.Wait(1000);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Ping operation failed: {ex.Message}");
            return false;
        }
        finally
        {
            try
            {
                icmpClient?.Close();
            }
            catch
            {
            }
        }

        Console.WriteLine($"\nPing statistics for {address}:");
        Console.WriteLine(
            $"    Packets: Sent = {count}, Received = {successful}, Lost = {failed} ({(failed * 100.0 / count):F0}% loss)");

        if (successful > 0)
        {
            int avgTime = totalTime / successful;
            Console.WriteLine("Approximate round trip times in milli-seconds:");
            Console.WriteLine($"    Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {avgTime}ms");
            return true;
        }

        return false;
    }

    private bool IsIPAddress(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        string[] parts = input.Split('.');

        if (parts.Length != 4)
            return false;

        foreach (string part in parts)
        {
            if (!byte.TryParse(part, out byte value))
                return false;
        }

        return true;
    }
}