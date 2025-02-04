using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace TheSailOS.NetworkTheSail;

public class UserTcpClient : IDisposable
{
    private readonly TcpClient _tcpClient;

    public UserTcpClient(Address ip, int port)
    {
        _tcpClient = new TcpClient(port);
        _tcpClient.Connect(ip, port);
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        _tcpClient.Send(data);
    }

    public string Receive()
    {
        var endpoint = new EndPoint(Address.Zero, 0);
        byte[] data = _tcpClient.Receive(ref endpoint);
        return Encoding.ASCII.GetString(data);
    }

    public void Dispose()
    {
        _tcpClient.Close();
    }
}