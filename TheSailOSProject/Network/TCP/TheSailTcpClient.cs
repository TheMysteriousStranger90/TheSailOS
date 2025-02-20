using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using TcpClient = Cosmos.System.Network.IPv4.TCP.TcpClient;

namespace TheSailOSProject.Network.TCP;

public static class TheSailTcpClient
{
    public static byte[] TcpClientConnect(Address destip, int destport, int localport, string data, int timeout = 80)
    {
        using var xClient = new TcpClient(localport);
        xClient.Connect(destip, destport, timeout);

        xClient.Send(Encoding.ASCII.GetBytes(data));

        var endpoint = new EndPoint(Address.Zero, 0);
        var recData = xClient.Receive(ref endpoint);
        var finalData = xClient.NonBlockingReceive(ref endpoint);

        xClient.Close();

        Console.WriteLine(endpoint.ToString());
        Console.WriteLine(recData.ToString());
        Console.WriteLine(finalData.ToString());

        return finalData;
    }
}