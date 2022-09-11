using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class IoTHubConnectionFixture
    {
        MqttClient? client;
        public IoTHubConnectionFixture()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }

        [Fact]
        public async Task DeviceSas()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                DeviceId = "testdevice",
                SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))

            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ModuleSas()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                DeviceId = "testdevice",
                ModuleId = "testmodule",
                SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task DeviceCert()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                X509Key = "ca-device.pem|ca-device.key"
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ModuleCert()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                X509Key = "ca-module.pem|ca-module.key"
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}