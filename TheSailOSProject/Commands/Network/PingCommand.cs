using System;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class PingCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ping <ip_address>");
            return;
        }

        string ipAddress = args[0];
        int time = PingClient.Ping(ipAddress);

        if (time >= 0)
        {
            Console.WriteLine($"Ping to {ipAddress} successful, time={time}ms");
        }
        else
        {
            Console.WriteLine($"Ping to {ipAddress} failed.");
        }
    }
}