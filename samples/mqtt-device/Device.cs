using dtmi_com_example_devicetemplate;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace mqtt_device;

public class Device : BackgroundService
{
    private Idevicetemplate? client;

    private const int default_interval = 30;

    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    public Device(ILogger<Device> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
        _logger.LogWarning("Connecting to .. {cs}", cs);
        client = await new ClientFactory(_configuration).CreateDeviceTemplateClientAsync(stoppingToken);
        _logger.LogWarning("Connected to {settings}",ClientFactory.computedSettings );

        client.Property_interval.Value = default_interval;
        client.Property_interval.OnMessage = Property_interval_UpdateHandler;
        client.Command_echo.OnMessage = Cmd_echo_Handler;


        await client.Property_sdkInfo.SendMessageAsync(ClientFactory.NuGetPackageVersion, stoppingToken);

        if (client is HubMqttClient hubClient)
        {
            client.InitialState = await hubClient.GetTwinAsync(stoppingToken);
            await TwinInitializer.InitPropertyAsync(client.Connection, client.InitialState, client.Property_interval, "interval", default_interval);
        }
        else
        {
            await PropertyInitializer.InitPropertyAsync(client.Property_interval, default_interval);
        }

        double lastTemp = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            lastTemp = GenerateSensorReading(lastTemp, 12, 45);
            //await client!.Telemetry_temp.SendTelemetryAsync(lastTemp, stoppingToken);
            var interval = client!.Property_interval.Value;
            _logger.LogInformation("Waiting {interval} s to send telemetry", interval);
            await Task.Delay(client.Property_interval.Value * 1000 , stoppingToken);
            
        }
    }

    private async Task<Ack<int>> Property_interval_UpdateHandler(int p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation("New prop interval received");
        var ack = new Ack<int>();

        if (p > 0)
        {
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = client.Property_interval.Version;
            ack.Value = p;
            //ack.LastReported = p;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Version = client.Property_interval.Version;
            ack.Value = client.Property_interval.Value;
        };
        return await Task.FromResult(ack);
    }

    private async Task<string> Cmd_echo_Handler(string req)
    {
        _logger.LogInformation($"Command echo received");
        return await Task.FromResult(req + req);
    }

    readonly Random random = new();
    double GenerateSensorReading(double currentValue, double min, double max)
    {
        double percentage = 15;
        double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
        value = Math.Max(value, min);
        value = Math.Min(value, max);
        return value;
    }
}
