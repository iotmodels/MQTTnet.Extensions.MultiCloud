using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class WritableProperty<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
{
    readonly IMqttClient _connection;
    readonly string _name;
    public T? Value { get; set; }
    public int? Version { get; set; }

    public WritableProperty(IMqttClient c, string name)
        : base(c, name)
    {
        _name = name;
        _connection = c;

        TopicTemplate = "device/{clientId}/props/{name}/set";
        ResponseTopic = "device/{clientId}/props/{name}/ack";
        RetainResponse = true;
    }

    public async Task SendMessageAsync(Ack<T> payload, CancellationToken cancellationToken = default)
    {
        var prop = new ReadOnlyProperty<Ack<T>>(_connection, _name);
        await prop.SendMessageAsync(payload, cancellationToken);
    }
}
