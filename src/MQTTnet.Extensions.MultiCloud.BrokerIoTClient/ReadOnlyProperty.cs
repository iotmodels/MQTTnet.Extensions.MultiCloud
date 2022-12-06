using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    private readonly string _name;
    public T? Value { get; set; }
    public int Version { get; set; }

    public ReadOnlyProperty(IMqttClient mqttClient) : this(mqttClient, string.Empty) { }
    public ReadOnlyProperty(IMqttClient mqttClient, string name)
        : base(mqttClient, name)
    {
        _name = name;
        TopicPattern = "device/{clientId}/props/{name}";
        WrapMessage = false;
        Retain = true;
    }

    public void InitProperty(string initialState)
    {
        throw new NotImplementedException();
    }

    public  Task SendMessageAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
