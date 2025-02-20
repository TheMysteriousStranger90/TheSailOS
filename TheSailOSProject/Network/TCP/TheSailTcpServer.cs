using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace TheSailOSProject.Network.TCP
{
    public static class TheSailTcpServer
    {
        public static void StartTcpServer()
        {
            try
            {
                var tcpListener = new TcpListener(80);
                tcpListener.Start();
                Console.WriteLine("TCP Server listening on port 80");

                while (true)
                {
                    var client = tcpListener.AcceptTcpClient();
                    if (client == null)
                    {
                        continue;
                    }

                    var endpoint = new EndPoint(Address.Zero, 0);
                    byte[] data = client.Receive(ref endpoint);
                    
                    if (data != null && data.Length > 0)
                    {
                        string request = Encoding.ASCII.GetString(data);
                        Console.WriteLine($"Received: {request}");

                        string response = "HTTP/1.1 200 OK\r\n" +
                                          "Content-Type: text/html\r\n" +
                                          "\r\n" +
                                          "<html><body><h1>Hello from Cosmos OS!</h1></body></html>";

                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        client.Send(responseBytes);
                    }

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP Server error: {ex.Message}");
            }
        }
    }
}