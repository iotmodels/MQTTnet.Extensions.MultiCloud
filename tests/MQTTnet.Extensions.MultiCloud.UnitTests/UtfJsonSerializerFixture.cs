using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests;

public class SerializersFixture
{
    [Fact]
    public void TryDeserializeOk()
    {
        Utf8JsonSerializer ser = new();
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

    [Fact]
    public void Serialize_Strings()
    {
        Utf8StringSerializer ser = new();
        var bytes = ser.ToBytes("hola");
        Assert.Equal("hola"u8.ToArray(), bytes);
    }

    [Fact]
    public void DeSerialize_Strings()
    {

        Utf8StringSerializer ser = new();
        var hola = "hola"u8.ToArray();
        if (ser.TryReadFromBytes<string>(hola, string.Empty, out string res))
        {
            Assert.Equal("hola", res);
        }
        else
        {
            Assert.Fail("Deserialize failed");
        }
    }
}
