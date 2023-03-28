using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e;

public class BrokerCommandFixture
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

        public ICommand<string, string> EchoCommand;

        public Producer(IMqttClient client)
        {
            mqttClient = client;

            EchoCommand = new Command<string, string>(mqttClient, "echo")
            {
                UnwrapRequest = false,
                OnMessage = async m =>
                {
                    await Task.Delay(m.Length * 100);
                    await Console.Out.WriteLineAsync("[Producer] Running Echo Command in client: " + client.Options.ClientId);
                    return await Task.FromResult<string>(m + m);
                }
            };
        }
    }

    internal class Consumer
    {
        readonly IMqttClient mqttClient;
        public RequestResponseBinder<string, string> echoCommand;

        public Consumer(IMqttClient client)
        {
            mqttClient = client;
            echoCommand = new RequestResponseBinder<string, string>(mqttClient, "echo", false);
        }
    }

    [Fact]
    public async Task InvokeCommandWithDefaultTopics()
    {
        IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("device11"));
        IMqttClient producerClientTwo = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("device22"));
        _ = new Producer(producerClientOne);
        _ = new Producer(producerClientTwo);

        IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer2"));
        Consumer consumer = new(consumerClient);
        var respOne = await consumer.echoCommand.InvokeAsync("device11", "Hello One");
        var respTwo = await consumer.echoCommand.InvokeAsync("device22", "Hello Two Loooonger ");

        Assert.Equal("Hello OneHello One", respOne);
        Assert.Equal("Hello Two Loooonger Hello Two Loooonger ", respTwo);

    }
}
