using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqtt_grpc_device_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mqtt_grpc_device
{
    internal class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;
        IMqttClient? connection;
        mqtt_grpc_sample_device? client;

        public Device(ILogger<Device> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionSettings cs = new(_configuration.GetConnectionString("cs")) { ModelId = mqtt_grpc_sample_device.ModelId };
            connection = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, stoppingToken);

            client = new mqtt_grpc_sample_device(connection);

            connection.DisconnectedAsync += MqttClient_DisconnectedAsync;
            connection.ConnectedAsync += MqttClient_ConnectedAsync;

            client.Interval.OnMessage = OnPropIntervalReceivedHandler;
            client.Echo.OnMessage = OnMqttCommandEchoReceivedHandler;
            client.GetRuntimeStats.OnMessage = OnCommandGetRuntimeStatsHanlder;

            client.Props.Interval = 30;
            client.Props.SdkInfo = BrokerClientFactory.NuGetPackageVersion;
            client.Props.Started = DateTime.UtcNow.ToTimestamp();
            await client.AllProperties.SendMessageAsync(client.Props, stoppingToken);

            var lastTemperature = 21.0;
            while (!stoppingToken.IsCancellationRequested)
            {
                lastTemperature = SensorReadings.GenerateSensorReading(lastTemperature, 12, 42);
                var telemetries = new Telemetries()
                {
                    Temperature = lastTemperature,
                    WorkingSet = Environment.WorkingSet / 500000
                };
                await client.AllTelemetries.SendMessageAsync(telemetries);
                _logger.LogInformation("LastTemp {lastTemp}. Waiting {interval}s", lastTemperature, client.Props.Interval);
                await Task.Delay(client.Props.Interval * 1000, stoppingToken);
            }
        }

        async Task<echoResponse> OnMqttCommandEchoReceivedHandler(echoRequest request)
        {
            _logger.LogInformation("Cmd echo Received: {req}", request.InEcho);
            var responsePayload = new echoResponse()
            {
                OutEcho = request.InEcho + " " + request.InEcho
            };
            return await Task.FromResult(responsePayload);
        }

        async Task<getRuntimeStatsResponse> OnCommandGetRuntimeStatsHanlder(getRuntimeStatsRequest request)
        {
            _logger.LogInformation("Cmd getRuntimeStats Received: {req}", request.Mode.ToString());
            var response = new getRuntimeStatsResponse();
            if (request.Mode == getRuntimeStatsMode.Basic)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
            }
            if (request.Mode == getRuntimeStatsMode.Normal)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
                response.DiagResults.Add("OsVersion", Environment.OSVersion.ToString());
            }
            if (request.Mode == getRuntimeStatsMode.Full)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
                response.DiagResults.Add("OsVersion", Environment.OSVersion.ToString());
                response.DiagResults.Add("SDKInfo", BrokerClientFactory.NuGetPackageVersion);
            }
            return await Task.FromResult(response);
        }
        async Task<ack> OnPropIntervalReceivedHandler(Properties desired)
        {
            ArgumentNullException.ThrowIfNull(connection);
            ack ack = new ack();
            client!.Interval.Version++;
            if (desired.Interval > 0)
            {
                client!.Props.Interval= desired.Interval;
                ack.Status = 200;
                ack.Version = client.Interval.Version!.Value;
                ack.Description = "property accepted";
                ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(desired);
            }
            else
            {
                ack.Status = 200;
                ack.Version = client.Interval.Version!.Value;
                ack.Description = "negative values not accepted";
                ack.Value = Google.Protobuf.WellKnownTypes.Any.Pack(client!.Props);
            }
            return await Task.FromResult(ack);
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

