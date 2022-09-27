using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;

public class TelemetryUTF8Json<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryUTF8Json(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new UTF8JsonSerializer())
    {
        topicPattern = "device/{clientId}/telemetry";
        wrapMessage = true;
    }
}
