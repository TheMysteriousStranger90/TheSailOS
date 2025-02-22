using System;
using System.Collections.Generic;
using TheSailOSProject.Users;

namespace TheSailOSProject.Session
{
    public static class SessionManager
    {
        private static Dictionary<string, Session> _activeSessions = new Dictionary<string, Session>();
        private static readonly object _sessionLock = new object();

        public static Session StartSession(User user)
        {
            lock (_sessionLock)
            {
                string sessionId = Guid.NewGuid().ToString();

                Session session = new Session(sessionId, user, System.DateTime.Now);
                _activeSessions.Add(sessionId, session);

                Console.WriteLine(
                    $"[SessionManager] Session started for user: {user.Username}, Session ID: {sessionId}");
                return session;
            }
        }

        public static Session GetSession(string sessionId)
        {
            lock (_sessionLock)
            {
                if (_activeSessions.ContainsKey(sessionId))
                {
                    return _activeSessions[sessionId];
                }

                return null;
            }
        }

        public static void EndSession(string sessionId)
        {
            lock (_sessionLock)
            {
                if (_activeSessions.ContainsKey(sessionId))
                {
                    _activeSessions.Remove(sessionId);
                    Console.WriteLine($"[SessionManager] Session ended for Session ID: {sessionId}");
                }
            }
        }

        public static void UpdateSessionActivity(string sessionId)
        {
            lock (_sessionLock)
            {
                Session session = GetSession(sessionId);
                if (session != null)
                {
                    session.LastActivity = System.DateTime.Now;
                    //Console.WriteLine($"[SessionManager] Session activity updated for Session ID: {sessionId}");
                }
            }
        }
    }
}