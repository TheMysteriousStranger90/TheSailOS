using System;
using TheSailOSProject.Network.TCP;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Network
{
    public class TcpServerCommand : ICommand
    {
        public void Execute(string[] args)
        {
            try
            {
                ConsoleManager.WriteLineColored("Starting TCP server...", ConsoleStyle.Colors.Primary);
                TheSailTcpServer.StartTcpServer();
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error starting TCP server: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        public string HelpText()
        {
            return "Starts a TCP server listening on port 80.\nUsage: tcpserver";
        }
    }
}