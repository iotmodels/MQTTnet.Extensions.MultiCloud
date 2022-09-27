using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.ReadOnlyProperty;

public class ReadOnlyPropertyUTFJson<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyPropertyUTFJson(IMqttClient mqttClient, string name) : base(mqttClient, name, new UTF8JsonSerializer())
    {
        topicPattern = "device/{clientId}/props/{name}";
        wrapMessage = true;
        nameInTopic = false;
        retain = true;
    }
}
