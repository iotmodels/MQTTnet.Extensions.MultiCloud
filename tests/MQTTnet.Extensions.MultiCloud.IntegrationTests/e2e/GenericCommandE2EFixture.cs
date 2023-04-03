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

        public GenericCommand genCommand;

        public Producer(IMqttClient client)
        {
            mqttClient = client;

            genCommand = new GenericCommand(mqttClient)
            {
                OnCmdDelegate = async req =>
                {
                    await Console.Out.WriteLineAsync("[Producer] Running Generic Command in client: " + client.Options.ClientId);
                    if (req.CommandName == "echo") // req: string, resp: string
                    {
                        await Task.Delay(req.RequestPayload!.ToString()!.Length * 100);
                        return await Task.FromResult(
                            new GenericCommandResponse() 
                            {
                                Status = 200,
                                ReponsePayload = req.RequestPayload.ToString() + req.RequestPayload.ToString()
                            });
                    }
                    if (req.CommandName == "isPrime") // req: int, resp: bool
                    {
                        int number = int.Parse(req.RequestPayload!.ToString()!);
                        return await Task.FromResult(
                            new GenericCommandResponse()
                            {
                                Status = 200,
                                ReponsePayload = true
                            });
                    }
                    else
                    {
                        return await Task.FromResult(
                            new GenericCommandResponse()
                            {
                                Status = 400
                            });
                    }
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

        var respOne = await consumer.mqttCommand.InvokeAsync("deviceOne", 
            new GenericCommandRequest() { CommandName = "echo", RequestPayload = "Hello One", CorrelationId = new byte[] { 1 } });

        Assert.Equal("Hello OneHello One", respOne.ReponsePayload!.ToString());
        
        var respTwo = await consumer.mqttCommand.InvokeAsync("deviceTwo", 
            new GenericCommandRequest() { CommandName = "echo", RequestPayload = "Hello Two Loooonger ", CorrelationId = new byte[] { 2 } });
        Assert.Equal("Hello Two Loooonger Hello Two Loooonger ", respTwo.ReponsePayload!.ToString());

        await producerClientOne.DisconnectAsync();
        await producerClientTwo.DisconnectAsync();
        await consumerClient.DisconnectAsync();
    }

    [Fact]
    public async Task NotImplementedReturns400()
    {
        IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceThree"));
        _ = new Producer(producerClientOne);

        IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer2"));
        Consumer consumer = new(consumerClient);

        var respOne = await consumer.mqttCommand.InvokeAsync("deviceThree",
            new GenericCommandRequest() { CommandName = "notimpl", RequestPayload = "Hello One" });
        Assert.Equal(400, respOne.Status);

        var respTwo = await consumer.mqttCommand.InvokeAsync("deviceThree",
          new GenericCommandRequest() { CommandName = "notimpl" });
        Assert.Equal(400, respTwo.Status);

        await producerClientOne.DisconnectAsync();
        await consumerClient.DisconnectAsync();
    }

    [Fact]
    public async Task CallWithPrimitiveTypes()
    {
        IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("deviceFour"));
        _ = new Producer(producerClientOne);

        IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer4"));
        Consumer consumer = new(consumerClient);

        var respIsPrime = await consumer.mqttCommand.InvokeAsync("deviceFour", new GenericCommandRequest
        {
            CommandName = "isPrime",
            RequestPayload = 4567
        });
        Assert.True(Convert.ToBoolean(respIsPrime.ReponsePayload!.ToString()));
    }


}
