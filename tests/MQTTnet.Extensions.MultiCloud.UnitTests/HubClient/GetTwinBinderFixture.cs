using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.UnitTests;
using System.Collections.Generic;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class GetTwinBinderFixture
    {
        private readonly MockMqttClient mockClient;
        private readonly GetTwinBinder binder;

        public GetTwinBinderFixture()
        {
            mockClient = new MockMqttClient();
            binder = new GetTwinBinder(mockClient);
        }

        [Fact]
        public void GetTwinAsync()
        {
            var twinTask = binder.ReadPropertiesDocAsync();
            var rid = binder.lastRid;
            mockClient.SimulateNewMessage($"$iothub/twin/res/200/?$rid={rid}", SampleTwin);
            Assert.StartsWith("$iothub/twin/GET/?$rid=", mockClient.topicRecceived);
            Assert.Equal(string.Empty, mockClient.payloadReceived);
            var twin = twinTask.Result;
            Assert.Equal(twin, SampleTwin);
        }

        private static string Stringify(object o) => System.Text.Json.JsonSerializer.Serialize(o);

        private static string SampleTwin
        {
            get => Stringify(new
            {
                reported = new
                {
                    myProp = "myValue"
                },
                desired = new Dictionary<string, object>() { { "$version", 1 } },
            });

        }
    }
}
