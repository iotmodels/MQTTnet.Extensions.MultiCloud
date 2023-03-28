using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
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
            var telemetryBinder = new Telemetry<int>(mqttClient, "temp");
            await telemetryBinder.SendMessageAsync(2);
            Assert.Equal("devices/mock/messages/events/", mqttClient.topicRecceived);
            Assert.Equal("{\"temp\":2}", mqttClient.payloadReceived);
        }

        // TODO: check generic (object) ToBytes .. Telemetry should be aware of Modules

        //[Fact]
        //public async Task SendTelemetryToModule()
        //{
        //    var mqttClient = new MockMqttClient("mock/myModule");
        //    var hubMqttClient = new HubMqttClient(mqttClient);
        //    //var telemetryBinder = new Telemetry<int>(mqttClient, "temp") { TopicPattern = "devices/{clientId}/modules/{}/messages/events/" };
        //    await hubMqttClient.SendTelemetryAsync(new { temp = 2 });
        //    Assert.Equal("devices/mock/modules/myModule/messages/events/", mqttClient.topicRecceived);
        //    Assert.Equal("{\"temp\":2}", mqttClient.payloadReceived);
        //}
    }
}
