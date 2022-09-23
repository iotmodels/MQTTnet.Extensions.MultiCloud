using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using mqtt_grpc_device_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace mqtt_grpc_device
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;

        mqtt_grpc_sample_device? client;
        

        public Device(ILogger<Device> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IMqttClient connection = await BrokerClientFactory.CreateFromConnectionSettingsAsync(_configuration.GetConnectionString("cs"), stoppingToken);
            client = new mqtt_grpc_sample_device(connection);

            connection.DisconnectedAsync += MqttClient_DisconnectedAsync;
            connection.ConnectedAsync += MqttClient_ConnectedAsync;

            client.PropSetterInterval.OnCallbackDelegate = OnPropIntervalReceivedHandler;
            client.CommandEcho.OnCallbackDelegate = OnMqttCommandEchoReceivedHandler;

            client.Props.Interval = 30;
            //client.Props.SdkInfo = GrpcMqttClientFactory.NuGetPackageVersion;
            client.Props.Started = DateTime.UtcNow.ToTimestamp();
            await client.ReportPropertiesAsync(stoppingToken);

           

            var lastTemperature = 21.0;
            while (!stoppingToken.IsCancellationRequested)
            {
                lastTemperature = SensorReadings.GenerateSensorReading(lastTemperature, 12, 42);
                var telemetries = new Telemetries() 
                { 
                    Temperature = lastTemperature, 
                    WorkingSet = Environment.WorkingSet 
                };
                await client.SendTelemetryAsync(telemetries, stoppingToken);
                _logger.LogInformation("LastTemp {lastTemp}. Waiting {interval}s", lastTemperature, client.Props.Interval);
                await Task.Delay(client.Props.Interval * 1000, stoppingToken);
            }
        }

        private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.LogWarning("Client Connected {reason}", arg.ConnectResult);
            await Task.Yield();
        }

        private async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogError("Client Disconnected {reason}", arg.ReasonString);
            if (arg.Exception != null)
            {
                _logger.LogCritical(arg.Exception, "Connection Exception");
            }
            await Task.Yield();
        }

        async Task<byte[]> OnMqttCommandEchoReceivedHandler(byte[] requestPayload)
        {
            var request = echoRequest.Parser.ParseFrom(requestPayload);
            _logger.LogInformation("Cmd echo Received: {req}", request.InEcho);
            var responsePayload = new echoResponse()
            {
                OutEcho = request.InEcho + " " + request.InEcho
            };
            return await Task.FromResult(responsePayload.ToByteArray());
        }

        async Task<byte[]> OnPropIntervalReceivedHandler(byte[] requestPayload)
        {
            ArgumentNullException.ThrowIfNull(client);
            var response = new ack()
            {
                Description = "intialize",
                Status = 0
            };
            if (requestPayload != null)
            {
                var request = Properties.Parser.ParseFrom(requestPayload);
                _logger.LogInformation("Prop interval Received: {req}", request.Interval);
                if (request.Interval > 0)
                {
                    client.Props.Interval = request.Interval;
                    await client.ReportPropertiesAsync();
                    response.Description = "property accepted";
                    response.Status = 200;
                }
                else
                {
                    response.Description = "negative values not accepted";
                    response.Status = 403;
                }
            }
            return response.ToByteArray();
        }
    }
}