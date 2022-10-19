using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System.Diagnostics;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class DeviceToCloudBinder<T> : IDeviceToCloud<T>
{
    private readonly string _name;
    private readonly IMqttClient _connection;
    private readonly IMessageSerializer _messageSerializer;

    public string TopicPattern = "device/{clientId}/telemetry";
    public bool WrapMessage = false;
    public bool Retain = false;

    public DeviceToCloudBinder(IMqttClient mqttClient, string name) : this(mqttClient, name, new UTF8JsonSerializer()) { }

    public DeviceToCloudBinder(IMqttClient mqttClient, string name, IMessageSerializer ser)
    {
        _connection = mqttClient;
        _name = name;
        _messageSerializer = ser;
    }

    public async Task SendMessageAsync(T payload, CancellationToken cancellationToken = default)
    {
        string topic = TopicPattern
                            .Replace("{clientId}", _connection.Options.ClientId)
                            .Replace("{name}", _name);
        byte[] payloadBytes;
        if (WrapMessage)
        {
            payloadBytes = _messageSerializer.ToBytes(new Dictionary<string, T> { { _name, payload } });
        }
        else
        {
            payloadBytes = _messageSerializer.ToBytes(payload);
        }
        var pubAck = await _connection.PublishAsync(
            new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payloadBytes)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(Retain)
                .Build(),
            cancellationToken);

        if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Trace.TraceWarning($"Message not published: {pubAck.ReasonCode}");
        }
    }
}
