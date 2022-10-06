using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyProperty(IMqttClient mqttClient, string name)
        : base(mqttClient, name)
    {
        TopicPattern = "$iothub/twin/PATCH/properties/reported/?$rid=1";
        WrapMessage = true;
        Retain = false;
    }
}
