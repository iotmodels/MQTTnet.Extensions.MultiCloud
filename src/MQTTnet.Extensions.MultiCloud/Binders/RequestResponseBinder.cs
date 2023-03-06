using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public class RequestResponseBinder<T, TResp>
{
    IMqttClient mqttClient;
    string commandName;
    TaskCompletionSource<TResp>? echoTcs;

    protected string commandTopicPattern = "device/{clientId}/commands/{commandName}";
    protected string commandResponseSuffix = "/resp";
    string remoteClientId = string.Empty;
    IMessageSerializer _serializer;

    public RequestResponseBinder(IMqttClient client, string name)
        : this(client, name, new UTF8JsonSerializer())
    {

    }

    public RequestResponseBinder(IMqttClient client, string name, IMessageSerializer serializer)
    {
        mqttClient = client;
        commandName = name;
        _serializer = serializer;
        mqttClient.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            var expectedTopic = commandTopicPattern.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName) + commandResponseSuffix;
            if (topic.Equals(expectedTopic))
            {
                if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, name, out TResp resp))
                {
                    echoTcs!.SetResult(resp);
                }
                else
                {
                    echoTcs!.SetException(new ApplicationException("Cannot deserialize bytes"));
                }
            }
            await Task.Yield();
        };
    }
    public async Task<TResp> InvokeAsync(string clientId, T request)
    {
        echoTcs = new TaskCompletionSource<TResp>();
        remoteClientId = clientId;
        string commandTopic = commandTopicPattern.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
        await mqttClient.SubscribeAsync(commandTopic + commandResponseSuffix);
        MqttApplicationMessage msg = new()
        {
            Topic = commandTopic,
            Payload = _serializer.ToBytes(request),
            ResponseTopic = commandTopic + commandResponseSuffix,
            CorrelationData = new byte[] { 1 }
        };
        var pubAck = await mqttClient.PublishAsync(msg);
        if (!pubAck.IsSuccess)
        {
            throw new ApplicationException("Error publishing Request Message");
        }
        return await echoTcs.Task.TimeoutAfter(TimeSpan.FromSeconds(5));
    }
}
