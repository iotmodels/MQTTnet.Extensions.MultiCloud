using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class TelemetryProtobuf<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryProtobuf(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtobufSerializer())
    {
        TopicPattern = "devices/{clientId}/messages/events/";
        WrapMessage = false;
    }
}
