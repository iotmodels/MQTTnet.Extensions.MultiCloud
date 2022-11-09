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
            var cs = new ConnectionSettings()
            {
                IdScope = "0ne006CCDE4",
                DeviceId = "sasdpstest",
                SharedAccessKey = "s7g9fIUf4mNNo1m/Ge3hGQi56ZJG9KzsZ46xL7O/rbI="
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureDpsCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ClientCert()
        {
            var cs = new ConnectionSettings()
            {
                IdScope = "0ne006CCDE4",
                X509Key = "ca-device.pem|ca-device.key"
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureDpsCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task IntermediateCert()
        {
            var cs = new ConnectionSettings()
            {
                IdScope = "0ne006CCDE4",
                X509Key = "dev03.pem|dev03.key|1234"
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureDpsCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}