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
                IdScope = "0ne006CCDE4",
                DeviceId = "testDpsDevice",
                SharedAccessKey = "nnCC1W/tfXYjmnVWhJqp6KNk8zROrObl+I9tW/wzKTM="
            };
            var client = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(client.IsConnected);
            Assert.Equal("rido.azure-devices.net", HubDpsFactory.ComputedSettings!.HostName);
            await client.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                        .Build());
        }


        [Fact]
        public async Task DpsCert()
        {
            var cs = new ConnectionSettings
            {
                IdScope = "0ne006CCDE4",
                X509Key = "dpsTestDevice.pem|dpsTestDevice.key"
            };
            var client = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(client.IsConnected);
            Assert.Equal("rido.azure-devices.net", HubDpsFactory.ComputedSettings!.HostName);
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
                DeviceId = "testdevice",
                SharedAccessKey = "MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA="
            };
            var client = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(client.IsConnected);
            Assert.Equal(Environment.GetEnvironmentVariable("TestHubName"), HubDpsFactory.ComputedSettings!.HostName);
            await client.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder()
                        .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                        .Build());
        }
    }
}
