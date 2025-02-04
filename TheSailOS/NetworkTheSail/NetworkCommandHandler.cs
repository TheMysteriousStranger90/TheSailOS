using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cosmos.System.Network.IPv4;

namespace TheSailOS.NetworkTheSail;

public class NetworkCommandHandler
{
    private readonly NetworkService _networkService;
    private readonly Dictionary<string, object> _connections;

    public NetworkCommandHandler()
    {
        _networkService = NetworkService.Instance;
        _connections = new Dictionary<string, object>();

        _networkService.SetStatusCallback(status => Console.WriteLine($"[Network] {status}"));
    }

    public void HandleCommand(string command, string[] args)
    {
        Console.WriteLine($"Debug: NetworkCommandHandler processing command: {command}");
    
        switch (command.ToLower())
        {
            case "netinit":
                Console.WriteLine("Debug: Initializing network...");
                var result = _networkService.Initialize();
                Console.WriteLine($"Network initialization {(result ? "successful" : "failed")}");
                break;

            case "connect":
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: connect <tcp|udp> <id> <ip> <port>");
                    return;
                }

                HandleConnect(args[0], args[1], args[2], int.Parse(args[3]));
                break;

            case "send":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: send <connection_id> <message>");
                    return;
                }

                HandleSend(args[0], string.Join(" ", args.Skip(1)));
                break;

            case "disconnect":
                if (args.Length < 1)
                {
                    Console.WriteLine("Usage: disconnect <connection_id>");
                    return;
                }

                _networkService.CloseConnection(args[0]);
                break;
            case "netstat":
                ShowNetworkStatus();
                break;

            case "connections":
                ShowConnections();
                break;

            case "ping":
                if (args.Length < 1)
                {
                    Console.WriteLine("Usage: ping <ip> [-t] [-c count]");
                    return;
                }

                HandlePing(args);
                break;
        }
    }

    private void HandleConnect(string protocol, string id, string ip, int port)
    {
        switch (protocol.ToLower())
        {
            case "tcp":
                var tcpClient = _networkService.CreateTcpConnection(id, ip, port);
                if (tcpClient != null)
                {
                    _connections[id] = tcpClient;
                }

                break;

            case "udp":
                var udpClient = _networkService.CreateUdpConnection(id, port, Address.Parse(ip), (ushort)port);
                if (udpClient != null)
                {
                    _connections[id] = udpClient;
                }

                break;
        }
    }

    private void HandleSend(string id, string message)
    {
        if (_connections.TryGetValue(id, out var connection))
        {
            try
            {
                switch (connection)
                {
                    case UserTcpClient tcp:
                        tcp.Send(message);
                        Console.WriteLine($"Response: {tcp.Receive()}");
                        break;

                    case UserUdpClient udp:
                        udp.Send(message);
                        Console.WriteLine($"Response: {udp.Receive()}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send failed: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Connection {id} not found");
        }
    }

    private void ShowNetworkStatus()
    {
        Console.WriteLine("\nNetwork Status:");
        Console.WriteLine($"IP Address: {NetworkManager.GetCurrentIP()}");
        Console.WriteLine($"Active Connections: {_connections.Count}");
    }

    private void ShowConnections()
    {
        if (_connections.Count == 0)
        {
            Console.WriteLine("No active connections");
            return;
        }

        Console.WriteLine("\nActive Connections:");
        foreach (var conn in _connections)
        {
            Console.WriteLine($"ID: {conn.Key}, Type: {conn.Value.GetType().Name}");
        }
    }

    private void HandlePing(string[] args)
    {
        try
        {
            var address = Address.Parse(args[0]);
            var icmpClient = new IcmpClient(address);
            
            icmpClient.SetCallbacks(
                result => 
                {
                    if (result.Success)
                    {
                        Console.WriteLine($"Reply from {args[0]}: time={result.Time}ms");
                    }
                    else
                    {
                        Console.WriteLine("Request timed out");
                    }
                },
                ex => Console.WriteLine($"Ping error: {ex.Message}")
            );

            if (args.Length > 1 && args[1] == "-t")
            {
                icmpClient.StartContinuousPing();
            }
            else
            {
                icmpClient.SendPing();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ping failed: {ex.Message}");
        }
    }
}