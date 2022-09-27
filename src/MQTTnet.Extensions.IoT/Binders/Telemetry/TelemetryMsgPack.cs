using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryMsgPack<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryMsgPack(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new MsgPackSerializer())
    {
        topicPattern = "device/{clientId}/telemetry";
        nameInTopic = true;
    }
}
