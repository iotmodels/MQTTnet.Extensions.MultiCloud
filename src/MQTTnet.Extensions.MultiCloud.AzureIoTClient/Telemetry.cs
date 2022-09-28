using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class Telemetry<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public Telemetry(IMqttClient mqttClient, string name)
    : base(mqttClient, name, new UTF8JsonSerializer())
    {
        TopicPattern = "devices/{clientId}/messages/events/";
        WrapMessage = true;
    }
}
