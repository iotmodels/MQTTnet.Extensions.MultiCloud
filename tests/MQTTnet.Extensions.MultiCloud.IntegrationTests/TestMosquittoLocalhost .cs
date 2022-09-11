using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    // docker run -it --rm -p 8080:8080 -p 1883:1883 -p 8883:8883 -p 8884:8884 -p 8443:8443  ridomin/mosquitto-local:dev
    public class TestMosquittoLocalhost
    {
        MqttClient? client;
        public TestMosquittoLocalhost()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }


        [Fact]
        public async Task FailsWithouCA()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = "localhost",
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
                HostName = "localhost",
                TcpPort = 8883,
                CaFile = "RidoFY23CA.crt",
                UserName = "user",
                Password = "password"
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
                HostName = "localhost",
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