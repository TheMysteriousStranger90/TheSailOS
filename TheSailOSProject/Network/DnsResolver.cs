using System;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DNS;

namespace TheSailOSProject.Network;

public class DnsResolver
{
    private static Address _primaryDnsServer = new Address(8, 8, 8, 8);
    private static Address _secondaryDnsServer = new Address(1, 1, 1, 1);

    public static Address GetAddress(string hostname)
    {
        if (!NetworkManager.IsInitialized)
        {
            Console.WriteLine("[ERROR] Network is not initialized. Call NetworkManager.Initialize() first.");
            return null;
        }

        if (IsValidIPAddress(hostname))
        {
            try
            {
                return Address.Parse(hostname);
            }
            catch
            {
                Console.WriteLine($"[ERROR] Invalid IP address format: {hostname}");
                return null;
            }
        }

        Address result = ResolveWithDns(hostname, _primaryDnsServer);

        if (result != null)
        {
            return result;
        }

        Console.WriteLine("[WARNING] Primary DNS failed, trying secondary DNS...");
        result = ResolveWithDns(hostname, _secondaryDnsServer);

        return result;
    }

    private static Address ResolveWithDns(string hostname, Address dnsServer)
    {
        try
        {
            Console.WriteLine($"[INFO] Resolving '{hostname}' using DNS server {dnsServer}...");

            using (var dnsClient = new DnsClient())
            {
                dnsClient.Connect(dnsServer);
                dnsClient.SendAsk(hostname);

                Address destination = dnsClient.Receive();

                if (destination != null && !destination.ToString().Equals("0.0.0.0"))
                {
                    Console.WriteLine($"[SUCCESS] Resolved '{hostname}' to {destination}");
                    return destination;
                }
                else
                {
                    Console.WriteLine($"[ERROR] DNS resolution returned invalid address for '{hostname}'");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] DNS resolution failed for '{hostname}': {ex.Message}");
            return null;
        }
    }

    private static bool IsValidIPAddress(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        string[] parts = input.Split('.');

        if (parts.Length != 4)
            return false;

        foreach (string part in parts)
        {
            if (!byte.TryParse(part, out byte value))
                return false;
        }

        return true;
    }

    public static void SetDnsServers(Address primary, Address secondary = null)
    {
        _primaryDnsServer = primary ?? new Address(8, 8, 8, 8);
        _secondaryDnsServer = secondary ?? new Address(1, 1, 1, 1);

        Console.WriteLine($"[INFO] DNS servers set to: Primary={_primaryDnsServer}, Secondary={_secondaryDnsServer}");
    }

    public static void DisplayDnsServers()
    {
        Console.WriteLine($"Primary DNS: {_primaryDnsServer}");
        Console.WriteLine($"Secondary DNS: {_secondaryDnsServer}");
    }
}