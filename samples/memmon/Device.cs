using dtmi_rido_memmon;
using Humanizer;
using Microsoft.ApplicationInsights;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace memmon;

public class Device : BackgroundService
{
    private readonly TelemetryClient _telemetryClient;

    private readonly Stopwatch clock = Stopwatch.StartNew();
    private int telemetryCounter = 0;
    private int commandCounter = 0;
    private int twinRecCounter = 0;
    private int reconnectCounter = 0;

    private double telemetryWorkingSet = 0;
    private double managedMemory = 0;
    private const bool default_enabled = true;
    private const int default_interval = 500;

    private string lastDiscconectReason = string.Empty;

    private Imemmon client;
    private ConnectionSettings connectionSettings;
    private readonly MemMonFactory memmonFactory;

    private string infoVersion = string.Empty;
    private static string safeAIKey = string.Empty;
    public Device(TelemetryClient tc, MemMonFactory clientFactory)
    {
        _telemetryClient = tc;
        memmonFactory = clientFactory;

        safeAIKey = string.IsNullOrEmpty(_telemetryClient.TelemetryConfiguration.InstrumentationKey) ?
                    string.Empty :
                    _telemetryClient.TelemetryConfiguration.InstrumentationKey.Substring(0, 6);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        infoVersion = MemMonFactory.NuGetPackageVersion;
        client = await memmonFactory.CreateMemMonClientAsync("cs", stoppingToken);
        connectionSettings = MemMonFactory.connectionSettings;
        client.Connection.DisconnectedAsync += Connection_DisconnectedAsync;

        _telemetryClient.TrackEvent("Connected", new Dictionary<string, string> { 
            { "ClientId", client.Connection.Options.ClientId},
            { "HostName", connectionSettings.HostName },
            { "Auth", connectionSettings.Auth.ToString()}
        });
        
        client.Property_enabled.OnMessage = Property_enabled_UpdateHandler;
        client.Property_interval.OnMessage= Property_interval_UpdateHandler;
        client.Command_getRuntimeStats.OnMessage= Command_getRuntimeStats_Handler;
        client.Command_isPrime.OnMessage = Command_isPrime_Handler;
        client.Command_malloc.OnMessage = Command_malloc_Hanlder;
        client.Command_free.OnMessage = Command_free_Hanlder;

        client.Property_started.InitProperty(client.InitialState);

        client.Property_timesRestarted.InitProperty(client.InitialState);
        client.Property_timesRestarted.Value++;
        await client.Property_timesRestarted.SendMessageAsync(stoppingToken);
        

        await client.Property_started.SendMessageAsync(DateTime.Now, stoppingToken);

        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        await client.Property_enabled.InitPropertyAsync(client.InitialState, default_enabled, stoppingToken);
         

        RefreshScreen(this);

        while (!stoppingToken.IsCancellationRequested)
        {
            telemetryWorkingSet = Environment.WorkingSet.Bytes().Megabytes;
            managedMemory = GC.GetTotalMemory(true).Bytes().Megabytes;
            if (client.Property_enabled.Value == true)
            {
                await client.Telemetry_workingSet.SendMessageAsync(telemetryWorkingSet, stoppingToken);
                await client.Telemetry_managedMemory.SendMessageAsync(managedMemory, stoppingToken);
                telemetryCounter++;
                _telemetryClient.TrackMetric("WorkingSet", telemetryWorkingSet);
                _telemetryClient.TrackMetric("managedMemory", managedMemory);
            }
            await Task.Delay(client.Property_interval.Value, stoppingToken);
        }
    }

    private async Task Connection_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
    {
        Trace.TraceError("Client Disconnected: " + arg.ReasonString);
        
        _telemetryClient.TrackEvent("Disconnected", new Dictionary<string, string> { 
            { "ClientId", client.Connection.Options.ClientId },
            { "Reason", arg.ReasonString }
        });
        _telemetryClient.TrackTrace("Client Disconnected: " + arg.ReasonString);
        if (arg.Exception != null)
        {
            _telemetryClient.TrackException(arg.Exception);
            throw arg.Exception;
        }

        lastDiscconectReason = arg.ReasonString;
        reconnectCounter++;
        await Task.Yield();
    }



    private async Task<Ack<bool>> Property_enabled_UpdateHandler(bool p)
    {
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", "enables" },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });

        var ack = new Ack<bool>
        {
            Description = "desired notification accepted",
            Status = 200,
            Version = client.Property_enabled.Version,
            Value = p,
        };
        client.Property_enabled.Value = p;
        return await Task.FromResult(ack);
    }

    private async Task<Ack<int>> Property_interval_UpdateHandler(int p)
    {
        ArgumentNullException.ThrowIfNull(client);
        twinRecCounter++;
        _telemetryClient.TrackEvent("DesiredPropertyReceived", new Dictionary<string, string>()
        {
            { "PropName", "interval" },
            { "NumTwinUpdates", twinRecCounter.ToString() }
        });
        
        var ack = new Ack<int>();
        
        if (p > 0)
        {
            client.Property_interval.Value = p;
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = client.Property_interval.Version;
            ack.Value = p;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Version = client.Property_enabled.Version;
            ack.Value = client.Property_interval.Value;
        };
        return await Task.FromResult(ack);
    }

    private async Task<bool> Command_isPrime_Handler(int number)
    {
        commandCounter++;
        IEnumerable<string> Multiples(int number)
        {
            return from n1 in Enumerable.Range(2, number / 2)
                   from n2 in Enumerable.Range(2, n1)
                   where n1 * n2 == number
                   select $"{n1} x {n2} => {number}";
        }
        
        bool result =  Multiples(number).Any();
        return await Task.FromResult(!result);

    }

    List<string> memory = new();
    IntPtr memoryPtr = IntPtr.Zero;
    private async Task<string> Command_malloc_Hanlder(int number)
    {
        commandCounter++;
        for (int i = 0; i < number; i++)
        {
            memory.Add(i.ToOrdinalWords());
        }

        memoryPtr = Marshal.AllocHGlobal(number);
        return await Task.FromResult(string.Empty);
    }

    private async Task<string> Command_free_Hanlder(string empty)
    {
        commandCounter++;
        await _telemetryClient.FlushAsync(CancellationToken.None);
        memory = new List<string>();
        GC.Collect(2, GCCollectionMode.Forced, false);
        if (memoryPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(memoryPtr);
            memoryPtr = IntPtr.Zero;
        }    
        return await Task.FromResult(string.Empty);
    }


    private async Task<Dictionary<string, string>> Command_getRuntimeStats_Handler(DiagnosticsMode req)
    {
        commandCounter++;
        _telemetryClient.TrackEvent("CommandReceived", new Dictionary<string, string>()
        {
            { "CommandName", "getRuntimeStats" },
            { "NumCommands", commandCounter.ToString() }
        });
        
        Dictionary<string, string> result = new()
        {
            { "machine name", Environment.MachineName },
            { "os version", Environment.OSVersion.ToString() },
            { "started", TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3) }
        };

        if (req == DiagnosticsMode.complete)
        {
            result.Add("sdk info:", infoVersion);
            result.Add("dotnet version:", Environment.Version.ToString());
        }
        if (req == DiagnosticsMode.full)
        {
            result.Add("sdk info:", infoVersion);
            result.Add("dotnet version:", Environment.Version.ToString());
            result.Add("interval: ", client.Property_interval.Value.ToString());
            result.Add("enabled: ", client.Property_enabled.Value.ToString());
            result.Add("twin receive: ", twinRecCounter.ToString());
            //result.diagnosticResults.Add($"twin sends: ", RidCounter.Current.ToString());
            result.Add("telemetry: ", telemetryCounter.ToString());
            result.Add("command: ", commandCounter.ToString());
            result.Add("reconnects: ", reconnectCounter.ToString());
            result.Add("workingSet", Environment.WorkingSet.Bytes().ToString());
            result.Add("GC Memmory", GC.GetTotalAllocatedBytes().Bytes().ToString());
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

            string enabled_value = client?.Property_enabled?.Value.ToString();
            string interval_value = client?.Property_interval?.Value.ToString();
            StringBuilder sb = new();
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"{connectionSettings?.HostName}:{connectionSettings?.TcpPort}");
            AppendLineWithPadRight(sb, $"{connectionSettings.ClientId} (Auth:{connectionSettings.Auth}/ TLS:{connectionSettings.UseTls}) GW: {connectionSettings.GatewayHostName}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "Property".PadRight(15), "Value".PadRight(15), "Version"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "-".PadRight(15, '-'), "-----".PadLeft(15, '-'), "------"));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "enabled".PadRight(15), enabled_value?.PadLeft(15), client?.Property_enabled?.Version));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "interval".PadRight(15), interval_value?.PadLeft(15), client?.Property_interval.Version));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "timesRestarted".PadRight(15), client.Property_timesRestarted.Value.ToString().PadLeft(15), client?.Property_timesRestarted.Version));
            AppendLineWithPadRight(sb, string.Format("{0:8} | {1:15} | {2}", "started".PadRight(15), client.Property_started.Value.ToShortTimeString().PadLeft(15), client?.Property_started?.Version));
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Reconnects: {reconnectCounter}");
            AppendLineWithPadRight(sb, $"Telemetry: {telemetryCounter}");
            AppendLineWithPadRight(sb, $"Twin receive: {twinRecCounter}");
            //AppendLineWithPadRight(sb, $"Twin send: {RidCounter.Current}");
            AppendLineWithPadRight(sb, $"Command messages: {commandCounter}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"WorkingSet: {telemetryWorkingSet} MB");
            AppendLineWithPadRight(sb, $"ManagedMemory: {managedMemory} MB");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Time Running: {TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3)}");
            AppendLineWithPadRight(sb, $"ConnectionStatus: {client.Connection.IsConnected} [{lastDiscconectReason}]");
            AppendLineWithPadRight(sb, $"NuGet: {infoVersion}");
            AppendLineWithPadRight(sb, $".NET: {Environment.Version}");
            AppendLineWithPadRight(sb, $"AppInsights: Enabled [{_telemetryClient.IsEnabled()}] InstrKey [{safeAIKey}]");
            AppendLineWithPadRight(sb, " ");
            return sb.ToString();
        }
        if (!client.Connection.IsConnected) throw new MqttClientDisconnectedException(null);
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(RenderData());
        _telemetryClient.Flush();
        screenRefresher = new Timer(RefreshScreen, this, 1000, 0);
    }
}