using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using System.Collections.Generic;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class GetTwinBinderFixture
    {
        private readonly MockMqttClient mockClient;
        private readonly GetTwinBinder binder;
        private readonly UpdateTwinBinder<object> updBinder;

        public GetTwinBinderFixture()
        {
            mockClient = new MockMqttClient();
            //binder = new TwinRequestResponseBinder(mockClient);
            binder = new GetTwinBinder(mockClient);
            updBinder = new UpdateTwinBinder<object>(mockClient);

        }

        [Fact]
        public void GetTwinAsync()
        {
            var twinTask = binder.InvokeAsync(mockClient.Options.ClientId, string.Empty);
            var rid = binder.lastRid;
            mockClient.SimulateNewMessage($"$iothub/twin/res/200/?$rid={rid}", SampleTwin);
            Assert.StartsWith("$iothub/twin/GET/?$rid=", mockClient.topicRecceived);
            Assert.Empty(mockClient.payloadReceived);
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
