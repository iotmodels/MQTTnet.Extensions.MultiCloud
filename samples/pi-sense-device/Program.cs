using Microsoft.ApplicationInsights.Extensibility.Implementation;
using pi_sense_device;
using System.Diagnostics;

TelemetryDebugWriter.IsTracingDisabled = Debugger.IsAttached;
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
        services.AddSingleton<SenseHatFactory>();
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

await host.RunAsync();
