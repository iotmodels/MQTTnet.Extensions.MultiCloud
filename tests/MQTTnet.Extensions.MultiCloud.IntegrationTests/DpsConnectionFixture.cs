using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class DpsConnectionFixture
    {
        private readonly MqttClient? client;
        public DpsConnectionFixture()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }

        [Fact]
        public async Task SasAuth()
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var cs = new ConnectionSettings()
            {
                IdScope = "0ne001F8884",
                DeviceId = "sasdpstest",
                SharedAccessKey = "s7g9fIUf4mNNo1m/Ge3hGQi56ZJG9KzsZ46xL7O/rbI="
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureDpsCredentials(cs)
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
                IdScope = "0ne001F8884",
                X509Key = "ca-device.pem|ca-device.key"
            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureDpsCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}