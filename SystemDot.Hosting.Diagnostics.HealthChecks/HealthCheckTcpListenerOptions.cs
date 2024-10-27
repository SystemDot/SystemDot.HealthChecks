namespace SystemDot.Hosting.Extensions;

public class HealthCheckTcpListenerOptions
{
    private const int DefaultTcpListeningPort = 80;

    public int Port { get; set; } = DefaultTcpListeningPort!;

    public TimeSpan UpdateHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(1);
}