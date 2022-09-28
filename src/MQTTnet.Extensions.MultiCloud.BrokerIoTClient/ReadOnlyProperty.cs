using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyProperty(IMqttClient mqttClient, string name) 
        : base(mqttClient, name)
    {
        TopicPattern = "device/{clientId}/props/{name}";
        WrapMessage = true;
        NameInTopic = false;
        Retain = true;
    }
}
