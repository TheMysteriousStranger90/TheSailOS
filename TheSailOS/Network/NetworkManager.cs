using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace TheSailOS.Network;

public class NetworkManager : IDisposable
{
    private TcpClient client;
    private bool isConnected;
    private const int ConnectionAttempts = 3;
    private const int OperationTimeout = 5000;

    public NetworkManager()
    {
        try
        {
            NetworkStack.Init();
            Console.WriteLine("Network initialized.");
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to initialize network", ex);
        }
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

    public async Task SendDataAsync(string data)
    {
        if (client != null && isConnected)
        {
            try
            {
                var sendTask = Task.Run(() => client.Send(Encoding.ASCII.GetBytes(data)));
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
            catch (Exception ex)
            {
                isConnected = false;
                throw new Exception("Failed to send data", ex);
            }
        }
        else
        {
            throw new Exception("Not connected to any server");
        }
    }

    public async Task<byte[]> ReceiveDataAsync()
    {
        if (client != null && isConnected)
        {
            try
            {
                var receiveTask = Task.Run(() =>
                {
                    EndPoint source = new EndPoint(client.LocalEndPoint.Address, client.LocalEndPoint.Port);
                    List<byte> allData = new List<byte>();
                    byte[] buffer;

                    do
                    {
                        buffer = client.Receive(ref source);
                        allData.AddRange(buffer);
                    } while (buffer.Length > 0);

                    return allData.ToArray();
                });

                if (await Task.WhenAny(receiveTask, Task.Delay(OperationTimeout)) == receiveTask)
                {
                    return await receiveTask;
                }
                else
                {
                    isConnected = false;
                    throw new Exception("Receive operation timed out");
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                throw new Exception("Failed to receive data", ex);
            }
        }
        else
        {
            throw new Exception("Not connected to any server");
        }
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