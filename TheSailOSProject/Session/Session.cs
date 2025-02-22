using TheSailOSProject.Users;

namespace TheSailOSProject.Session;

public class Session
{
    public string SessionId { get; private set; }
    public User User { get; private set; }
    public System.DateTime StartTime { get; private set; }
    public System.DateTime LastActivity { get; set; }

    public Session(string sessionId, User user, System.DateTime startTime)
    {
        SessionId = sessionId;
        User = user;
        StartTime = startTime;
        LastActivity = startTime;
    }
}