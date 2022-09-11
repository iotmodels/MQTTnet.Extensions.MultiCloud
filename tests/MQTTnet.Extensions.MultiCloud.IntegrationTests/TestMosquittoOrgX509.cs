using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class TestMosquittoOrg
    {
        MqttClient? client;
        public TestMosquittoOrg()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }

        [Fact(Skip = "random failures in Windows")]
        public async Task LetsEncrypt()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var cs = new ConnectionSettings()
            {
                HostName = "test.mosquitto.org",
                TcpPort = 8886,
                ClientId = "test-client",
                DisableCrl = true
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task FailsWithouCA()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = "test.mosquitto.org",
                TcpPort = 8883,
                ClientId = "test-client"
            };
            try
            {
                var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                    .WithConnectionSettings(cs)
                    .Build());
            }
            catch (MqttCommunicationException ex)
            {
                //Assert.Equal("The remote certificate was rejected by the provided RemoteCertificateValidationCallback.", ex.Message);
                Assert.Equal(-2146233088, ex.HResult);
            }
        }

        [Fact]
        public async Task ConfiguredCA()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = "test.mosquitto.org",
                TcpPort = 8883,
                ClientId = "test-client",
                CaFile = "mosquitto.org.crt"
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
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = "test.mosquitto.org",
                TcpPort = 8884,
                ClientId = "test-client",
                CaFile = "mosquitto.org.crt",
                X509Key = "client.pfx|1234"
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