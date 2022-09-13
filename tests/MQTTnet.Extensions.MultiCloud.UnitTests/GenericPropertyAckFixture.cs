using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class GenericPropertyAckFixture
    {
        [Fact]
        public void BuildAck()
        {
            GenericPropertyAck ack = new GenericPropertyAck()
            {
                Status =200,
                Value = Json.Stringify(new {myProp = "myVal"})
            };
            var jsonAck = ack.BuildAck();
            Assert.Equal("", jsonAck);
        }
    }
}
