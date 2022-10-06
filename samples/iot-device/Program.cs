using iot_device;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<DeviceUtf8>();
    })
    .Build();

await host.RunAsync();
