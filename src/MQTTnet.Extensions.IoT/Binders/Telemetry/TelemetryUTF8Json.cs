using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry;

public class TelemetryUTF8Json<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryUTF8Json(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new UTF8JsonSerializer())
    {
        topicPattern = "device/{clientId}/telemetry";
        wrapMessage = true;
    }
}
