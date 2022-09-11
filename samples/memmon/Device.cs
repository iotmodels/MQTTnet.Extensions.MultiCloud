using System.Text;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Humanizer;

using dtmi_rido_pnp_memmon;
using System.Reflection;
using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;

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
    private const int default_interval = 5;

    private string lastDiscconectReason = string.Empty;

    private Imemmon client;
    private ConnectionSettings connectionSettings;

    private string infoVersion = string.Empty;

    public Device(ILogger<Device> logger, IConfiguration configuration, TelemetryClient tc)
    {
        _logger = logger;
        _configuration = configuration;
        _telemetryClient = tc;
        //infoVersion = typeof(ConnectionSettings).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("Connecting..");
        var memmonFactory = new MemMonFactory(_configuration);
        client = await memmonFactory.CreateMemMonClientAsync(_configuration.GetConnectionString("cs"), stoppingToken);
        client.Connection.DisconnectedAsync += Connection_DisconnectedAsync;
        connectionSettings = MemMonFactory.connectionSettings;
        _logger.LogWarning("Connected");

        Type baseClient = client.GetType().BaseType;
        infoVersion = $"{baseClient.Namespace} {baseClient.Assembly.GetType("ThisAssembly")!.GetField("NuGetPackageVersion",BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)}";  

        client.Property_enabled.OnProperty_Updated = Property_enabled_UpdateHandler;
        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;
        client.Command_getRuntimeStats.OnCmdDelegate = Command_getRuntimeStats_Handler;

        await client.Property_enabled.InitPropertyAsync(client.InitialState, default_enabled, stoppingToken);
        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        
        await client.Property_interval.ReportPropertyAsync(stoppingToken);

        client.Property_enabled.PropertyValue.SetDefault(default_enabled);
        await client.Property_enabled.ReportPropertyAsync(stoppingToken);

        client.Property_started.PropertyValue = DateTime.Now;
        await client.Property_started.ReportPropertyAsync(stoppingToken);

        RefreshScreen(this);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (client?.Property_enabled.PropertyValue.Value == true)
            {
                telemetryWorkingSet = Environment.WorkingSet;
                await client.Telemetry_workingSet.SendTelemetryAsync(telemetryWorkingSet, stoppingToken);
                telemetryCounter++;
                _telemetryClient.TrackMetric("WorkingSet", telemetryWorkingSet);
            }
            var interval = client?.Property_interval.PropertyValue?.Value;
            await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
        }
    }

    private async Task Connection_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
    {
        _telemetryClient.TrackTrace("Client Disconnected: " + arg.ReasonString);
        lastDiscconectReason = arg.ReasonString;
        reconnectCounter++;
        await Task.Yield();
    }

    

    private async Task<PropertyAck<bool>> Property_enabled_UpdateHandler(PropertyAck<bool> p)
    {
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", p.Name },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });

        var ack = new PropertyAck<bool>(p.Name)
        {
            Description = "desired notification accepted",
            Status = 200,
            Version = p.Version,
            Value = p.Value
        };
        client.Property_enabled.PropertyValue = ack;
        await client.Property_enabled.ReportPropertyAsync();
        return await Task.FromResult(ack);
    }

    private async Task<PropertyAck<int>> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", p.Name },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });
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
        await client.Property_interval.ReportPropertyAsync();
        return await Task.FromResult(ack);
    }

    private async Task<Cmd_getRuntimeStats_Response> Command_getRuntimeStats_Handler(Cmd_getRuntimeStats_Request req)
    {
        commandCounter++;
        _telemetryClient.TrackEvent("CommandReceived", new Dictionary<string, string>()
        {
            { "CommandName", "getRuntimeStats" },
            { "NumCommands", commandCounter.ToString() }
        });
        var result = new Cmd_getRuntimeStats_Response()
        {
            Status = 200
        };

        result.diagnosticResults.Add("machine name", Environment.MachineName);
        result.diagnosticResults.Add("os version", Environment.OSVersion.ToString());
        result.diagnosticResults.Add("started", TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3));
        if (req.DiagnosticsMode == DiagnosticsMode.complete)
        {
            result.diagnosticResults.Add("this app:", System.Reflection.Assembly.GetExecutingAssembly()?.FullName ?? "");
        }
        if (req.DiagnosticsMode == DiagnosticsMode.full)
        {
            result.diagnosticResults.Add("interval: ", client.Property_interval.PropertyValue.Value.ToString());
            result.diagnosticResults.Add("enabled: ", client.Property_enabled.PropertyValue.Value.ToString());
            result.diagnosticResults.Add("twin receive: ", twinRecCounter.ToString());
            //result.diagnosticResults.Add($"twin sends: ", RidCounter.Current.ToString());
            result.diagnosticResults.Add("telemetry: ", telemetryCounter.ToString());
            result.diagnosticResults.Add("command: ", commandCounter.ToString());
            result.diagnosticResults.Add("reconnects: ", reconnectCounter.ToString());
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

            string enabled_value = client?.Property_enabled?.PropertyValue.Value.ToString();
            string interval_value = client?.Property_interval.PropertyValue?.Value.ToString();
            StringBuilder sb = new();
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"{connectionSettings?.HostName}:{connectionSettings?.TcpPort}");
            AppendLineWithPadRight(sb, $"{connectionSettings.ClientId} (Auth:{connectionSettings.Auth}/ TLS:{connectionSettings.UseTls})");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "Property", "Value".PadRight(15), "Version"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "--------", "-----".PadLeft(15, '-'), "------"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "enabled".PadRight(8), enabled_value?.PadLeft(15), client?.Property_enabled?.PropertyValue.Version));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "interval".PadRight(8), interval_value?.PadLeft(15), client?.Property_interval.PropertyValue?.Version));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "started".PadRight(8), client.Property_started.PropertyValue.ToShortTimeString().PadLeft(15), client?.Property_started?.Version));
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
            AppendLineWithPadRight(sb, $"ConnectionStatus: {client.Connection.IsConnected} [{lastDiscconectReason}]");
            AppendLineWithPadRight(sb, $"NuGet: {infoVersion}");
            AppendLineWithPadRight(sb, " ");
            return sb.ToString();
        }

        Console.SetCursorPosition(0, 0);
        Console.WriteLine(RenderData());
        screenRefresher = new Timer(RefreshScreen, this, 1000, 0);
    }
}