using dtmi_rido_pnp_sensehat;
using Iot.Device.SenseHat;
using Iot.Device.Tlc1543;
using Microsoft.ApplicationInsights;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Color = System.Drawing.Color;

namespace pi_sense_device;

public class Device : BackgroundService
{
    private Isensehat? client;

    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;
    private TelemetryClient _telemetryClient;

    private const int default_interval = 5;

    ConnectionSettings connectionSettings;
    public Device(ILogger<Device> logger, IConfiguration configuration, TelemetryClient tc)
    {
        _logger = logger;
        _configuration = configuration;
        _telemetryClient = tc;
        connectionSettings = new ConnectionSettings(_configuration.GetConnectionString("cs"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("Connecting..");
        client = await new SenseHatFactory(_configuration).CreateSenseHatClientAsync(_configuration.GetConnectionString("cs"), stoppingToken);
        _logger.LogWarning($"Connected to {connectionSettings}");

        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;
        client.Property_combineTelemetry.OnProperty_Updated = Property_combineTelemetry_UpdateHandler;
        client.Command_ChangeLCDColor.OnCmdDelegate = Cmd_ChangeLCDColor_Handler;

        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        await client.Property_interval.ReportPropertyAsync(stoppingToken);

        await client.Property_combineTelemetry.InitPropertyAsync(client.InitialState, true, stoppingToken);
        await client.Property_combineTelemetry.ReportPropertyAsync(stoppingToken);

        client.Property_piri.PropertyValue = $"os: {Environment.OSVersion}, proc: {RuntimeInformation.ProcessArchitecture}, clr: {Environment.Version}";
        await client.Property_piri.ReportPropertyAsync(stoppingToken);

        var netInfo = "eth: " + GetLocalIPv4();
        client.Property_ipaddr.PropertyValue = netInfo;
        _telemetryClient.TrackTrace(netInfo);
        await client.Property_ipaddr.ReportPropertyAsync(stoppingToken);

        double t1 = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                using SenseHat sh = new SenseHat();
                _telemetryClient.TrackMetric("temp1", sh.Temperature.DegreesCelsius);
                if (client.Property_combineTelemetry.PropertyValue.Value)
                {
                    await client.SendTelemetryAsync(new AllTelemetries
                    {
                        t1 = sh.Temperature.DegreesCelsius,
                        t2 = sh.Temperature2.DegreesCelsius,
                        h = sh.Humidity.Percent,
                        p = sh.Pressure.Pascals
                    }, stoppingToken); ;
                }
                else
                {
                    await client.Telemetry_t1.SendTelemetryAsync(sh.Temperature.DegreesCelsius, stoppingToken);
                    await client.Telemetry_t2.SendTelemetryAsync(sh.Temperature2.DegreesCelsius, stoppingToken);
                    await client.Telemetry_h.SendTelemetryAsync(sh.Humidity.Percent, stoppingToken);
                    await client.Telemetry_p.SendTelemetryAsync(sh.Pressure.Pascals, stoppingToken);
                }

            }    
            else
            {
                _telemetryClient.TrackMetric("temp1", 2);
                t1 = GenerateSensorReading(t1, 10, 40);

                if (client.Property_combineTelemetry.PropertyValue.Value)
                {
                    await client.SendTelemetryAsync(new AllTelemetries
                    {
                        t1 = t1,
                        t2 = GenerateSensorReading(t1, 5, 35),
                        h = Environment.WorkingSet / 1000000,
                        p = Environment.WorkingSet / 1000000
                    }, stoppingToken);;
                }
                else
                {
                    await client.Telemetry_t1.SendTelemetryAsync(t1, stoppingToken);
                    await client.Telemetry_t2.SendTelemetryAsync(GenerateSensorReading(t1, 5, 35), stoppingToken);
                    await client.Telemetry_h.SendTelemetryAsync(Environment.WorkingSet / 1000000, stoppingToken);
                    await client.Telemetry_p.SendTelemetryAsync(Environment.WorkingSet / 1000000, stoppingToken);
                }
            }
            var interval = client?.Property_interval.PropertyValue?.Value;
            _logger.LogInformation($"Waiting {interval} s to send telemetry");
            await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
        }
    }

    private async Task<PropertyAck<int>> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation($"New prop received");
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
        return await Task.FromResult(ack);
    }

    private async Task<PropertyAck<bool>> Property_combineTelemetry_UpdateHandler(PropertyAck<bool> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        var ack = new PropertyAck<bool>(p.Name);
        ack.Description = "desired notification accepted";
        ack.Status = 200;
        ack.Version = p.Version;
        ack.Value = p.Value;
        ack.LastReported = p.Value;
        client.Property_combineTelemetry.PropertyValue = ack;
        return await Task.FromResult(ack);
    }

    string oldColor = "white";
    private async Task<Cmd_ChangeLCDColor_Response> Cmd_ChangeLCDColor_Handler(Cmd_ChangeLCDColor_Request req)
    {
        _logger.LogInformation($"New Command received");
        var color = Color.FromName(req.request);

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
        {
            using SenseHat sh = new SenseHat();
            sh.Fill(color);
        }
        else
        {
            var orig = Console.BackgroundColor;
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), req.request, true);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(" ");
                await Task.Delay(100);
            }
            Console.BackgroundColor = orig;

        }    
        var result = new Cmd_ChangeLCDColor_Response()
        {
            response = oldColor
        };
        oldColor = req.request;
        return await Task.FromResult(result);
    }

    internal static string GetLocalIPv4()
    {  // Checks your IP adress from the local network connected to a gateway. This to avoid issues with double network cards
        string output = "";  // default output
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) // Iterate over each network interface
        {  // Find the network interface which has been provided in the arguments, break the loop if found
            if (item.OperationalStatus == OperationalStatus.Up)
            {   // Fetch the properties of this adapter
                IPInterfaceProperties adapterProperties = item.GetIPProperties();
                // Check if the gateway adress exist, if not its most likley a virtual network or smth
                if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                {   // Iterate over each available unicast adresses
                    foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                    {   // If the IP is a local IPv4 adress
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {   // we got a match!
                            output = ip.Address.ToString();
                            break;  // break the loop!!
                        }
                    }
                }
            }
            // Check if we got a result if so break this method
            //if (output != "") { break; }
        }
        // Return results
        return output;
    }

    Random random = new Random();
    double GenerateSensorReading(double currentValue, double min, double max)
    {
        double percentage = 15;
        double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
        value = Math.Max(value, min);
        value = Math.Min(value, max);
        return value;
    }
}
