using TheSailOSProject.Hardware.Memory;

namespace TheSailOSProject.Commands.Memory;

public class MemoryCommand : ICommand
{
    public void Execute(string[] args)
    {
        MemoryManager.LogMemoryStatistics();
    }
}