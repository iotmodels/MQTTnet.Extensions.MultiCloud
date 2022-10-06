using Avro;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class TelemetryAvro<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryAvro(IMqttClient mqttClient, Schema schema) :
        this(mqttClient, string.Empty, schema)
    { }

    public TelemetryAvro(IMqttClient mqttClient, string name, Schema schema)
        : base(mqttClient, name, new AvroSerializer(schema))
    {
        TopicPattern = "device/{clientId}/tel";
    }
}
