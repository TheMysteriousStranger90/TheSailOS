using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;

namespace TheSailOSProject.Network.UDP
{
    public class TheSailUdpClient
    {
        public static byte[] UdpClientConnect(Address destip, int destport, int localport, string message, int timeout = 80)
        {
            try
            {
                var client = new UdpClient(localport);
                client.Connect(destip, destport);

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                client.Send(messageBytes);
                
                var endpoint = new EndPoint(Address.Zero, 0);
                var data = client.Receive(ref endpoint);
                var data2 = client.NonBlockingReceive(ref endpoint); 
                
                client.Close();

                return data2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Client error: {ex.Message}");
                return null;
            }
        }
    }
}