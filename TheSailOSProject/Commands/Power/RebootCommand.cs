namespace TheSailOSProject.Commands.Power;

public class RebootCommand : ICommand
{
    public void Execute(string[] args)
    {
        Cosmos.System.Power.Reboot();
    }
}