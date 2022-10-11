using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests;

public class UtfJsonSerializerFixture
{
    [Fact]
    public void TryDeserializeOk()
    {
        UTF8JsonSerializer ser = new();
        byte[] payload = Encoding.UTF8.GetBytes(Json.Stringify(new { myBool = true }));
        if (ser.TryReadFromBytes(payload, "myBool", out bool propVal))
        {
            Assert.True(propVal);
        }
        else
        {
            Assert.Fail("prop not found");
        }

        if (ser.TryReadFromBytes(payload, "notFound", out bool propVal2))
        {
            Assert.Fail("bad found");
        }
        else
        {
            Assert.False(propVal2);
        }
    }
}
