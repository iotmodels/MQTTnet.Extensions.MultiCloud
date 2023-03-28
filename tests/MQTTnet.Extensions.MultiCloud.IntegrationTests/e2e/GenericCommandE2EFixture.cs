using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e;

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
                OnCmdDelegate = async req =>
                {
                    await Task.Delay(req.CommandPayload!.ToString()!.Length * 100);
                    await Console.Out.WriteLineAsync("[Producer] Running Generic Command in client: " + client.Options.ClientId);
                    return await Task.FromResult(
                        new GenericCommandResponse() 
                        {
                            Status = 200,
                            ReponsePayload = req.CommandPayload.ToString() + req.CommandPayload.ToString()
                        });
                }
            };
        }
    }

    internal class Consumer
    {
        readonly IMqttClient mqttClient;
        public GenericCommandClient mqttCommand;

        public Consumer(IMqttClient client)
        {
            mqttClient = client;
            mqttCommand = new GenericCommandClient(mqttClient);
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
        var respOne = await consumer.mqttCommand.InvokeAsync("deviceOne", new GenericCommandRequest() { CommandName = "echo", CommandPayload = "Hello One" });
        var respTwo = await consumer.mqttCommand.InvokeAsync("deviceTwo", new GenericCommandRequest() { CommandName = "echo", CommandPayload = "Hello Two Loooonger " });

        Assert.Equal("Hello OneHello One", respOne.ReponsePayload);
        Assert.Equal("Hello Two Loooonger Hello Two Loooonger ", respTwo.ReponsePayload);

    }
}
