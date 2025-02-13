using System;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class NetworkConfigureCommand : ICommand
{
    public void Execute(string[] args)
    {
        try
        {
            
            NetworkManager.ConfigureManually();
            Console.WriteLine("[SUCCESS] Network configured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to configure network: {ex.Message}");
        }
    }
}