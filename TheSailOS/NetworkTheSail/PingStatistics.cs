namespace TheSailOS.NetworkTheSail;

public class PingStatistics
{
    public int Sent { get; set; }
    public int Received { get; set; }
    public double PacketLoss => (Sent - Received) / (double)Sent * 100;
    public double AverageTime { get; set; }
    public int MinTime { get; set; }
    public int MaxTime { get; set; }

    public override string ToString()
    {
        return $"Ping statistics:\n" +
               $"    Packets: Sent = {Sent}, Received = {Received}, Lost = {Sent - Received} ({PacketLoss:F1}% loss)\n" +
               $"Approximate round trip times in milliseconds:\n" +
               $"    Minimum = {MinTime}ms, Maximum = {MaxTime}ms, Average = {AverageTime:F1}ms";
    }
}