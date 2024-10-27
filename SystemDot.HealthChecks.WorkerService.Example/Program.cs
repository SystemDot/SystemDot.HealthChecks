using SystemDot.HealthChecks.WorkerService.Example;
using SystemDot.Hosting.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHealthChecks().AddHealthCheckTcpListener();

var host = builder.Build();
host.Run();