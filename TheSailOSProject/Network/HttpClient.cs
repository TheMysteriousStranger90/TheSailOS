using System;
using System.Text;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace TheSailOSProject.Network;

public class HttpClient
{
    public static string DownloadString(string url)
    {
        if (!NetworkManager.IsInitialized)
        {
            throw new Exception("Network is not initialized. Call NetworkManager.Initialize() first.");
        }

        if (url.StartsWith("https://"))
        {
            throw new Exception("HTTPS is not currently supported. Please use HTTP protocol.");
        }

        if (!url.StartsWith("http://"))
        {
            url = "http://" + url;
        }

        try
        {
            string domainName = ExtractDomainNameFromUrl(url);
            string path = ExtractPathFromUrl(url);

            Console.WriteLine($"[INFO] Downloading from: {url}");
            Console.WriteLine($"[INFO] Domain: {domainName}, Path: {path}");

            Address serverAddress = DnsResolver.GetAddress(domainName);

            if (serverAddress == null)
            {
                throw new Exception($"Could not resolve hostname: {domainName}");
            }

            Console.WriteLine($"[INFO] Connecting to {serverAddress}:80...");

            string request = BuildHttpGetRequest(domainName, path);

            string response = SendHttpRequest(serverAddress, 80, request);

            string content = ExtractHttpContent(response);

            Console.WriteLine($"[SUCCESS] Download completed ({content.Length} bytes)");
            return content;
        }
        catch (Exception ex)
        {
            throw new Exception($"HTTP request failed: {ex.Message}");
        }
    }

    public static byte[] DownloadData(string url)
    {
        string content = DownloadString(url);
        return Encoding.ASCII.GetBytes(content);
    }

    private static string BuildHttpGetRequest(string host, string path)
    {
        return $"GET {path} HTTP/1.1\r\n" +
               $"Host: {host}\r\n" +
               $"User-Agent: TheSailOS/0.0.4\r\n" +
               $"Accept: */*\r\n" +
               $"Connection: close\r\n" +
               $"\r\n";
    }

    private static string SendHttpRequest(Address serverAddress, int port, string request)
    {
        try
        {
            using (var tcpClient = new TcpClient(port))
            {
                Console.WriteLine("[DEBUG] Establishing TCP connection...");
                tcpClient.Connect(serverAddress, port);

                Console.WriteLine("[DEBUG] Sending HTTP request...");
                var requestBytes = Encoding.ASCII.GetBytes(request);
                tcpClient.Send(requestBytes);

                Console.WriteLine("[DEBUG] Waiting for response...");
                var endPoint = new Cosmos.System.Network.IPv4.EndPoint(Address.Zero, 0);
                
                var responseData = tcpClient.Receive(ref endPoint);

                if (responseData != null && responseData.Length > 0)
                {
                    Console.WriteLine($"[DEBUG] Received {responseData.Length} bytes");
                    return Encoding.ASCII.GetString(responseData);
                }

                throw new Exception("No response received from server");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"TCP connection failed: {ex.Message}");
        }
    }

    private static string ExtractHttpContent(string response)
    {
        int headerEnd = response.IndexOf("\r\n\r\n");

        if (headerEnd >= 0)
        {
            return response.Substring(headerEnd + 4);
        }

        headerEnd = response.IndexOf("\n\n");
        if (headerEnd >= 0)
        {
            return response.Substring(headerEnd + 2);
        }

        return response;
    }

    private static string ExtractDomainNameFromUrl(string url)
    {
        int start;

        if (url.Contains("://"))
            start = url.IndexOf("://") + 3;
        else
            start = 0;

        int end = url.IndexOf("/", start);

        if (end == -1)
            end = url.Length;

        string domain = url.Substring(start, end - start);

        int portIndex = domain.IndexOf(":");
        if (portIndex != -1)
        {
            domain = domain.Substring(0, portIndex);
        }

        return domain;
    }

    private static string ExtractPathFromUrl(string url)
    {
        int start;

        if (url.Contains("://"))
            start = url.IndexOf("://") + 3;
        else
            start = 0;

        int indexOfSlash = url.IndexOf("/", start);

        if (indexOfSlash != -1)
            return url.Substring(indexOfSlash);
        else
            return "/";
    }
}