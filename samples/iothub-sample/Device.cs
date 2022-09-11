using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace iothub_sample
{
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
            var connectionString = _configuration.GetConnectionString("cs");
            _logger.LogInformation($"Connecting to: {new ConnectionSettings(connectionString)}");

            var client = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionString, stoppingToken));

                var v = await client.ReportPropertyAsync(new { started = DateTime.Now }, stoppingToken);
                var twin = await client.GetTwinAsync(stoppingToken);
            client.OnCommandReceived = async m =>
            {
                return await Task.FromResult(new CommandResponse()
                {
                    Status = 200,
                    ReponsePayload = JsonSerializer.Serialize(new { myResponse = "whatever" })
                });
            };

            client.OnPropertyUpdateReceived = async m =>
            {
                return await Task.FromResult(new GenericPropertyAck
                {
                    Value = m.ToJsonString(),
                    Status = 200,
                    Version = m["$version"].GetValue<int>()
                });
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var puback = await client.SendTelemetryAsync(new { workingSet = Environment.WorkingSet }, stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
