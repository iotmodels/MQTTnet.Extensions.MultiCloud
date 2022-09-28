using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System.Diagnostics;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class DeviceToCloudBinder<T>
{
    private readonly IMqttClient connection;
    private readonly string name;
    private readonly IMessageSerializer MessageSerializer;

    protected string TopicPattern = "device/{clientId}/telemetry";
    protected bool NameInTopic = false;
    public bool WrapMessage = false;
    protected bool Retain = false;

    public DeviceToCloudBinder(IMqttClient mqttClient, string name) : this(mqttClient, name, new UTF8JsonSerializer()) { }

    public DeviceToCloudBinder(IMqttClient mqttClient, string name, IMessageSerializer ser)
    {
        connection = mqttClient;
        this.name = name;
        MessageSerializer = ser;
    }

    public async Task SendMessageAsync(T payload, CancellationToken cancellationToken = default)
    {

        string topic = TopicPattern.Replace("{clientId}", connection.Options.ClientId);
        if (NameInTopic)
        {
            topic = topic.Replace("{name}", name);
        }
        else
        {
            topic = topic.Replace("/{name}", string.Empty);
        }

        byte[] payloadBytes;
        if (WrapMessage)
        {
            payloadBytes = MessageSerializer.ToBytes(new Dictionary<string, T> { { name, payload } });
        }
        else
        {
            payloadBytes = MessageSerializer.ToBytes(payload);
        }

        var pubAck = await connection.PublishBinaryAsync(
           topic,
           payloadBytes,
           MqttQualityOfServiceLevel.AtMostOnce,
           Retain,
           cancellationToken);

        if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Trace.TraceWarning($"Message not published: {pubAck.ReasonCode}");
        }
    }

}
