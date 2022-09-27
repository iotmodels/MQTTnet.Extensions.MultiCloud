using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using MQTTnet.Server;
using System.Xml.Linq;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryProtobuff<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryProtobuff(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtoBuffSerializer())
    {
        topicPattern = "grpc/{clientId}/tel";
    }
}
