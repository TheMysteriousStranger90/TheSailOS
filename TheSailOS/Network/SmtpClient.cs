using System;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace TheSailOS.Network;

public class SmtpClient : IDisposable
{
    private TcpClient client;
    private bool isConnected;
    private const int ConnectionAttempts = 3;
    private const int OperationTimeout = 5000;

    public SmtpClient()
    {
    }

    public bool Connect(Address address, ushort port)
    {
        int attempts = 0;
        while (attempts < ConnectionAttempts)
        {
            try
            {
                client = new TcpClient(address, port);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == ConnectionAttempts)
                {
                    throw new Exception("Failed to connect", ex);
                }
            }
        }

        return false;
    }

    public async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        if (client != null && isConnected)
        {
            try
            {
                await SendCommandAsync($"MAIL FROM:<{from}>\r\n");
                await SendCommandAsync($"RCPT TO:<{to}>\r\n");
                await SendCommandAsync("DATA\r\n");
                await SendCommandAsync($"Subject: {subject}\r\n{body}\r\n.\r\n");
            }
            catch (Exception ex)
            {
                isConnected = false;
                throw new Exception("Failed to send email", ex);
            }
        }
        else
        {
            throw new Exception("Not connected to any server");
        }
    }

    private async Task SendCommandAsync(string command)
    {
        var sendTask = Task.Run(() =>
        {
            client.Send(Encoding.ASCII.GetBytes(command));
            var response = ReadResponse();
            if (!response.StartsWith("2"))
            {
                throw new Exception($"SMTP command failed with response: {response}");
            }
        });
        if (await Task.WhenAny(sendTask, Task.Delay(OperationTimeout)) == sendTask)
        {
            await sendTask;
        }
        else
        {
            isConnected = false;
            throw new Exception("Send operation timed out");
        }
    }

    private string ReadResponse()
    {
        var response = new StringBuilder();
        bool isMultiline = false;
        do
        {
            EndPoint source = new EndPoint(client.LocalEndPoint.Address, client.LocalEndPoint.Port);

            var buffer = new byte[1024];
            var received = client.Receive(ref source);
            if (received.Length > 0)
            {
                var line = Encoding.ASCII.GetString(buffer, 0, received.Length);
                response.Append(line);
                isMultiline = line.StartsWith("3");
            }
        } while (isMultiline);

        return response.ToString();
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.Close();
            client = null;
            isConnected = false;
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}