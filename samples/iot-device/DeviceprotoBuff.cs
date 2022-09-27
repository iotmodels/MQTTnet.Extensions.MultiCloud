using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

using device_template_protos;

namespace iot_device;

public class DeviceprotoBuff : BackgroundService
{
    private readonly ILogger<DeviceprotoBuff> _logger;
    private readonly IConfiguration _configuration;

    private IMqttClient? mqtt;

    public DeviceprotoBuff(ILogger<DeviceprotoBuff> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }


    protected  override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionSettings cs = new(_configuration.GetConnectionString("cs") + ";ModelId=device-template.proto");
        mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, stoppingToken);
        _logger.LogInformation($"Connected {cs}");

        var client = new ClientProtobuff(mqtt!);

        client.Interval.Value = new Properties() { Interval = 5};
        await client!.SdkInfo.SendMessageAsync(new Properties { SdkInfo = "my SDK"});

        client.Interval.OnMessage = async m =>
        {
            ack ack = new ack(); //<int>(mqtt!, "interval");
            if (m.Interval > 0)
            {
                client.Interval.Value = m;
                ack.Status = 200;
                ack.Description = "property accepted";
                ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(m);
            }
            else
            {
                ack.Status = 403;
                ack.Description = $"negative value ({m}) not accepted";
                ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(client.Interval.Value);
            }
            return await Task.FromResult(ack);
        };

        client.Echo.OnMessage = async m =>
        {
            string result = "echo ";
            for (int i = 0; i < 3; i++)
            {
                result += m.InEcho;
            }
            return await Task.FromResult(new echoResponse { OutEcho = result});
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
            
                await client.Temperature.SendMessageAsync(new Telemetries { Temp = 32.1});
            
            _logger.LogInformation("Worker running at: {time}, enabled {enabled}", DateTimeOffset.Now, true);
            await Task.Delay(client.Interval.Value.Interval * 1000, stoppingToken);
        }
    }
}