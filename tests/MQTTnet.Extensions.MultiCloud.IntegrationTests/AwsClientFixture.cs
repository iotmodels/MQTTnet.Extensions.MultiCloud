using MQTTnet.Client;

using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using System.Threading.Tasks;
using Xunit;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class AwsClientFixture
    {
        private readonly ConnectionSettings cs = new ConnectionSettings()
        {
            HostName = "a38jrw6jte2l2x-ats.iot.us-west-1.amazonaws.com",
            ClientId = "testdevice22",
            X509Key = "testdevice22.pem|testdevice22.key"
        };

        [Fact]
        public async Task GetUpdateShadow()
        {
            IMqttClient? client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
            await client!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build());
            Assert.True(client.IsConnected);
            var awsClient = new AwsMqttClient(client);
            var shadow = await awsClient.GetShadowAsync();
            Assert.NotNull(shadow);
            var updRes = await awsClient.UpdateShadowAsync(new
            {
                name = "rido3"
            });
            Assert.True(updRes > 0);
            await client.DisconnectAsync();
            Assert.False(client.IsConnected);
        }
    }
}
