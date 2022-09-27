using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryMsgPackBinder<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryMsgPackBinder(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new MsgPackSerializer())
    {
        nameInTopic = true;
    }
}
