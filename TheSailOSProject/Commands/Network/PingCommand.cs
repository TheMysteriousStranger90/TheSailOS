using System;
using Cosmos.System.Network.Config;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class PingCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ping <ip_address>");
            Console.WriteLine("Example: ping 8.8.8.8");
            return;
        }

        string ipAddress = args[0];
        Console.WriteLine($"Pinging {ipAddress}...");
        
        if (NetworkConfiguration.CurrentAddress == null)
        {
            Console.WriteLine("[ERROR] Network is not initialized. Please configure network first.");
            return;
        }
        
        Console.WriteLine($"[DEBUG] Current IP: {NetworkConfiguration.CurrentAddress}");

        var results = PingClient.Ping(ipAddress);
        int successCount = 0;
        int totalTime = 0;

        foreach (var result in results)
        {
            if (result.Success)
            {
                Console.WriteLine($"Reply from {ipAddress}: time={result.Time}ms");
                successCount++;
                totalTime += result.Time;
            }
            else
            {
                Console.WriteLine($"Request timed out: {result.Error}");
            }
        }
        
        Console.WriteLine("\nPing statistics:");
        Console.WriteLine($"    Packets: Sent = {results.Length}, Received = {successCount}, " +
                          $"Lost = {results.Length - successCount} " +
                          $"({(int)((results.Length - successCount) * 100.0 / results.Length)}% loss)");

        if (successCount > 0)
        {
            double avgTime = totalTime / (double)successCount;
            Console.WriteLine($"Approximate round trip times in milli-seconds:");
            Console.WriteLine($"    Average = {avgTime:F2}ms");
        }
    }
}