using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class HubWritablePropertyUTFJsonFixture
    {

        private static string Stringify(object o) => System.Text.Json.JsonSerializer.Serialize(o);
        [Fact]
        public void DesiredPropGetsTriggeredAndIsReportedBackWithAck()
        {
            var mqttClient = new MockMqttClient();
            var desiredBinder = new WritableProperty<int>(mqttClient, "myProp");

            desiredBinder.OnMessage = async p =>
            {
                return await Task.FromResult(new Ack<int>()
                {
                    Status = 222,
                    Version = desiredBinder.Version,
                    Value = p
                });
            };
            

            var desiredMsg = new Dictionary<string, object>
            {
                { "myProp", 1 },
                { "$version", 3 }
            };

            mqttClient.SimulateNewMessage("$iothub/twin/PATCH/properties/desired/?$rid=1&$version=3", Stringify(desiredMsg));
            Assert.StartsWith($"$iothub/twin/PATCH/properties/reported/?$rid=1&$version=3", mqttClient.topicRecceived);

            var expected = Stringify(new
            {
                myProp = new
                {
                    av = 3,
                    ac = 222,
                    value = 1,
                }
            });
            Assert.Equal(expected, mqttClient.payloadReceived);
        }
    }
}
