using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e;

public class BrokerPropertyFixture
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

        public IReadOnlyProperty<DateTime> Started;
        public IWritableProperty<int> Interval;

        public Producer(IMqttClient client)
        {
            mqttClient = client;
            Started = new ReadOnlyProperty<DateTime>(mqttClient, "started");
            Interval = new WritableProperty<int>(mqttClient, "interval");
        }
    }

    internal class Consumer
    {
        readonly IMqttClient mqttClient;
        public PropertyClient<DateTime> Started;
        public PropertyClient<int> Interval;

        public Consumer(IMqttClient client)
        {
            mqttClient = client;
            Started = new PropertyClient<DateTime>(mqttClient, "started");
            Interval = new PropertyClient<int>(mqttClient, "interval");
        }
    }

    [Fact]
    public async Task ReadProperties()
    {
        IMqttClient producerClientOne = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("device4"));
        IMqttClient producerClientTwo = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("device5"));
        var p1 = new Producer(producerClientOne);
        var p2 = new Producer(producerClientTwo);

        int intervalNewValue = -1;
        p1.Interval.OnMessage = async m =>
        {
            await Task.Yield();
            intervalNewValue = m;
            return new Ack<int>
            {
                Status = 200,
                Value = m
            };
        };

        IMqttClient consumerClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(TestCS("consumer"));
        Consumer consumer = new(consumerClient);
        await consumer.Interval.StartAsync("+");
        await consumer.Started.StartAsync("+");
        consumer.Interval.OnPropertyUpdated = (id, p) =>
        {
            Console.WriteLine($"Property updated: {id} interval={p}");
        };
        DateTime startedRead = DateTime.MinValue;
        consumer.Started.OnPropertyUpdated = (id, p) =>
        {
            startedRead = p;
            Console.WriteLine($"Property updated: {id} started={p}");
        };

        DateTime now = DateTime.Now;
        await p1.Started.SendMessageAsync(now);
        await consumer.Interval.UpdatePropertyAsync("device4", 23);
        await Task.Delay(500);
        Assert.Equal(23, intervalNewValue);
        Assert.Equal(now, startedRead);
    }
}
