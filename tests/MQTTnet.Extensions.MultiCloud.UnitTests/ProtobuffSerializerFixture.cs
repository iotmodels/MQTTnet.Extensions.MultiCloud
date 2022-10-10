using Google.Protobuf;
using mqtt_grpc_device_protos;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class ProtobuffSerializerFixture
    {
        [Fact]
        public void TryDeserialize()
        {
            Properties props = new()
            {
                Interval = 3
            };
            ProtobufSerializer ser = new(Properties.Parser);
            byte[] payload = props.ToByteArray();
            if (ser.TryReadFromBytes(payload, "interval", out Properties propVal))
            {
                Assert.Equal(3, propVal.Interval);
            }
            else
            {
                Assert.Fail("incorrect prop found");
            }

            if (ser.TryReadFromBytes(payload, "sdkInfo", out Properties propVal2))
            {
                Assert.Fail("incorrect found");
            }
            else
            {
                Assert.Null(propVal2);
            }

        }
    }
}
