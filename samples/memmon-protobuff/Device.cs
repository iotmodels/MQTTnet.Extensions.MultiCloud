using _protos;
using Google.Protobuf.WellKnownTypes;
using Humanizer;
using memmon_model_protos;
using Microsoft.ApplicationInsights;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Diagnostics;
using System.Text;

namespace memmon;

public class Device : BackgroundService
{
    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;
    private readonly TelemetryClient _telemetryClient;

    private readonly Stopwatch clock = Stopwatch.StartNew();
    private int telemetryCounter = 0;
    private int commandCounter = 0;
    private int twinRecCounter = 0;
    private int reconnectCounter = 0;

    private double telemetryWorkingSet = 0;
    private const bool default_enabled = true;
    private const int default_interval = 45;

    private string lastDiscconectReason = string.Empty;

    private MemmonClient client;
    private ConnectionSettings connectionSettings;

    private string infoVersion = string.Empty;

    public Device(ILogger<Device> logger, IConfiguration configuration, TelemetryClient tc)
    {
        _logger = logger;
        _configuration = configuration;
        _telemetryClient = tc;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cs = new ConnectionSettings(_configuration.GetConnectionString("cs")) { ModelId="memmon.proto"};
        _logger.LogWarning($"Connecting to..{cs}");
        var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, stoppingToken);
        connectionSettings = cs;
        mqtt.DisconnectedAsync += Connection_DisconnectedAsync;
        infoVersion = BrokerClientFactory.NuGetPackageVersion;
        
        _logger.LogWarning("Connected");

        client = new MemmonClient(mqtt);

        client.Property_enabled.OnMessage = Property_enabled_UpdateHandler;
        client.Property_interval.OnMessage= Property_interval_UpdateHandler;
        client.getRuntimeStats.OnMessage= Command_getRuntimeStats_Handler;

        client.Props.Started = DateTime.UtcNow.ToTimestamp();
        client.Props.Enabled = default_enabled;
        client.Props.Interval = default_interval;


        await client.AllProperties.SendMessageAsync(client.Props);

        RefreshScreen(this);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (client.Props.Enabled == true)
            {
                telemetryWorkingSet = Environment.WorkingSet;
                await client.AllTelemetry.SendMessageAsync(new Telemetries { WorkingSet = telemetryWorkingSet }, stoppingToken);
                telemetryCounter++;
                _telemetryClient.TrackMetric("WorkingSet", telemetryWorkingSet);
            }
            await Task.Delay(client.Props.Interval * 1000, stoppingToken);
        }
    }

    private async Task Connection_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
    {
        _telemetryClient.TrackTrace("Client Disconnected: " + arg.ReasonString);
        if (arg.Exception != null)
        {
            _telemetryClient.TrackException(arg.Exception);
        }

        lastDiscconectReason = arg.ReasonString;
        reconnectCounter++;
        await Task.Yield();
    }

    private async Task<ack> Property_enabled_UpdateHandler(Properties desired)
    {
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", "enables" },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });
        client.Property_enabled.Version++;
        var ack = new ack
        {
            Value = Google.Protobuf.WellKnownTypes.Any.Pack(desired),
            Version = client.Property_enabled.Version.Value,
            Description = "desired notification accepted",
            Status = 200
        };
        client.Props.Enabled = desired.Enabled;
        return await Task.FromResult(ack);
    }

    private async Task<ack> Property_interval_UpdateHandler(Properties desired)
    {
        ArgumentNullException.ThrowIfNull(client);
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", "interval" },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });

        var ack = new ack();
        client.Property_interval.Version++;
        if (desired.Interval > 0)
        {
            ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(desired);
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = client.Property_interval.Version.Value;
            client.Props.Interval = desired.Interval;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(client.Props);
            ack.Version = client.Property_interval.Version.Value;
        };
        return await Task.FromResult(ack);
    }

    private async Task<getRuntimeStatsResponse> Command_getRuntimeStats_Handler(getRuntimeStatsRequest req)
    {
        commandCounter++;
        _telemetryClient.TrackEvent("CommandReceived", new Dictionary<string, string>()
        {
            { "CommandName", "getRuntimeStats" },
            { "NumCommands", commandCounter.ToString() }
        });

        var result = new getRuntimeStatsResponse();
        result.DiagResults.Add("machine name", Environment.MachineName);
        result.DiagResults.Add("os version", Environment.OSVersion.ToString());
        result.DiagResults.Add("started", TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3));
        

        if (req.Mode == getRuntimeStatsMode.Normal)
        {
            result.DiagResults.Add("sdk info:", infoVersion);
        }
        if (req.Mode == getRuntimeStatsMode.Full)
        {
            result.DiagResults.Add("sdk info:", infoVersion);
            result.DiagResults.Add("interval: ", client.Props.Interval.ToString());
            result.DiagResults.Add("enabled: ", client.Props.Enabled.ToString());
            result.DiagResults.Add("twin receive: ", twinRecCounter.ToString());
            //result.diagnosticResults.Add($"twin sends: ", RidCounter.Current.ToString());
            result.DiagResults.Add("telemetry: ", telemetryCounter.ToString());
            result.DiagResults.Add("command: ", commandCounter.ToString());
            result.DiagResults.Add("reconnects: ", reconnectCounter.ToString());
        }
        return await Task.FromResult(result);
    }

#pragma warning disable IDE0052 // Remove unread private members
    private Timer screenRefresher;
#pragma warning restore IDE0052 // Remove unread private members
    private void RefreshScreen(object state)
    {
        string RenderData()
        {
            void AppendLineWithPadRight(StringBuilder sb, string s) => sb.AppendLine(s?.PadRight(Console.BufferWidth > 1 ? Console.BufferWidth - 1 : 300));

            string enabled_value = client?.Props.Enabled.ToString();
            string interval_value = client?.Props.Interval.ToString();
            StringBuilder sb = new();
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"{connectionSettings?.HostName}:{connectionSettings?.TcpPort}");
            AppendLineWithPadRight(sb, $"{connectionSettings.ClientId} (Auth:{connectionSettings.Auth}/ TLS:{connectionSettings.UseTls})");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "Property", "Value".PadRight(15), "Version"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "--------", "-----".PadLeft(15, '-'), "------"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "enabled".PadRight(8), enabled_value?.PadLeft(15), client.Property_enabled.Version.Value));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "interval".PadRight(8), interval_value?.PadLeft(15), client.Property_interval .Version.Value));
            //AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "started".PadRight(8), client.Props.Started.Seconds.ToString().PadLeft(15), ""));
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Reconnects: {reconnectCounter}");
            AppendLineWithPadRight(sb, $"Telemetry: {telemetryCounter}");
            AppendLineWithPadRight(sb, $"Twin receive: {twinRecCounter}");
            //AppendLineWithPadRight(sb, $"Twin send: {RidCounter.Current}");
            AppendLineWithPadRight(sb, $"Command messages: {commandCounter}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"WorkingSet: {telemetryWorkingSet.Bytes()}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Time Running: {TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3)}");
            AppendLineWithPadRight(sb, $"NuGet: {infoVersion}");
            AppendLineWithPadRight(sb, " ");
            return sb.ToString();
        }

        Console.SetCursorPosition(0, 0);
        Console.WriteLine(RenderData());
        screenRefresher = new Timer(RefreshScreen, this, 1000, 0);
    }
}