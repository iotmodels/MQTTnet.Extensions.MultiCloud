using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.ReadOnlyProperty;

public class ReadOnlyPropertyProtobuff<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyPropertyProtobuff(IMqttClient mqttClient, string name) : base(mqttClient, name, new ProtoBuffSerializer())
    {
        topicPattern = "grpc/{clientId}/props";
        wrapMessage = false;
        nameInTopic = false;
        retain = true;
    }
}
