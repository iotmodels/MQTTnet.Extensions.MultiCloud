using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Serializers;
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
            Assert.StartsWith($"$iothub/twin/PATCH/properties/reported/?$rid=1", mqttClient.topicRecceived);

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

        [Fact]
        public void ReceiveWPWithVersion()
        {
            MockMqttClient mockMqtt = new MockMqttClient();
            WritableProperty<string> wp = new WritableProperty<string>(mockMqtt, "aStringProp");
            Assert.Equal(-1, wp.Version);
            Assert.Null(wp.Value);
            bool propReceived = false;
            wp.OnMessage = async (message) =>
            {
                propReceived = true;
                wp.Value = message;
                wp.Version++;
                return await Task.FromResult(
                    new Ack<string>
                    {
                        Value = message,
                        Version = wp.Version,
                        Status = 200
                    });
            };

            mockMqtt.SimulateNewBinaryMessage("$iothub/twin/PATCH/properties/desired/?$rid=1&$version=3",
                new UTF8JsonSerializer().ToBytes(new { aStringProp = "string value" } ));
            Assert.True(propReceived);
            Assert.Equal(4, wp.Version);
            Assert.Equal("string value", wp.Value);
            Assert.Equal($"$iothub/twin/PATCH/properties/reported/?$rid=1", mockMqtt.topicRecceived);
            Assert.Equal("{\"aStringProp\":{\"av\":4,\"ac\":200,\"value\":\"string value\"}}", mockMqtt.payloadReceived);

            propReceived = false;
            mockMqtt.SimulateNewBinaryMessage("$iothub/twin/PATCH/properties/desired/?$rid=1&$version=4",
                new UTF8JsonSerializer().ToBytes(new { aStringProp = "second string value" }));
            Assert.True(propReceived);
            Assert.Equal(5, wp.Version);
            Assert.Equal("second string value", wp.Value);
            Assert.Equal($"$iothub/twin/PATCH/properties/reported/?$rid=1", mockMqtt.topicRecceived);
            Assert.Equal("{\"aStringProp\":{\"av\":5,\"ac\":200,\"value\":\"second string value\"}}", mockMqtt.payloadReceived);


        }
    }
}
