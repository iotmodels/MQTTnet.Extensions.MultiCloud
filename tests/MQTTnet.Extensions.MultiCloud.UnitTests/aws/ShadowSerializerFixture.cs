using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.aws
{
    public class ShadowSerializerFixture
    {
        [Fact]
        public void SerializeWithoutVersion()
        {
            ShadowSerializer ser = new();
            var bytes = ser.ToBytes(new { prop = 1});
            Assert.NotNull(bytes);
            Assert.Equal(10, bytes.Length);
            Assert.Equal(0,ser.Version);
        }

        [Fact]
        public void SerializeWithVersion()
        {
            ShadowSerializer ser = new();
            var bytes = ser.ToBytes(new { prop = 1 }, version: 3);
            Assert.NotNull(bytes);
            Assert.Equal(10, bytes.Length);
            Assert.Equal(3, ser.Version);
        }
    }
}
