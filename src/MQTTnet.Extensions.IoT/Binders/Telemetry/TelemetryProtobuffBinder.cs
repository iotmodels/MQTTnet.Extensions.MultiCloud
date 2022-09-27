using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryProtobuffBinder<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryProtobuffBinder(IMqttClient mqttClient, string name, MessageParser parser)
        : base(mqttClient, name, new ProtoBuffSerializer(parser))
    {
    }
}
