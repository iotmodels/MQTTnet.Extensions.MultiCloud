using memmon;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System.Diagnostics;

TelemetryDebugWriter.IsTracingDisabled = Debugger.IsAttached;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
