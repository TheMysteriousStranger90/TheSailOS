using System;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class HttpGetCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: httpget <url>");
            return;
        }

        string url = args[0];
        string content = HttpClient.Get(url);

        if (content != null)
        {
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine($"Failed to retrieve content from {url}.");
        }
    }
}