using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class Telemetry<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public Telemetry(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new UTF8JsonSerializer())
    {
        topicPattern = "device/{clientId}/telemetry";
        wrapMessage = true;
    }
}
