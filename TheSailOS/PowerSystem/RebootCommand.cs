using System;
using TheSailOS.Configuration;
using TheSailOS.Power.Interfaces;

namespace TheSailOS.PowerSystem;

public class RebootCommand : IRebootable
{
    public void Reboot()
    {
        TheSailOSCfg.Load();
        if (!TheSailOSCfg.BootLock)
        {
            Console.WriteLine("System is rebooting...");
            Cosmos.System.Power.Reboot();
        }
        else
        {
            Console.WriteLine("Reboot is locked by system configuration.");
        }
    }
}