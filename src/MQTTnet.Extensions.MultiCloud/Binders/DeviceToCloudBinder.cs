using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Diagnostics;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class DeviceToCloudBinder<T>
{
    private readonly IMqttClient connection;
    private readonly string name;
    private readonly IMessageSerializer messageSerializer;
    protected string topicPattern = "device/{clientId}/telemetry";

    protected bool nameInTopic = false;
    public bool wrapMessage = false;

    protected bool retain = false;

    public DeviceToCloudBinder(IMqttClient mqttClient, string name, IMessageSerializer ser)
    {
        connection = mqttClient;
        this.name = name;
        messageSerializer = ser;
    }

    public async Task SendMessageAsync(T payload, CancellationToken cancellationToken = default)
    {

        string topic = topicPattern.Replace("{clientId}", connection.Options.ClientId);
        if (nameInTopic)
        {
            topic = topic.Replace("{name}", name);
        }
        else
        {
            topic = topic.Replace("/{name}", string.Empty);
        }

        byte[] payloadBytes;
        if (wrapMessage)
        {
            payloadBytes = messageSerializer.ToBytes(new Dictionary<string, T> { { name, payload } });
        }
        else
        {
            payloadBytes = messageSerializer.ToBytes(payload);
        }

        var pubAck = await connection.PublishBinaryAsync(
           topic,
           payloadBytes,
           MqttQualityOfServiceLevel.AtMostOnce,
           retain,
           cancellationToken);

        if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Trace.TraceWarning($"Message not published: {pubAck.ReasonCode}");
        }
    }

}
