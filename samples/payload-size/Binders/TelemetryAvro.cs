using Avro;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using payload_size.Serializers;

namespace payload_size.Binders;

public class TelemetryAvro<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryAvro(IMqttClient mqttClient, Schema schema) :
        this(mqttClient, string.Empty, schema)
    { }

    public TelemetryAvro(IMqttClient mqttClient, string name, Schema schema)
        : base(mqttClient, name, new AvroSerializer<T>(schema))
    {
        TopicPattern = "devices/{clientId}/messages/events/";
        WrapMessage = false;
    }
}
