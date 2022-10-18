using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class WritableProperty<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>, IDeviceToCloud<Ack<T>>
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

        SubscribeTopicPattern = "device/{clientId}/props/{name}/set/#";
        RequestTopicPattern = "device/{clientId}/props/{name}/set";
        ResponseTopicPattern = "device/{clientId}/props/{name}/ack";
        RetainResponse = true;
        CleanRetained = true;
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

    public async Task InitPropertyAsync(string intialState, T defaultValue, CancellationToken cancellationToken = default)
    {
        Ack<T> ack = new()
        {
            Value = defaultValue,
            Version = 0,
            Status = 203,
            Description = "init from default value"
        };
        Value = ack.Value;
        Version = ack.Version;
        await SendMessageAsync(ack, cancellationToken);

    }
}
