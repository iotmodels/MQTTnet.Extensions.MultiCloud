using Avro;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyPropertyAvro<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyPropertyAvro(IMqttClient mqttClient, Schema schema)
        : this(mqttClient, string.Empty, schema) { }

    public ReadOnlyPropertyAvro(IMqttClient mqttClient, string name, Schema schema)
        : base(mqttClient, name, new AvroSerializer(schema))
    {
        TopicPattern = "device/{clientId}/props";
        WrapMessage = false;
        NameInTopic = false;
        Retain = true;
    }
}
