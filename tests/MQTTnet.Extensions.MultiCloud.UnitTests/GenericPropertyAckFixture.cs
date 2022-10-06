using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class GenericPropertyAckFixture
    {
        [Fact]
        public void BuildAck()
        {
            GenericPropertyAck ack = new()
            {
                Status = 200,
                Value = Json.Stringify(new { myProp = "myVal" })
            };
            var jsonAck = ack.BuildAck();
            Assert.Equal("{\"myProp\":{\"ac\":200,\"av\":0,\"ad\":null,\"value\":\"myVal\"}}", jsonAck);
        }
    }
}
