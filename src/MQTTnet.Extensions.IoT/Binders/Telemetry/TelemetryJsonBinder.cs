using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryJsonBinder<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryJsonBinder(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new UTF8JsonSerializer())
    {
        wrapMessage = true;
    }
}
