# SystemDot.HealthChecks

Useful additions to the .NET Health Check packages.

## SystemDot.Hosting.Diagnostics.HealthChecks

Supports exposing health checks via TCP which can be particularly useful if you want to expose health checks in a non-HTTP workload.

A key use case where you might want to expose health status by TCP for Kubernetes (AKS, EKS, GKE, Azure Container Apps, other implementations) health probes (startup, liveness and readiness).


### Examples

See the two example projects for use in both a non-HTTP and HTTP workload.


### C# background worker

```c#
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHostedService<Worker>();
    builder.Services.AddHealthChecks().AddHealthCheckTcpListener();
    
    var host = builder.Build();
    host.Run();
```