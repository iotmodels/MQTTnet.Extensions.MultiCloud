using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class Telemetry<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public Telemetry(IMqttClient mqttClient, string name)
    : base(mqttClient, name)
    {
        TopicPattern = "devices/{clientId}/messages/events/";
        WrapMessage = true;
    }
}
