using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class TelemetryProtobuff<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryProtobuff(IMqttClient mqttClient) :
        this(mqttClient, string.Empty)
    { }

    public TelemetryProtobuff(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtobufSerializer())
    {
        TopicPattern = "device/{clientId}/tel";
    }
}
