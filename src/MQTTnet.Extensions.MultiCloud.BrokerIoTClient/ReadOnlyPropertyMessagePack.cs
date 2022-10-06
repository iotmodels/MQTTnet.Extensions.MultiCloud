using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyPropertyMessagePack<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyPropertyMessagePack(IMqttClient mqttClient, string name) : base(mqttClient, name, new MsgPackSerializer())
    {
        TopicPattern = "device/{clientId}/props/{name}";
        WrapMessage = false;
        Retain = true;
    }
}
