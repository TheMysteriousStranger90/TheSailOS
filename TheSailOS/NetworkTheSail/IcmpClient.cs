using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cosmos.System.Network.IPv4;

namespace TheSailOS.NetworkTheSail;

public class IcmpClient : IDisposable
{
    private readonly ICMPClient _icmpClient;
    private readonly Address _destination;
    private readonly List<PingResult> _pingHistory;
    private bool _isPinging;
    private Action<PingResult> _onPingCompleted;
    private Action<Exception> _onError;

    public class PingResult
    {
        public int Time { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public IcmpClient(Address destination)
    {
        _icmpClient = new ICMPClient();
        _destination = destination;
        _pingHistory = new List<PingResult>();
        _icmpClient.Connect(destination);
    }

    public void SetCallbacks(Action<PingResult> onPingCompleted, Action<Exception> onError)
    {
        _onPingCompleted = onPingCompleted;
        _onError = onError;
    }

    public PingResult SendPing(int timeout = 5000)
    {
        try
        {
            _isPinging = true;
            _icmpClient.SendEcho();

            var endpoint = new EndPoint(Address.Zero, 0);
            int time = _icmpClient.Receive(ref endpoint);

            var result = new PingResult
            {
                Time = time,
                Success = time > 0,
                Timestamp = DateTime.Now
            };

            _pingHistory.Add(result);
            _onPingCompleted?.Invoke(result);
            return result;
        }
        catch (Exception ex)
        {
            _onError?.Invoke(ex);
            return new PingResult { Success = false, Timestamp = DateTime.Now };
        }
        finally
        {
            _isPinging = false;
        }
    }

    public void StartContinuousPing(int interval = 1000)
    {
        _isPinging = true;
        while (_isPinging)
        {
            SendPing();
            Thread.Sleep(interval);
        }
    }

    public PingStatistics GetStatistics()
    {
        return new PingStatistics
        {
            Sent = _pingHistory.Count,
            Received = _pingHistory.Count(x => x.Success),
            AverageTime = _pingHistory.Where(x => x.Success).Average(x => x.Time),
            MinTime = _pingHistory.Where(x => x.Success).Min(x => x.Time),
            MaxTime = _pingHistory.Where(x => x.Success).Max(x => x.Time)
        };
    }

    public void Dispose()
    {
        _isPinging = false;
        _icmpClient.Close();
    }
}