using dtmi_rido_pnp_memmon.mqtt;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using pnp_memmon;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e
{
    public class BrokerE2EFixture
    {
        private readonly ConnectionSettings cs = new ConnectionSettings()
        {
            HostName = "localhost",
            ClientId = "e2e-device",
            TcpPort = 1883,
            UseTls = false,
            UserName = "user",
            Password = "password"
        };
        private readonly ConnectionSettings scs = new ConnectionSettings()
        {
            HostName = "localhost",
            ClientId = "e2e-app",
            TcpPort = 1883,
            UseTls = false,
            UserName = "user",
            Password = "password"
        };

        [Fact, Trait("e2e", "broker")]
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

        //[Fact, Trait("e2e", "broker")]
        //public async Task DeviceReadsInitialProps()
        //{
        //    PropertyAck<int> ack = new PropertyAck<int>("interval");
        //    var ta = await BrokerClientFactory.CreateFromConnectionSettingsAsync(scs);
        //    //ta.ApplicationMessageReceivedAsync += async m =>
        //    //{
        //    //    if (m.ApplicationMessage.Topic == "pnp/e2e-device/props/interval")
        //    //    {
        //    //        //ack = Json.FromString<PropertyAck<int>>(Encoding.UTF8.GetString(m.ApplicationMessage.Payload))!;
        //    //    }
        //    //    await Task.Yield();

        //    //};
        //    //await ta.SubscribeAsync("pnp/e2e-device/props/interval");

        //    await ta.PublishJsonAsync("pnp/e2e-device/props/interval/set", 3, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);

        //    cs.ModelId = Imemmon.ModelId;
        //    var td = new memmon(await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs));


        //    td.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;

        //    PropertyAck<int> Property_interval_UpdateHandler(PropertyAck<int> p)
        //    {
        //        ack.Description = "desired notification accepted from e2e";
        //        ack.Status = 200;
        //        ack.Version = p.Version;
        //        ack.Value = p.Value;
        //        ack.LastReported = p.Value;

        //        //td.Property_interval.PropertyValue = ack;
        //        //await td.Property_interval.ReportPropertyAsync();
        //        Assert.Equal(3, td.Property_interval.PropertyValue.Value); //TODO Task.Wait on Async
        //        Assert.Equal(200, ack.Status);

        //        return ack;
        //    };
        //    //Task.WaitAll(Property_interval_UpdateHandler);
        //}
    }
}
