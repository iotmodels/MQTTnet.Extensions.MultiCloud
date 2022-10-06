using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class TestAws
    {
        private readonly MqttClient? client;
        public TestAws()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
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
                HostName = "a38jrw6jte2l2x-ats.iot.us-west-1.amazonaws.com",
                ClientId = "testdevice22",
                X509Key = "testdevice22.pem|testdevice22.key"
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