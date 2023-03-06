using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public class RequestResponseBinder<T, TResp>
{
    IMqttClient mqttClient;
    string commandName;
    TaskCompletionSource<TResp>? echoTcs;

    protected string requestTopicPattern = "device/{clientId}/commands/{commandName}";
    protected string responseTopicSub = "device/{clientId}/commands/{commandName}/+";
    protected string responseTopicSuccess = "device/{clientId}/commands/{commandName}/resp";
    protected string responseTopicFailure = "device/{clientId}/commands/{commandName}/err";
    protected bool requireNotEmptyPayload = true;


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
            var expectedTopic = responseTopicSuccess.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
            if (topic.StartsWith(expectedTopic))
            {
                if (requireNotEmptyPayload)
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
                else
                {
                    echoTcs!.SetResult(default!);
                }
            }
            await Task.Yield();
        };
    }
    public async Task<TResp> InvokeAsync(string clientId, T request)
    {
        echoTcs = new TaskCompletionSource<TResp>();
        remoteClientId = clientId;
        string commandTopic = requestTopicPattern.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
        var responseTopic = responseTopicSub.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
        await mqttClient.SubscribeAsync(responseTopic);
        MqttApplicationMessage msg = new()
        {
            Topic = commandTopic,
            Payload = _serializer.ToBytes(request),
            ResponseTopic = responseTopic,
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
