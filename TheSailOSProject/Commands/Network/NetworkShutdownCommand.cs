using System;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class NetworkShutdownCommand : ICommand
{
    public void Execute(string[] args)
    {
        try
        {
            NetworkManager.Shutdown();
            Console.WriteLine("[SUCCESS] Network connection shutdown successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to shutdown network: {ex.Message}");
        }
    }
}