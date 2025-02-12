namespace TheSailOSProject.Commands.Power;

public class ShutdownCommand : ICommand
{
    public void Execute(string[] args)
    {
        Cosmos.System.Power.Shutdown();
    }
}