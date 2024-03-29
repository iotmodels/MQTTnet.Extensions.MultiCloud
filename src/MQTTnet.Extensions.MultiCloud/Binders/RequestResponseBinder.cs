﻿using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public class RequestResponseBinder<T, TResp>
{
    readonly IMqttClient mqttClient;
    readonly string commandName;
    TaskCompletionSource<TResp>? tcs;

    protected string requestTopicPattern = "device/{clientId}/commands/{commandName}";
    protected string responseTopicSub = "device/{clientId}/commands/{commandName}/+";
    protected string responseTopicSuccess = "device/{clientId}/commands/{commandName}/resp";
    protected string responseTopicFailure = "device/{clientId}/commands/{commandName}/err";
    protected bool requireNotEmptyPayload = true;
    string remoteClientId = string.Empty;
    readonly bool _unwrap = true;
    Guid corr = Guid.NewGuid();

    protected Func<string, TResp>? VersionExtractor { get; set; }

    readonly IMessageSerializer _serializer;

    public RequestResponseBinder(IMqttClient client, string name, bool unwrap)
        : this(client, name, unwrap, new Utf8JsonSerializer())
    {

    }

    public RequestResponseBinder(IMqttClient client, string name, bool unwrap, IMessageSerializer serializer)
    {
        mqttClient = client;
        commandName = name;
        _serializer = serializer;
        _unwrap = unwrap;
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

                if (requireNotEmptyPayload)
                {
                    if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, _unwrap ? name : string.Empty, out TResp resp))
                    {
                        tcs!.SetResult(resp);
                    }
                    else
                    {
                        tcs!.SetException(new ApplicationException("Cannot deserialize bytes"));
                    }
                }
                else
                {
                    // update twin returns version from topic response
                    TResp resp = VersionExtractor!.Invoke(topic);
                    tcs!.SetResult(resp);
                }
            }
            await Task.Yield();
        };
    }
    public async Task<TResp> InvokeAsync(string clientId, T request, CancellationToken ct = default)
    {
        tcs = new TaskCompletionSource<TResp>();
        remoteClientId = clientId;
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
