using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class AzPubSubConnectionFixture
    {
        [Fact]
        public async Task ConenctToAzPubSub()
        {
            ConnectionSettings cs = new()
            {
                HostName = "rido-pubsub.centraluseuap-1.ts.eventgrid.azure.net",
                X509Key = "client.pfx|1234"
            };
            var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs);
            Assert.True(mqtt.IsConnected);
            await mqtt.DisconnectAsync(new Client.MqttClientDisconnectOptions() { Reason = Client.MqttClientDisconnectReason.NormalDisconnection});
        }
    }
}
