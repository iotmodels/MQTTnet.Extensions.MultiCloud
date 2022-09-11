using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class TestHive
    {
        MqttClient? client;
        public TestHive()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }

        [Fact]
        public async Task UserNamePassword()
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var cs = new ConnectionSettings()
            {
                HostName = "f8826e3352314ca98102cfbde8aff20e.s2.eu.hivemq.cloud",
                UserName = "client1",
                Password = "Myclientpwd.000"

            };
            var connAck = await client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}