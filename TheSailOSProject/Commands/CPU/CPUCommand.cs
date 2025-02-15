using System;
using TheSailOSProject.Hardware.CPU;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.CPU;

public class CPUCommand : ICommand
{
    public void Execute(string[] args)
    {
        try
        {
            CPUManager.LogCPUStatistics();
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error getting CPU info: {ex.Message}", ConsoleStyle.Colors.Error);
        }
    }
}