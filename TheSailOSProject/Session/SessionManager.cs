using System;
using System.Collections.Generic;
using System.Linq;
using TheSailOSProject.Users;

namespace TheSailOSProject.Session
{
    public static class SessionManager
    {
        private static Dictionary<string, Session> _activeSessions = new Dictionary<string, Session>();
        private static readonly object _sessionLock = new object();
        private static int _sessionCounter;

        public static Session StartSession(User user)
        {
            lock (_sessionLock)
            {
                var timestamp = System.DateTime.Now.Ticks;
                var sessionId = $"{user.Username}-{timestamp}-{++_sessionCounter}";
            
                var session = new Session(sessionId, user, System.DateTime.Now);
                _activeSessions.Add(sessionId, session);

                Console.WriteLine($"[Session] New session created: {sessionId}");
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
        
        public static List<Session> GetAllSessions()
        {
            lock (_sessionLock)
            {
                return _activeSessions.Values.ToList();
            }
        }
        
        public static void CleanupInactiveSessions(TimeSpan maxInactiveTime)
        {
            lock (_sessionLock)
            {
                var cutoff = System.DateTime.Now - maxInactiveTime;
                var toRemove = _activeSessions.Values
                    .Where(s => s.LastActivity < cutoff)
                    .ToList();

                foreach (var session in toRemove)
                {
                    EndSession(session.SessionId);
                }
            }
        }
    }
}