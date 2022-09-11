using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class HubMqttClientFixture
    {

        ConnectionSettings cs = new ConnectionSettings()
        {
            HostName = Environment.GetEnvironmentVariable("TestHubName"),
            DeviceId = "testdevice",
            SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))
        };

        IHubMqttClient? hubClient;


        [Fact]
        public async Task GetTwin()
        {
            hubClient = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(cs));
            var twin = await hubClient.GetTwinAsync();
            Assert.NotNull(twin);
            Assert.Contains("$version", twin);
        }

        [Fact]
        public async Task ReportProperty()
        {
            cs.DeviceId = "iothub-sample";
            hubClient = new HubMqttClient(await HubDpsFactory.CreateFromConnectionSettingsAsync(cs));
            var prop = new { name = "myProperty", updated = DateTime.Now.ToLongTimeString() };
            var v = await hubClient.ReportPropertyAsync(prop);
            var twin = await hubClient.GetTwinAsync();
            Assert.NotNull(twin);
            Assert.Contains(prop.updated, twin);

        }

    }
}
