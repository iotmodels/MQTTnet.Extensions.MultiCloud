using dtmi_rido_pnp_sensehat;
using Iot.Device.SenseHat;
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

    private readonly TelemetryClient _telemetryClient;
    private readonly SenseHatFactory _senseHatFactory;
    private readonly ILogger<Device> _logger;
    private const int default_interval = 5;

    public Device(SenseHatFactory factory, TelemetryClient tc, ILogger<Device> logger)
    {
        _senseHatFactory = factory;
        _telemetryClient = tc;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client = await _senseHatFactory.CreateSenseHatClientAsync("cs", stoppingToken);

        client.Property_interval.OnMessage = Property_interval_UpdateHandler;
        client.Property_combineTelemetry.OnMessage = Property_combineTelemetry_UpdateHandler;
        client.Command_ChangeLCDColor.OnMessage = Cmd_ChangeLCDColor_Handler;


        await client.Property_sdkInfo.SendMessageAsync(SenseHatFactory.NuGetPackageVersion, stoppingToken);

        client.Property_interval.Value = default_interval;
        client.Property_combineTelemetry.Value = true;

        await client.Property_combineTelemetry.InitPropertyAsync(client.InitialState, true, stoppingToken);
        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);

        await client.Property_piri.SendMessageAsync($"os: {Environment.OSVersion}, proc: {RuntimeInformation.ProcessArchitecture}, clr: {Environment.Version}", stoppingToken);

        var netInfo = "eth: " + GetLocalIPv4();
        _telemetryClient.TrackTrace(netInfo);
        await client.Property_ipaddr.SendMessageAsync(netInfo, stoppingToken);
        
        //var tp = new TelemetryProtobuf<Telemetries>(client.Connection, string.Empty) ;

        double t1 = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                using SenseHat sh = new();
                _telemetryClient.TrackMetric("temp1", sh.Temperature.DegreesCelsius);
                if (client.Property_combineTelemetry.Value)
                {
                    await client.SendTelemetryAsync(new AllTelemetries
                    {
                        t1 = sh.Temperature.DegreesCelsius,
                        t2 = sh.Temperature2.DegreesCelsius,
                        h = sh.Humidity.Percent,
                        p = sh.Pressure.Pascals
                    }, stoppingToken);
                }
                else
                {
                    await client.Telemetry_t1.SendMessageAsync(sh.Temperature.DegreesCelsius, stoppingToken);
                    await client.Telemetry_t2.SendMessageAsync(sh.Temperature2.DegreesCelsius, stoppingToken);
                    await client.Telemetry_h.SendMessageAsync(sh.Humidity.Percent, stoppingToken);
                    await client.Telemetry_p.SendMessageAsync(sh.Pressure.Pascals, stoppingToken);
                }

            }
            else
            {
                _telemetryClient.TrackMetric("temp1", 2);
                t1 = GenerateSensorReading(t1, 10, 40);

                if (client.Property_combineTelemetry.Value)
                {

                    await client.SendTelemetryAsync(new AllTelemetries
                    {
                        t1 = t1,
                        t2 = GenerateSensorReading(t1, 5, 35),
                        h = Environment.WorkingSet / 1000000,
                        p = Environment.WorkingSet / 1000000
                    }, stoppingToken);
               
                }
                else
                {
                    await client.Telemetry_t1.SendMessageAsync(t1, stoppingToken);
                    await client.Telemetry_t2.SendMessageAsync(GenerateSensorReading(t1, 5, 35), stoppingToken);
                    await client.Telemetry_h.SendMessageAsync(Environment.WorkingSet / 1000000, stoppingToken);
                    await client.Telemetry_p.SendMessageAsync(Environment.WorkingSet / 1000000, stoppingToken);
                }
            }
            int interval = client!.Property_interval.Value;
            _logger.LogInformation("Waiting {interval} s to send telemetry", interval);
            await Task.Delay(interval * 1000, stoppingToken);
        }
    }

    private async Task<Ack<int>> Property_interval_UpdateHandler(int p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation($"New prop received");
        var ack = new Ack<int>();

        if (p > 0)
        {
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = client.Property_interval.Version;
            ack.Value = p;
            client.Property_interval.Value = p;
            //ack.LastReported = p.Value;
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

    private async Task<Ack<bool>> Property_combineTelemetry_UpdateHandler(bool p)
    {
        ArgumentNullException.ThrowIfNull(client);
        var ack = new Ack<bool>
        {
            Description = "desired notification accepted",
            Status = 200,
            Version = client.Property_combineTelemetry.Version,
            Value = p
        };
        client.Property_combineTelemetry.Value = p;
        //ack.LastReported = p.Value;
        return await Task.FromResult(ack);
    }

    private string oldColor = "white";
    private async Task<string> Cmd_ChangeLCDColor_Handler(string req)
    {
        _logger.LogInformation($"New Command received");
        var color = Color.FromName(req);

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
        {
            using SenseHat sh = new ();
            sh.Fill(color);
        }
        else
        {
            var orig = Console.BackgroundColor;
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), req, true);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(" ");
                await Task.Delay(100);
            }
            Console.BackgroundColor = orig;

        }
        string result = oldColor;
        oldColor = req;
        return result;
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

    private readonly Random random = new ();

    private double GenerateSensorReading(double currentValue, double min, double max)
    {
        double percentage = 15;
        double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
        value = Math.Max(value, min);
        value = Math.Min(value, max);
        return value;
    }
}
