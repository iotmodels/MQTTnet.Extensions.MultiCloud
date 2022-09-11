
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.UnitTests.HubClient
{
    public class DesiredUpdatePropertyBinderFixture
    {
        private static string Stringify(object o) => System.Text.Json.JsonSerializer.Serialize(o);
        [Fact]
        public void ReceiveDesired()
        {
            var mqttClient = new MockMqttClient();
            var desiredBinder = new DesiredUpdatePropertyBinder<int>(mqttClient, "myProp")
            {
                OnProperty_Updated = async p =>
                {
                    p.Status = 222;
                    return await Task.FromResult(p);
                }
            };

            var desiredMsg = new Dictionary<string, object>
            {
                { "myProp", 1 },
                { "$version", 3 }
            };

            mqttClient.SimulateNewMessage("$iothub/twin/PATCH/properties/desired", Stringify(desiredMsg));
            Assert.StartsWith($"$iothub/twin/PATCH/properties/reported/?$rid=", mqttClient.topicRecceived);

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
