using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
        _logger.LogWarning("Connecting to: {connectionSettings}", connectionSettings);

        var client = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionSettings, stoppingToken));
        
        var v = await client.UpdateTwinAsync(new { started = DateTime.Now }, stoppingToken);
        _logger.LogInformation("Updated Twin to verison: {v}", v);
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
