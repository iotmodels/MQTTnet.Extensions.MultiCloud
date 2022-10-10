using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

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
        SubscribeTopicPattern = "$iothub/twin/PATCH/properties/desired/#";
        RequestTopicPattern = "$iothub/twin/PATCH/properties/desired/#";
        ResponseTopicPattern = "$iothub/twin/PATCH/properties/reported/?$rid={rid}";
        UnwrapRequest = true;
        WrapResponse = true;
        PreProcessMessage = tp =>
        {
            Version = tp.Version;
        };
    }

    public async Task SendMessageAsync(Ack<T> payload, CancellationToken cancellationToken = default)
    {
        var prop = new ReadOnlyProperty<Ack<T>>(_connection, _name);
        await prop.SendMessageAsync(payload, cancellationToken);
    }

    public async Task InitPropertyAsync(string intialState, T defaultValue, CancellationToken cancellationToken = default)
    {
        Ack<T> ack = TwinInitializer.InitFromTwin(intialState, _name, defaultValue);
        Version = ack.Version;
        Value = ack.Value;
        await SendMessageAsync(ack, cancellationToken);
    }
}
