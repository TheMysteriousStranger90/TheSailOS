using System;
using TheSailOSProject.Session;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Session;

public class SessionCommand : ICommand
{
    public string Name => "session";
    public string Description => "Manage user sessions";

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        string subCommand = args[0].ToLower();

        switch (subCommand)
        {
            case "list":
                ListSessions();
                break;

            case "stats":
                ShowStatistics();
                break;

            case "kill":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: session kill <session_id>", ConsoleStyle.Colors.Error);
                    return;
                }

                KillSession(args[1]);
                break;

            case "cleanup":
                CleanupSessions();
                break;

            default:
                ConsoleManager.WriteLineColored("Unknown subcommand: " + subCommand, ConsoleStyle.Colors.Error);
                ShowUsage();
                break;
        }
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("Session Management Commands:", ConsoleStyle.Colors.Primary);
        Console.WriteLine("  session list     - List all active sessions");
        Console.WriteLine("  session stats    - Show session statistics");
        Console.WriteLine("  session kill <id> - Terminate a specific session");
        Console.WriteLine("  session cleanup  - Clean up inactive sessions");
    }

    private void ListSessions()
    {
        var sessions = SessionManager.GetAllSessions();

        if (sessions.Count == 0)
        {
            ConsoleManager.WriteLineColored("No active sessions", ConsoleStyle.Colors.Warning);
            return;
        }

        ConsoleManager.WriteLineColored("Active Sessions:", ConsoleStyle.Colors.Primary);
        Console.WriteLine("ID                          | User       | Status     | Duration | Commands");
        Console.WriteLine("----------------------------|------------|------------|----------|----------");

        foreach (var session in sessions)
        {
            string id = session.SessionId.Length > 27
                ? session.SessionId.Substring(0, 27)
                : session.SessionId.PadRight(27);

            string user = session.User.Username.Length > 10
                ? session.User.Username.Substring(0, 10)
                : session.User.Username.PadRight(10);

            string status = SessionStatusToString(session.Status).PadRight(10);
            string duration = session.GetFormattedDuration().PadRight(8);
            string commands = session.CommandsExecuted.ToString();

            Console.WriteLine(id + " | " + user + " | " + status + " | " + duration + " | " + commands);
        }
    }

    private void ShowStatistics()
    {
        var stats = SessionManager.GetStatistics();

        ConsoleManager.WriteLineColored("Session Statistics:", ConsoleStyle.Colors.Primary);
        Console.WriteLine("  Active Sessions: " + stats.TotalActive);
        Console.WriteLine("  Idle Sessions: " + stats.TotalIdle);
        Console.WriteLine("  Total Created: " + stats.TotalCreated);
        Console.WriteLine("  Total Commands: " + stats.TotalCommands);
        Console.WriteLine("  Average Duration: " + stats.AverageDuration.TotalMinutes.ToString("F1") + " minutes");
    }

    private void KillSession(string sessionId)
    {
        var session = SessionManager.GetSession(sessionId);

        if (session == null)
        {
            ConsoleManager.WriteLineColored("Session not found: " + sessionId, ConsoleStyle.Colors.Error);
            return;
        }

        if (!Kernel.CurrentUser.IsAdministrator())
        {
            ConsoleManager.WriteLineColored("Only administrators can kill sessions", ConsoleStyle.Colors.Error);
            return;
        }

        SessionManager.EndSession(sessionId);
        ConsoleManager.WriteLineColored("Session terminated: " + sessionId, ConsoleStyle.Colors.Success);
    }

    private void CleanupSessions()
    {
        SessionManager.CleanupInactiveSessions(System.TimeSpan.FromMinutes(1));
        ConsoleManager.WriteLineColored("Inactive sessions cleaned up", ConsoleStyle.Colors.Success);
    }

    private string SessionStatusToString(SessionStatus status)
    {
        switch (status)
        {
            case SessionStatus.Active:
                return "Active";
            case SessionStatus.Idle:
                return "Idle";
            case SessionStatus.Suspended:
                return "Suspended";
            case SessionStatus.Terminated:
                return "Terminated";
            default:
                return "Unknown";
        }
    }
}