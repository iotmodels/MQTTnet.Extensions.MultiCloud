using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;

public class ReadOnlyPropertyMessagePack<T> : DeviceToCloudBinder<T>, IROProperty<T>
{
    public ReadOnlyPropertyMessagePack(IMqttClient mqttClient, string name) : base(mqttClient, name, new MsgPackSerializer())
    {
        topicPattern = "device/{clientId}/props/{name}";
        wrapMessage = false;
        nameInTopic = true;
        retain = true;
    }
}
