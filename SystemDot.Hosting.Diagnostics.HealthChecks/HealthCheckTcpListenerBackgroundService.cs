using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SystemDot.Hosting.Extensions;

public sealed class HealthCheckTcpListenerBackgroundService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly TcpListener _tcpListener;
    private readonly HealthCheckTcpListenerOptions _options;
    private readonly ILogger<HealthCheckTcpListenerBackgroundService> _logger;

    public HealthCheckTcpListenerBackgroundService(
        HealthCheckService healthCheckService,
        HealthCheckTcpListenerOptions options,
        ILogger<HealthCheckTcpListenerBackgroundService> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _tcpListener = new TcpListener(IPAddress.Any, _options.Port);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health Check Tcp Listener starting.");

        try
        {
            await Task.Yield();
            _tcpListener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckHealthReportAsync(stoppingToken);
                Thread.Sleep(_options.UpdateHeartbeatInterval);
            }

            _tcpListener.Stop();
        }
        catch (SocketException e)
            when (e.SocketErrorCode is SocketError.AccessDenied or SocketError.AddressAlreadyInUse)
        {
            _logger.LogError(
                e,
                "SocketErrorCode: \"{socketErrorCode}\" for the configured port: \"{port}\". " +
                "This is probably due to another application using this port. " +
                "Ensure an unused port is configured. " +
                "The behavior of whether an exception in a BackgroundService stops the Host can be configured using HostOptions.BackgroundServiceExceptionBehavior.",
                e.SocketErrorCode,
                _options.Port);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred starting the BackgroundService." +
                "The behavior of whether an exception in a BackgroundService stops the Host can be configured using HostOptions.BackgroundServiceExceptionBehavior.");
            throw;
        }

        _logger.LogInformation("Health Check Tcp Listener ending.");
    }

    private async Task CheckHealthReportAsync(CancellationToken token)
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(token);
            if (healthReport.Status != HealthStatus.Healthy)
            {
                _tcpListener.Stop();
                _logger.LogWarning("Health Report status is {status}.", healthReport.Status.ToString());
                return;
            }

            _tcpListener.Start();
            while (_tcpListener.Server.IsBound && _tcpListener.Pending())
            {
                using (var client = await _tcpListener.AcceptTcpClientAsync(token))
                {
                    client.Close();
                }

                _logger.LogTrace("Health Report status is {status}.", healthReport.Status.ToString());
            }

            _logger.LogTrace("Check Health Report executed.");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking the Health Report.");
        }
    }
}