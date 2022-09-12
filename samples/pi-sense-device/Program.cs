using pi_sense_device;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

await host.RunAsync();
