using dtmi_rido_pnp_memmon;
using dtmi_rido_pnp_memmon.mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mosquitto_sample
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        public Device(ILogger<Device> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Connecting");
            //var cs = new ConnectionSettings() { HostName = "localhost", UseTls = false, TcpPort = 1883, UserName = "user", Password="password" , ClientId="memmon-mosq"};
            //var cs = new ConnectionSettings() 
            //{ 
            //    HostName = "localhost", 
            //    UseTls = true, 
            //    TcpPort = 8883, 
            //    UserName = "user", 
            //    Password = "password", 
            //    ClientId = "memmon-mosq",
            //    CaFile = "ca.pem"
            //};
            var cs = new ConnectionSettings()
            {
                HostName = "localhost",
                UseTls = true,
                TcpPort = 8884,
                X509Key = "ca-device.pem|ca-device.key",
                CaFile = "ca.pem"
            };
            //var mqtt = new MqttFactory().CreateMqttClient();
            //await mqtt.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs, true).Build(),  stoppingToken);
            var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true);
            _logger.LogWarning($"Connected {cs}");
            Imemmon mm = new memmon(mqtt);

            mm.Command_getRuntimeStats.OnCmdDelegate = req =>
            {
                _logger.LogWarning("Cmd received" + req.ToString());
                return Task.FromResult(new Cmd_getRuntimeStats_Response());
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                await mm.Telemetry_workingSet.SendTelemetryAsync(Environment.WorkingSet);
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
