using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{

    public ReadOnlyProperty(IMqttClient mqttClient) : this(mqttClient, string.Empty) { }
    public ReadOnlyProperty(IMqttClient mqttClient, string name)
        : base(mqttClient, name)
    {
        TopicPattern = "device/{clientId}/props/{name}";
        WrapMessage = false;
        NameInTopic = true;
        Retain = true;
    }
}
