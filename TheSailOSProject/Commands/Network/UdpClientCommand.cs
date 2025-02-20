using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using TheSailOSProject.Network.UDP;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Network
{
    public class UdpClientCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 4)
            {
                ConsoleManager.WriteLineColored(
                    "Usage: udpclient <destination_ip> <destination_port> <local_port> <message> [timeout]",
                    ConsoleStyle.Colors.Warning);
                return;
            }

            try
            {
                string[] ipParts = args[0].Split('.');
                if (ipParts.Length != 4)
                {
                    throw new ArgumentException("Invalid IP address format");
                }

                byte[] ipBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    ipBytes[i] = byte.Parse(ipParts[i]);
                }

                Address destIp = new Address(ipBytes[0], ipBytes[1], ipBytes[2], ipBytes[3]);
                int destPort = int.Parse(args[1]);
                int localPort = int.Parse(args[2]);
                string message = args[3];
                int timeout = args.Length > 4 ? int.Parse(args[4]) : 80;

                ConsoleManager.WriteLineColored($"Connecting to {args[0]}:{destPort}...", ConsoleStyle.Colors.Primary);

                byte[] response = TheSailUdpClient.UdpClientConnect(destIp, destPort, localPort, message, timeout);

                if (response != null && response.Length > 0)
                {
                    string responseText = Encoding.ASCII.GetString(response);
                    ConsoleManager.WriteLineColored("Response received:", ConsoleStyle.Colors.Success);
                    Console.WriteLine(responseText);
                }
                else
                {
                    ConsoleManager.WriteLineColored("No response received", ConsoleStyle.Colors.Warning);
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        public string HelpText()
        {
            return "Connects to a UDP server and sends a message.\n" +
                   "Usage: udpclient <destination_ip> <destination_port> <local_port> <message> [timeout]\n" +
                   "Example: udpclient 192.168.1.100 8080 9000 \"Hello Server\"";
        }
    }
}