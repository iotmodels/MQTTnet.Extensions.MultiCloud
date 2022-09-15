using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace iothub_sample;
public class Device : BackgroundService
{
    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    public Device(ILogger<Device> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionSettings = new ConnectionSettings(_configuration.GetConnectionString("cs"));
        _logger.LogWarning($"Connecting to: {connectionSettings}");

        var client = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionSettings, stoppingToken));
        await client.InitState();

        Console.Write(" Initial State: ");
        Console.WriteLine(client.InitialState);

        var v = await client.ReportPropertyAsync(new { started = DateTime.Now }, stoppingToken);
        Console.Write(" Updated Twin: ");
        var twin = await client.GetTwinAsync(stoppingToken);
        Console.WriteLine(twin);
        
        client.OnCommandReceived = m =>
        {
            Console.WriteLine(m.CommandName);
            Console.WriteLine(m.CommandPayload);
            return new GenericCommandResponse()
            {
                Status = 200,
                ReponsePayload = JsonSerializer.Serialize(new { myResponse = "whatever" })
            };
        };

        client.OnPropertyUpdateReceived = m =>
        {
            Console.WriteLine(m.ToString());

            return new GenericPropertyAck
            {
                Value = m.ToJsonString(),
                Status = 200,
                Version = m["$version"].GetValue<int>()
            };
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var puback = await client.SendTelemetryAsync(new { workingSet = Environment.WorkingSet }, stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }
}
