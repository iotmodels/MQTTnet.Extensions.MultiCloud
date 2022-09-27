using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;

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
