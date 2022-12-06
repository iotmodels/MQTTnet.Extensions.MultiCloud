using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient;

public class ReadOnlyPropertyFixture
{
    MockMqttClient mqttClient;

    public ReadOnlyPropertyFixture()
    {
        mqttClient = new MockMqttClient();
    }

    [Fact]
    public void InitPropertyFound()
    {
        var rop = new ReadOnlyProperty<int>(mqttClient, "myIntProp");
        string json = """
            {
                "reported" : {
                    "myIntProp" : 2,
                    "$version" : 32
                }
            }
            """;
        rop.InitProperty(json);
        Assert.Equal(2, rop.Value);
        Assert.Equal(32, rop.Version);
    }

    [Fact]
    public void InitPropertyNotFound()
    {
        var rop = new ReadOnlyProperty<int>(mqttClient, "myIntProp");
        string json = """
            {
                "reported" : {
                    "$version" : 1
                }
            }
            """;
        rop.InitProperty(json);
        Assert.Equal(default(int), rop.Value);
        Assert.Equal(0, rop.Value);
        Assert.Equal(1, rop.Version);
    }
}
