using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class WritableProperty<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
{
    readonly IMqttClient _connection;
    readonly string _name;
    public T? Value { get; set; }
    public int? Version { get; set; } = -1;

    public WritableProperty(IMqttClient c, string name)
        : base(c, name)
    {
        _name = name;
        _connection = c;

        RequestTopicPattern = "device/{clientId}/props/{name}/set/#";
        ResponseTopicPattern = "device/{clientId}/props/{name}/ack";
        RetainResponse = true;
        PreProcessMessage = tp =>
        {
            Version = tp.Version;
        };
    }

    public async Task SendMessageAsync(Ack<T> payload, CancellationToken cancellationToken = default)
    {
        var prop = new ReadOnlyProperty<Ack<T>>(_connection, _name)
        {
            TopicPattern = "device/{clientId}/props/{name}/ack",
            WrapMessage = false
        };
        await prop.SendMessageAsync(payload, cancellationToken);
    }
}
