using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;

public class TelemetryMsgPack<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryMsgPack(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new MsgPackSerializer())
    {
        topicPattern = "device/{clientId}/telemetry";
        nameInTopic = true;
    }
}
