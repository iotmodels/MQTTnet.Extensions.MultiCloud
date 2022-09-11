using System.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using memmon;

TelemetryDebugWriter.IsTracingDisabled = Debugger.IsAttached;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
