using System;
using TheSailOS.Configuration;
using TheSailOS.Power.Interfaces;

namespace TheSailOS.PowerSystem;

public class ShutdownCommand : IShutdownable
{
    public void Shutdown()
    {
        TheSailOSCfg.Load();
        if (!TheSailOSCfg.BootLock)
        {
            Console.WriteLine("System is shutting down...");
            Cosmos.System.Power.Shutdown();
        }
        else
        {
            Console.WriteLine("Shutdown is locked by system configuration.");
        }
    }
}