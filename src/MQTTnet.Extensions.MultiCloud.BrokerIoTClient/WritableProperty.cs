using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class WritableProperty<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
{
    public T? Value { get; set; }
    public int? Version { get; set; }

    public WritableProperty(IMqttClient c, string name)
        : base(c, name)
    {
        TopicTemplate = "device/{clientId}/props/{name}/set";
        ResponseTopic = "device/{clientId}/props/{name}/ack";
        RetainResponse = true;
    }
}
