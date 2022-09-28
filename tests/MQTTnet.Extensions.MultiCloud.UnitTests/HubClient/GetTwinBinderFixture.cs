using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Collections.Generic;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class GetTwinBinderFixture
    {
        private readonly MockMqttClient mockClient;
        private readonly RequestResponseBinder binder;

        public GetTwinBinderFixture()
        {
            mockClient = new MockMqttClient();
            binder = new RequestResponseBinder(mockClient);
        }

        [Fact]
        public void GetTwinAsync()
        {
            var twinTask = binder.GetTwinAsync();
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
