using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class TestMosquittoRidoDev
    {
        private readonly MqttClient? client;
        public TestMosquittoRidoDev()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }


        [Fact]
        public async Task NotFailsWithouCA()
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var cs = new ConnectionSettings()
            {
                HostName = "mosquitto.rido.dev",
                TcpPort = 8883,
                ClientId = "test-client",
                UserName = "client1",
                Password = "Pass@Word1"
            };

            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ConfiguredCA()
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var cs = new ConnectionSettings()
            {
                HostName = "mosquitto.rido.dev",
                TcpPort = 8883,
                UserName = "client1",
                Password = "Pass@Word1"
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ClientCert()
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var cs = new ConnectionSettings()
            {
                HostName = "mosquitto.rido.dev",
                TcpPort = 8884,
                ClientId = "test-client",
                CaFile = "RidoFY23CA.crt",
                X509Key = "ca-device.pem|ca-device.key"
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}