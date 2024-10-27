using Microsoft.Extensions.DependencyInjection;

namespace SystemDot.Hosting.Extensions;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddHealthCheckTcpListener(
        this IHealthChecksBuilder healthChecksBuilder,
        Action<HealthCheckTcpListenerOptions>? options = null)
    {
        HealthCheckTcpListenerOptions healthCheckTcpListenerOptions = new();
        options?.Invoke(healthCheckTcpListenerOptions);
        healthChecksBuilder
            .Services
            .AddSingleton(healthCheckTcpListenerOptions)
            .AddHostedService<HealthCheckTcpListenerBackgroundService>();
        return healthChecksBuilder;
    }
}