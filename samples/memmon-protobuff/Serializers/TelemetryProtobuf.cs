using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace Serializers;

public class TelemetryProtobuf<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{

    public TelemetryProtobuf(IMqttClient mqttClient) :
      this(mqttClient, string.Empty)
    { }

    public TelemetryProtobuf(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtobufSerializer<T>())
    {
        TopicPattern = "device/{clientId}/tel";
        WrapMessage = false;
    }
}
