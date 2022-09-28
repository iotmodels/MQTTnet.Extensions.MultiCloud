using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyProperty(IMqttClient mqttClient, string name) : base(mqttClient, name, new UTF8JsonSerializer())
    {
        topicPattern = "$iothub/twin/PATCH/properties/reported/?$rid=1";
        wrapMessage = true;
        nameInTopic = false;
        retain = false;
    }
}
