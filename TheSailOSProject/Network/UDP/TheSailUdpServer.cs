using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;

namespace TheSailOSProject.Network.UDP
{
    public class TheSailUdpServer
    {
        public static void StartUdpServer(int port = 8080)
        {
            try
            {
                var server = new UdpClient(port);
                Console.WriteLine($"UDP Server listening on port {port}");

                while (true)
                {
                    var endpoint = new EndPoint(Address.Zero, 0);
                    byte[] data = server.Receive(ref endpoint);

                    if (data != null && data.Length > 0)
                    {
                        string message = Encoding.ASCII.GetString(data);
                        Console.WriteLine($"Received from {endpoint}: {message}");
                        
                        string response = "Message received";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        
                        server.Send(responseBytes, endpoint.Address, endpoint.Port);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Server error: {ex.Message}");
            }
        }
    }
}