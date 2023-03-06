using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e;

internal class Producer
{
    IMqttClient mqttClient;

    public ICommand<string, string> EchoCommand;

    public Producer(IMqttClient client)
    {
        mqttClient = client;

        EchoCommand = new Command<string, string>(mqttClient, "echo");
        EchoCommand.OnMessage = async m =>
        {
            await Task.Delay(m.Length * 100);
            await Console.Out.WriteLineAsync("[Producer] Running Echo Command in client: " + client.Options.ClientId);
            return await Task.FromResult<string>(m + m);
        };
    }
}

internal class Consumer
{
    IMqttClient mqttClient;
    public RequestResponseBinder<string, string> echoCommand;

    public Consumer(IMqttClient client)
    {
        mqttClient = client;
        echoCommand = new RequestResponseBinder<string, string>(mqttClient, "echo");
    }
}


public class BrokerCommandFixture
{
    private ConnectionSettings TestCS(string clientId)
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

    [Fact]
    public async Task InvokeCommandWithDefaultTopics()
    {
        IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceOne"));
        IMqttClient producerClientTwo = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceTwo"));
        Producer p1 = new Producer(producerClientOne);
        Producer p2 = new Producer(producerClientTwo);

        IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer"));
        Consumer consumer = new Consumer(consumerClient);
        var respOne = await consumer.echoCommand.InvokeAsync("deviceOne", "Hello One");
        var respTwo = await consumer.echoCommand.InvokeAsync("deviceTwo", "Hello Two Loooonger ");

        Assert.Equal("Hello OneHello One", respOne);
        Assert.Equal("Hello Two Loooonger Hello Two Loooonger ", respTwo);

    }
}
