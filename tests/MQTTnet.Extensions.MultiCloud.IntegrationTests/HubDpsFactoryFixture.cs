using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class HubDpsFactoryFixture
    {
        [Fact]
        public async Task CheckComputedConnectionSettingsWhenUsingDPS()
        {
            var cs = new ConnectionSettings
            {
                IdScope = "0ne001F8884",
                DeviceId = "testDpsDevice",
                SharedAccessKey = "DFOYheu+mab5uynYpRBiIut/MqHA61EkirLJ9BBaJdE="
            };
            var client = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(client.IsConnected);
            Assert.Equal(Environment.GetEnvironmentVariable("TestHubName"), HubDpsFactory.ComputedSettings.HostName);
            await client.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                        .Build());
        }
        [Fact]
        public async Task CheckComputedConnectionSettingsWhenNotUsingDPS()
        {
            var cs = new ConnectionSettings
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                DeviceId = "testDpsDevice",
                SharedAccessKey = "DFOYheu+mab5uynYpRBiIut/MqHA61EkirLJ9BBaJdE="
            };
            var client = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(client.IsConnected);
            Assert.Equal(Environment.GetEnvironmentVariable("TestHubName"), HubDpsFactory.ComputedSettings.HostName);
            await client.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                        .Build());
        }
    }
}
