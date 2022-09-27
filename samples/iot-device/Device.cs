using iot_device_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace iot_device;

public class Device : BackgroundService
{
    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    private IMqttClient? mqtt;

    public Device(ILogger<Device> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected async Task ExecuteAsync2(CancellationToken stoppingToken)
    {
        ConnectionSettings cs = new(_configuration.GetConnectionString("hub"));
        IMqttClient connection = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs, stoppingToken);
        IoTHubClient hubClient = new IoTHubClient(connection);
        var twin = await hubClient.GetTwinAsync();
        Console.WriteLine(twin);
    }


    protected  override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new ClientUTF8Json(mqtt!);

        client.Enabled.Value = true;
        client.Interval.Value = 5;
        await client!.SdkInfo.SendMessageAsync("my SDK");

        client.Interval.OnMessage = async m =>
        {
            Ack<int> ack = new Ack<int>(mqtt!, "interval");
            if (m > 0)
            {
                client.Interval.Value = m;
                ack.Status = 200;
                ack.Description = "property accepted";
                ack.Value = m;
            }
            else
            {
                ack.Status = 403;
                ack.Description = $"negative value ({m}) not accepted";
                ack.Value = client.Interval.Value;
            }
            return await Task.FromResult(ack);
        };

        client.EchoRepeater.OnMessage = async m =>
        {
            string result = "echo ";
            for (int i = 0; i < m; i++)
            {
                result += "echo ";
            }
            return await Task.FromResult(result);
        };

        client.Enabled.OnMessage = async m =>
        {
            client.Enabled.Value = m;
            return await Task.FromResult(new Ack<bool>(mqtt!, "enabled")
            {
                Status = 200,
                Description ="Prop enabled accepted",
                Value = m
            });
        };


        while (!stoppingToken.IsCancellationRequested)
        {
            if (client.Enabled.Value)
            {
                await client.Temperature.SendMessageAsync(32.2);
            }
            _logger.LogInformation("Worker running at: {time}, enabled {enabled}", DateTimeOffset.Now, client.Enabled.Value);
            await Task.Delay(client.Interval.Value * 1000, stoppingToken);
        }
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        ConnectionSettings cs = new(_configuration.GetConnectionString("cs"));
        mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, stoppingToken);
        _logger.LogInformation($"Connected {cs}");
        await ExecuteAsync(stoppingToken);
    }
}