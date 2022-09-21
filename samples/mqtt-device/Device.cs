using dtmi_com_example_devicetemplate;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using mqttdevice;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Reflection;

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

        client.Command_echo.OnCmdDelegate = Cmd_echo_Handler;
        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;

        var prop = new mqttdevice.Properties
        {
            SdkInfo = ClientFactory.NuGetPackageVersion,
            Started = DateTime.UtcNow.ToTimestamp()
        };
        //client.Property_sdkInfo.PropertyValue = $"{baseClient.Namespace} {baseClient.Assembly.GetType("ThisAssembly")!.GetField("NuGetPackageVersion", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)}";
        await client.Property_sdkInfo.ReportPropertyAsync(prop.ToByteArray(), stoppingToken);

        //await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        //await client.Property_interval.ReportPropertyAsync(stoppingToken);

        double lastTemp = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            lastTemp = GenerateSensorReading(lastTemp, 12, 45);
            //await client!.Telemetry_temp.SendTelemetryAsync(lastTemp, stoppingToken);
            var tel = new Telemetries();
            tel.Temperature = lastTemp;
            tel.WorkingSet = Environment.WorkingSet;
            _logger.LogWarning("lastTemp {lastTemp}", lastTemp);

            await client.Telemetry_temp.SendTelemetryAsync(tel.ToByteArray(), stoppingToken);

            //await client.Connection.PublishAsync(
            //    new MQTTnet.MqttApplicationMessageBuilder()
            //        .WithTopic($"device/{client.Connection.Options.ClientId}/telemetry")
            //        .WithPayload(tel.ToByteArray())
            //        .Build(), stoppingToken);

            //var interval = client!.Property_interval.PropertyValue?.Value;
            //_logger.LogInformation("Waiting {interval} s to send telemetry", interval);
            //await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
            await Task.Delay(20000, stoppingToken);
        }
    }

    private PropertyAck<int> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation("New prop {name} received", p.Name);
        var ack = new PropertyAck<int>(p.Name);

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
