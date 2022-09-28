using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class HubTelemetryUTF8JsonFixture
    {
        [Fact]
        public async Task SendTelemetry()
        {
            var mqttClient = new MockMqttClient();
            var telemetryBinder = new HubTelemetryUTF8Json<int>(mqttClient, "temp");
            await telemetryBinder.SendMessageAsync(2);
            Assert.Equal("devices/mock/messages/events/", mqttClient.topicRecceived);
            Assert.Equal("{\"temp\":2}", mqttClient.payloadReceived);
        }
    }
}
