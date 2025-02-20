using System;
using TheSailOSProject.Network.UDP;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Network
{
    public class UdpServerCommand : ICommand
    {
        public void Execute(string[] args)
        {
            try
            {
                int port = args.Length > 0 ? int.Parse(args[0]) : 8080;
                ConsoleManager.WriteLineColored($"Starting UDP server on port {port}...", ConsoleStyle.Colors.Primary);
                TheSailUdpServer.StartUdpServer(port);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error starting UDP server: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        public string HelpText()
        {
            return "Starts a UDP server.\n" +
                   "Usage: udpserver [port]\n" +
                   "Default port: 8080";
        }
    }
}