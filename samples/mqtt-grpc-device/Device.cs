using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqtt_grpc_device_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mqtt_grpc_device
{
    internal class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;
        public Device(ILogger<Device> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }

        private mqtt_grpc_sample_device? client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IMqttClient connection = await BrokerClientFactory.CreateFromConnectionSettingsAsync(_configuration.GetConnectionString("cs"), true, stoppingToken);

            client = new mqtt_grpc_sample_device(connection);

            connection.DisconnectedAsync += MqttClient_DisconnectedAsync;
            connection.ConnectedAsync += MqttClient_ConnectedAsync;

            client.PropSetterInterval.OnCallbackDelegate = OnPropIntervalReceivedHandler;
            client.CommandEcho.OnCallbackDelegate = OnMqttCommandEchoReceivedHandler;
            client.CommandGetRuntimeStats.OnCallbackDelegate = OnCommandGetRuntimeStatsHanlder;

            client.Props.Interval = 30;
            client.Props.SdkInfo = BrokerClientFactory.NuGetPackageVersion;
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

        async Task<byte[]> OnCommandGetRuntimeStatsHanlder(byte[] payload)
        {
            var request = getRuntimeStatsRequest.Parser.ParseFrom(payload);
            _logger.LogInformation("Cmd getRuntimeStats Received: {req}", request.Mode.ToString());
            var responsePayload = new getRuntimeStatsResponse();
            if (request.Mode == getRuntimeStatsMode.Basic)
            {
                responsePayload.DiagResults.Add("machineName", Environment.MachineName);
            }
            if (request.Mode == getRuntimeStatsMode.Normal)
            {
                responsePayload.DiagResults.Add("machineName", Environment.MachineName);
                responsePayload.DiagResults.Add("OsVersion", Environment.OSVersion.ToString());
            }
            if (request.Mode == getRuntimeStatsMode.Full)
            {
                responsePayload.DiagResults.Add("machineName", Environment.MachineName);
                responsePayload.DiagResults.Add("OsVersion", Environment.OSVersion.ToString());
                responsePayload.DiagResults.Add("SDKInfo", BrokerClientFactory.NuGetPackageVersion);
            }
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
                    response.Value = Google.Protobuf.WellKnownTypes.Any.Pack(request);
                }
                else
                {
                    response.Value = Google.Protobuf.WellKnownTypes.Any.Pack(client.Props);
                    response.Description = "negative values not accepted";
                    response.Status = 403;
                }
            }
            await Task.Delay(1500);
            return response.ToByteArray();
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
    }
}
