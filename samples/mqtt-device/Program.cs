using mqtt_device;
using System.Diagnostics;

ConsoleTraceListener consoleTraceListener = new ConsoleTraceListener();
Trace.Listeners.Add(consoleTraceListener);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
