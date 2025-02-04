using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;

namespace TheSailOS.NetworkTheSail;

public class UserUdpClient : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly EndPoint _remoteEndPoint;

    public UserUdpClient(int localPort, Address remoteAddress, int remotePort)
    {
        _udpClient = new UdpClient(localPort);
        _remoteEndPoint = new EndPoint(remoteAddress, (ushort)remotePort);
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        _udpClient.Send(data, _remoteEndPoint.Address, _remoteEndPoint.Port);
        ;
    }

    public string Receive()
    {
        var endpoint = new EndPoint(Address.Zero, 0);
        byte[] data = _udpClient.Receive(ref endpoint);
        return Encoding.ASCII.GetString(data);
    }

    public void Dispose()
    {
        _udpClient.Close();
    }
}