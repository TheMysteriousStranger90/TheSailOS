using TheSailOSProject.Users;

namespace TheSailOSProject.Session;

public enum SessionStatus
{
    Active,
    Idle,
    Suspended,
    Terminated
}

public class Session
{
    public string SessionId { get; private set; }
    public User User { get; private set; }
    public System.DateTime StartTime { get; private set; }
    public System.DateTime LastActivity { get; set; }
    public SessionStatus Status { get; private set; }
    public int CommandsExecuted { get; private set; }
    public string LoginIP { get; private set; }

    public Session(string sessionId, User user, System.DateTime startTime)
    {
        SessionId = sessionId;
        User = user;
        StartTime = startTime;
        LastActivity = startTime;
        Status = SessionStatus.Active;
        CommandsExecuted = 0;
        LoginIP = "Local";
    }

    public void UpdateActivity()
    {
        LastActivity = System.DateTime.Now;
        if (Status == SessionStatus.Idle)
        {
            Status = SessionStatus.Active;
        }
    }

    public void IncrementCommandCount()
    {
        CommandsExecuted++;
        UpdateActivity();
    }

    public void MarkAsIdle()
    {
        Status = SessionStatus.Idle;
    }

    public void MarkAsSuspended()
    {
        Status = SessionStatus.Suspended;
    }

    public void MarkAsTerminated()
    {
        Status = SessionStatus.Terminated;
    }

    public System.TimeSpan GetSessionDuration()
    {
        return System.DateTime.Now - StartTime;
    }

    public System.TimeSpan GetIdleTime()
    {
        return System.DateTime.Now - LastActivity;
    }

    public string GetFormattedDuration()
    {
        var duration = GetSessionDuration();
        if (duration.TotalHours >= 1)
        {
            return duration.Hours + "h " + duration.Minutes + "m";
        }
        else if (duration.TotalMinutes >= 1)
        {
            return duration.Minutes + "m " + duration.Seconds + "s";
        }
        else
        {
            return duration.Seconds + "s";
        }
    }
}