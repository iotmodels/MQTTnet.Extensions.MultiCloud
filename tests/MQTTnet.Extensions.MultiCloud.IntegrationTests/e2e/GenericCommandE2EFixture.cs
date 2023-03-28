using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e
{
    public  class GenericCommandE2EFixture
    {
        private static ConnectionSettings TestCS(string clientId)
        {
            return new ConnectionSettings
            {
                HostName = "localhost",
                UseTls = false,
                TcpPort = 1883,
                UserName = "user",
                Password = "password",
                ClientId = clientId
            };
        }

        internal class Producer
        {
            readonly IMqttClient mqttClient;

            public GenericCommand EchoCommand;

            public Producer(IMqttClient client)
            {
                mqttClient = client;

                EchoCommand = new GenericCommand(mqttClient)
                {
                    //UnwrapRequest = false,
                    OnCmdDelegate = async m =>
                    {
                        await Task.Delay(m.CommandPayload!.Length * 100);
                        await Console.Out.WriteLineAsync("[Producer] Running Generic Command in client: " + client.Options.ClientId);
                        return await Task.FromResult(
                            new GenericCommandResponse() 
                            {
                                Status = 200,
                                ReponsePayload = m.CommandPayload + m.CommandPayload
                            });
                    }
                };
            }
        }

        internal class Consumer
        {
            readonly IMqttClient mqttClient;
            public GenericCommandClient echoCommand;

            public Consumer(IMqttClient client)
            {
                mqttClient = client;
                echoCommand = new GenericCommandClient(mqttClient);
            }
        }

        [Fact]
        public async Task InvokeCommandWithDefaultTopics()
        {
            IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceOne"));
            IMqttClient producerClientTwo = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceTwo"));
            _ = new Producer(producerClientOne);
            _ = new Producer(producerClientTwo);

            IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer"));
            Consumer consumer = new(consumerClient);
            var respTwo = await consumer.echoCommand.InvokeAsync("deviceTwo", new GenericCommandRequest() { CommandName = "echo", CommandPayload = "Hello Two Loooonger " });
            var respOne = await consumer.echoCommand.InvokeAsync("deviceOne", new GenericCommandRequest() { CommandName = "echo", CommandPayload = "Hello One" });

            Assert.Equal("Hello OneHello One", respOne.ReponsePayload);
            Assert.Equal("Hello Two Loooonger Hello Two Loooonger ", respTwo.ReponsePayload);

        }
    }
}
