using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using payload_size.Serializers;

namespace payload_size.Binders;

public class ReadOnlyPropertyProtobuff<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public T? Value { get; set; }
    public int Version { get; set; }
    public ReadOnlyPropertyProtobuff(IMqttClient mqttClient) : this(mqttClient, string.Empty) { }

    public ReadOnlyPropertyProtobuff(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtobufSerializer<T>())
    {
        TopicPattern = "device/{clientId}/props";
        WrapMessage = false;
        Retain = true;
    }

    public void InitProperty(string initialState)
    {
        throw new System.NotImplementedException();
    }

    public Task SendMessageAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
