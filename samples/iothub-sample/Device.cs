using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
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

            //var mqtt = await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionString);

            //int i = 0;
            //mqtt.ApplicationMessageReceivedAsync += async m =>
            //{
            //    Console.WriteLine($"msg {i++} received");
            //    Console.WriteLine(m.ApplicationMessage.Topic);
            //    Console.WriteLine(m.ApplicationMessage.PayloadFormatIndicator);
            //    Console.WriteLine(m.ApplicationMessage.Payload);
            //    await Task.Yield();
            //};
            //await mqtt.SubscribeAsync("$iothub/twin/res/#", cancellationToken: stoppingToken);
            //await mqtt.SubscribeAsync("$iothub/twin/res/#", cancellationToken: stoppingToken);
            //await mqtt.PublishStringAsync("$iothub/twin/PATCH/properties/reported/?$rid=1", Json.Stringify(new { hey = "ho"}));


            var client = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionString, stoppingToken));

            client.Connection.DisconnectedAsync += Connection_DisconnectedAsync;

            var v = await client.ReportPropertyAsync(new { started = "DateTime.Now" }, stoppingToken);

            var twin = await client.GetTwinAsync(stoppingToken);

            client.OnCommandReceived = async m =>
            {
                return await Task.FromResult(new CommandResponse()
                {
                    Status = 200,
                    ReponsePayload = JsonSerializer.Serialize(new { myResponse = "whatever" })
                });
            };

            client.OnPropertyUpdateReceived = m =>
            {
                _logger.LogInformation(m.ToJsonString());
                return new GenericPropertyAck
                {
                    Value = m.ToJsonString(),
                    Status = 200,
                    Version = m["$version"].GetValue<int>()
                };
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                //var puback = await client.SendTelemetryAsync(new { workingSet = Environment.WorkingSet }, stoppingToken);
                await Task.Delay(50000, stoppingToken);
            }
        }

        private async Task Connection_DisconnectedAsync(MQTTnet.Client.MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogCritical($"Client Disconnected: {arg.ReasonString}");
            await Task.Yield();

        }
    }
}
