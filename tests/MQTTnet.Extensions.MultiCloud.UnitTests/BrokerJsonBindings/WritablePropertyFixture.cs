using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.BrokerJsonBindings
{
    public class WritablePropertyFixture
    {
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

            mockMqtt.SimulateNewBinaryMessage("device/mock/props/aStringProp/set",
                new UTF8JsonSerializer().ToBytes("string value"));
            Assert.True(propReceived);
            Assert.Equal(0, wp.Version);
            Assert.Equal("string value", wp.Value);
            Assert.Equal("device/mock/props/aStringProp/ack", mockMqtt.topicRecceived);
            Assert.Equal("{\"av\":0,\"ac\":200,\"value\":\"string value\"}", mockMqtt.payloadReceived);

            propReceived = false;
            mockMqtt.SimulateNewBinaryMessage("device/mock/props/aStringProp/set",
              new UTF8JsonSerializer().ToBytes("second string value"));
            Assert.True(propReceived);
            Assert.Equal(1, wp.Version);
            Assert.Equal("second string value", wp.Value);
            Assert.Equal("device/mock/props/aStringProp/ack", mockMqtt.topicRecceived);
            Assert.Equal("{\"av\":1,\"ac\":200,\"value\":\"second string value\"}", mockMqtt.payloadReceived);

        }
    }
}
