using mqtt_device;
using System.Diagnostics;

//using ConsoleTraceListener consoleListerner = new ConsoleTraceListener();
//Trace.Listeners.Add(consoleListerner);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
