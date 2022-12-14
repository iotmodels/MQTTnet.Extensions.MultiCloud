using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using payload_size.Serializers;

namespace payload_size.Binders;

public class TelemetryProtobuf<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{

    public TelemetryProtobuf(IMqttClient mqttClient) :
      this(mqttClient, string.Empty)
    { }

    public TelemetryProtobuf(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtobufSerializer())
    {
        TopicPattern = "devices/{clientId}/messages/events/";
        WrapMessage = false;
    }
}
