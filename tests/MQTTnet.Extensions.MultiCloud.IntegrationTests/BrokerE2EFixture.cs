using dtmi_rido_pnp_memmon.mqtt;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using pnp_memmon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class BrokerE2EFixture
    {
        ConnectionSettings cs = new ConnectionSettings()
        {
            HostName = "localhost",
            ClientId = "e2e-device",
            TcpPort = 1883,
            UseTls = false,
            UserName = "user",
            Password = "password"
        };

        ConnectionSettings scs = new ConnectionSettings()
        {
            HostName = "localhost",
            ClientId = "e2e-app",
            TcpPort = 1883,
            UseTls = false,
            UserName = "user",
            Password = "password"
        };

        [Fact]
        public async Task DeviceSendsBirth()
        {
            BirthConvention.BirthMessage? bm = null;
            var birthFound = false;
            var ta = await BrokerClientFactory.CreateFromConnectionSettingsAsync(scs);
            ta.ApplicationMessageReceivedAsync += async m =>
            {

                birthFound = true;
                var jsonString = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                bm = Json.FromString<BirthConvention.BirthMessage>(jsonString);
                await Task.Yield();

            };
            await ta.SubscribeAsync("pnp/e2e-device/birth");
            cs.ModelId = Imemmon.ModelId;
            var td = new memmon(await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs));
            await Task.Delay(100);
            Assert.True(birthFound);
            Assert.Equal(Imemmon.ModelId, bm!.ModelId);
            Assert.Equal(BirthConvention.ConnectionStatus.online, bm!.ConnectionStatus);
            
            // TODO simulate disconnection, or LWT
            
        }
    }
}
