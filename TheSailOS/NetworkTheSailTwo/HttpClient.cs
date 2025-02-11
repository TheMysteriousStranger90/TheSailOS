using System;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DNS;
using CosmosHttp.Client;

namespace TheSailOS.NetworkTheSailTwo;

public static class HttpClient
{
    public static string Get(string url)
    {
        if (url.StartsWith("https://"))
        {
            throw new NotSupportedException("HTTPS is not supported. Use HTTP instead.");
        }

        string domain = ExtractDomainNameFromUrl(url);
        string path = ExtractPathFromUrl(url);

        Address serverIp = ResolveDns(domain);

        HttpRequest request = new HttpRequest
        {
            IP = serverIp.ToString(),
            Domain = domain,
            Path = path,
            Method = "GET"
        };

        request.Send();
        return request.Response.Content;
    }

    private static Address ResolveDns(string domain)
    {
        using (var dns = new DnsClient())
        {
            dns.Connect(new Address(8, 8, 8, 8));
            dns.SendAsk(domain);
            return dns.Receive();
        }
    }

    private static string ExtractDomainNameFromUrl(string url)
    {
        int start = url.IndexOf("://") + 3;
        int end = url.IndexOf('/', start);
        return end == -1 ? url[start..] : url[start..end];
    }

    private static string ExtractPathFromUrl(string url)
    {
        int start = url.IndexOf("://") + 3;
        int slashIndex = url.IndexOf('/', start);
        return slashIndex == -1 ? "/" : url[slashIndex..];
    }
}