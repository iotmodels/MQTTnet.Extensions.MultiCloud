using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

using device_template_protos;

namespace iot_device;

public class DeviceUtf8 : BackgroundService
{
    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    private IMqttClient? mqtt;

    public DeviceUtf8(ILogger<Device> logger, IConfiguration configuration)
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
        ConnectionSettings cs = new(_configuration.GetConnectionString("cs") + ";ModelId=dtmi:com:example:DeviceTemplate;1");
        mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, stoppingToken);
        _logger.LogInformation($"Connected {cs}");
        var client = new ClientUTF8Json(mqtt!);

        client.Interval.Value = 5;
        await client!.SdkInfo.SendMessageAsync("my SDK");

        client.Interval.OnMessage = async m =>
        {
            Ack<int> ack = new Ack<int>(mqtt,"interval"); //<int>(mqtt!, "interval");
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

        client.Echo.OnMessage = async m =>
        {
            string result = string.Empty;
            for (int i = 0; i < 3; i++)
            {
                result += m;
            }
            return await Task.FromResult(result);
        };

        //client.Enabled.OnMessage = async m =>
        //{
        //    client.Enabled.Value = m;
        //    return await Task.FromResult(new Ack<bool>(mqtt!, "enabled")
        //    {
        //        Status = 200,
        //        Description ="Prop enabled accepted",
        //        Value = m
        //    });
        //};


        while (!stoppingToken.IsCancellationRequested)
        {
             await client.Temperature.SendMessageAsync(32.1);
            _logger.LogInformation("Worker running at: {time}, enabled {enabled}", DateTimeOffset.Now, true);
            await Task.Delay(client.Interval.Value * 1000, stoppingToken);
        }
    }

   
}