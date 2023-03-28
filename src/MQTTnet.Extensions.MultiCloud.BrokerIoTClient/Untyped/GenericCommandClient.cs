using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Server;
using System.Xml.Linq;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;

public class GenericCommandClient 
{
    private readonly IMqttClient mqttClient;
    TaskCompletionSource<GenericCommandResponse>? tcs;
    Guid corr = Guid.NewGuid();
    string remoteClientId = string.Empty;
    
    const string requestTopicPattern = "device/{clientId}/commands/{commandName}";
    const string responseTopicSuccess = "device/{clientId}/commands/{commandName}/resp";
    const string responseTopicSub = "device/{clientId}/commands/{commandName}/+";

    string commandName;
    IMessageSerializer _serializer;

    public GenericCommandClient(IMqttClient client)
    {
        mqttClient = client;
        _serializer = new UTF8JsonSerializer();
        commandName = string.Empty;

        mqttClient.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            var expectedTopic = responseTopicSuccess.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
            if (topic.StartsWith(expectedTopic))
            {
                if (m.ApplicationMessage.CorrelationData != null && corr != new Guid(m.ApplicationMessage.CorrelationData))
                {
                    tcs!.SetException(new ApplicationException("Invalid correlation data"));
                }
                
                if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, string.Empty, out GenericCommandResponse methodResponse))
                {
                    tcs!.SetResult(methodResponse);
                }
                else
                {
                    tcs!.SetException(new ApplicationException("Cannot deserialize bytes"));
                }
                
            }
            await Task.Yield();
        };
    }

    public async Task<GenericCommandResponse> InvokeAsync(string clientId, GenericCommandRequest request, CancellationToken ct = default)
    {
        tcs = new TaskCompletionSource<GenericCommandResponse>();
        remoteClientId = clientId;
        commandName = request.CommandName!;
        string commandTopic = requestTopicPattern.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
        var responseTopic = responseTopicSub.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName);
        await mqttClient.SubscribeAsync(responseTopic, Protocol.MqttQualityOfServiceLevel.AtMostOnce, ct);

        MqttApplicationMessage msg = new()
        {
            Topic = commandTopic,
            Payload = _serializer.ToBytes(request),
            ResponseTopic = responseTopicSuccess.Replace("{clientId}", remoteClientId).Replace("{commandName}", commandName),
            CorrelationData = corr.ToByteArray()
        };
        var pubAck = await mqttClient.PublishAsync(msg);
        if (!pubAck.IsSuccess)
        {
            throw new ApplicationException("Error publishing Request Message");
        }
        return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(5));
    }
}
