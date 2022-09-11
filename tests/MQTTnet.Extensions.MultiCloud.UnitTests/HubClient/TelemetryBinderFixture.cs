using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.UnitTests.HubClient
{
    public class TelemetryBinderFixture
    {
        [Fact]
        public async Task SendTelemetry()
        {
            var mqttClient = new MockMqttClient();
            var telemetryBinder = new Telemetry<int>(mqttClient, "temp");
            await telemetryBinder.SendTelemetryAsync(2);
            Assert.Equal("devices/mock/messages/events/", mqttClient.topicRecceived);
            Assert.Equal("{\"temp\":2}", mqttClient.payloadReceived);
        }

        [Fact]
        public async Task SendTelemetryWithComponent()
        {
            var mqttClient = new MockMqttClient();
            var telemetryBinder = new Telemetry<int>(mqttClient, "temp", "myComp");
            await telemetryBinder.SendTelemetryAsync(2);
            Assert.Equal("devices/mock/messages/events/$.sub=myComp", mqttClient.topicRecceived);
            Assert.Equal("{\"temp\":2}", mqttClient.payloadReceived);
        }
    }
}
