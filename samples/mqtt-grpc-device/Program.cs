using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt_grpc_device;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();
