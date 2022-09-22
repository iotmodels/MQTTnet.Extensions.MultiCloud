using dtmi_com_example_devicetemplate;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using mqttdevice;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace mqtt_device;

public class Device : BackgroundService
{
    private Idevicetemplate? client;

    private const int default_interval = 5;

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

        client.Command_echo.OnCmdDelegate = Cmd_echo_Handler;
        client.Command_getRuntimeStats.OnCmdDelegate = Cmd_getRuntimeStats_Handler;
        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;

        var wProps = new mqttdevice.WProperties();
        wProps.Interval = default_interval;
        await client.Property_interval.InitPropertyAsync(wProps.ToByteArray(), stoppingToken);
        
        client.Property_interval.PropertyValue.Value = default_interval;
        var prop = new mqttdevice.Properties
        {
            SdkInfo = ClientFactory.NuGetPackageVersion,
            Started = DateTime.UtcNow.ToTimestamp(),
            Interval = default_interval
        };
        await client.Property_sdkInfo.ReportPropertyAsync(prop.ToByteArray(), stoppingToken);

        

        double lastTemp = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            lastTemp = GenerateSensorReading(lastTemp, 12, 45);
            _logger.LogWarning("lastTemp {lastTemp}", lastTemp);

            var tel = new Telemetries()
            {
                Temperature = lastTemp,
                WorkingSet = Environment.WorkingSet
            };
            await client.Telemetry_temp.SendTelemetryAsync(tel.ToByteArray(), stoppingToken);

            var interval = client!.Property_interval.PropertyValue?.Value;
            _logger.LogInformation("Waiting {interval} s to send telemetry", interval);
            await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
        }
    }

    private PropertyAck<int> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation("New prop {name} received", p.Name);
        var ack = new PropertyAck<int>(p.Name);
        var propValue = mqttdevice.WProperties.Parser.ParseFrom(p.ValueBytes);
        p.Value = propValue.Interval;
        if (p.Value > 0)
        {
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = p.Version;
            ack.Value = p.Value;
            ack.LastReported = p.Value;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Version = p.Version;
            ack.Value = client.Property_interval.PropertyValue.LastReported > 0 ?
                            client.Property_interval.PropertyValue.LastReported :
                            default_interval;
        };
        client.Property_interval.PropertyValue = ack;
        var props = new mqttdevice.Properties();
        props.Interval = ack.Value;
        ack.ValueBytes = props.ToByteArray();
        return ack;
    }

    private async Task<Cmd_echo_Response> Cmd_echo_Handler(Cmd_echo_Request req)
    {
        _logger.LogInformation($"Command echo received");
        var es = new EchoService();
        var resp = await es.echo(req.EchoRequest, null);
        return new Cmd_echo_Response 
        { 
            Status = 200,
            ReponsePayload = resp.OutEcho,
            ResponseBytes = resp.ToByteArray()
        };
    }

    private async Task<Cmd_getRuntimeStats_Response> Cmd_getRuntimeStats_Handler(Cmd_getRuntimeStats_Request req)
    {
        var grs = new GetRuntimeStatsService();
        var gResp = await grs.getRuntimeStats(req.Request, null);
        var response = new Cmd_getRuntimeStats_Response()
        {
            Status = 200,
            ResponseBytes = gResp.ToByteArray()
        };
        return response;
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
