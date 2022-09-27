using iot_device;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
