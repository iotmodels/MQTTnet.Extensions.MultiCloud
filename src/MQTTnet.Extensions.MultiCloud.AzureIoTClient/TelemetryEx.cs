using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class TelemetryEx<T> : ITelemetry<T>
{
    const string topicPattern = "devices/{clientId}/messages/events/";
    string _telemetryTopic;
    IManagedMqttClient _mClient;
    Utf8JsonSerializer _serializer;
    public TelemetryEx(IManagedMqttClient mqttClient, string clientId)
    {
        _serializer = new Utf8JsonSerializer();
        _mClient = mqttClient;
        _telemetryTopic = topicPattern.Replace("{clientId}", clientId);
    }

    public Task SendMessageAsync(T payload, CancellationToken cancellationToken = default) => 
       _mClient.EnqueueAsync(new ManagedMqttApplicationMessageBuilder()
             .WithApplicationMessage(new MqttApplicationMessageBuilder()
                .WithTopic(_telemetryTopic)
                .WithPayload(_serializer.ToBytes(payload))
                .Build())
            .Build());
    
}
