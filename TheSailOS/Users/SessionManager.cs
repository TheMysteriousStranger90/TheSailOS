using System.Collections.Generic;

namespace TheSailOS.Users;

public class SessionManager
{
    private Dictionary<string, Session> sessions;

    public SessionManager()
    {
        sessions = new Dictionary<string, Session>();
    }

    public Session CreateSession(User user)
    {
        var session = new Session(user);
        sessions.Add(session.Id, session);
        return session;
    }

    public void DeleteSession(string sessionId)
    {
        sessions.Remove(sessionId);
    }

    public User GetUserFromSession(string sessionId)
    {
        if (sessions.TryGetValue(sessionId, out var session))
        {
            return session.User;
        }

        return null;
    }
}