using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Server;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;

public class GenericCommandClient 
{
    readonly IMqttClient _mqttClient;
    readonly IMessageSerializer _serializer;
    TaskCompletionSource<GenericCommandResponse>? _tcs;
    string? _commandName;
    string _remoteClientId;
    byte[]? corr = new byte[] { };


    string requestTopicPattern = "device/{clientId}/commands/{commandName}";
    string responseTopicSub = "device/{clientId}/commands/{commandName}/+";
    string responseTopicSuccess = "device/{clientId}/commands/{commandName}/resp";


    public GenericCommandClient(IMqttClient client) 
    {
        _mqttClient = client;
        _remoteClientId = string.Empty;
        _serializer = new Utf8JsonSerializer();

        _mqttClient.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;

            var expectedTopic = responseTopicSuccess.Replace("{clientId}", _remoteClientId).Replace("{commandName}", _commandName);
            if (topic.StartsWith(expectedTopic))
            {
                if (m.ApplicationMessage.CorrelationData != null && !corr.SequenceEqual(m.ApplicationMessage.CorrelationData))
                {
                    _tcs!.SetException(new ApplicationException("Invalid correlation data"));
                }

                //var up = m.ApplicationMessage.UserProperties.FirstOrDefault(p => p.Name.Equals("status"));
                //int status = up != null ? int.Parse(up.Value) : 500;

                if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, string.Empty, out string respPayload))
                {
                    GenericCommandResponse resp = new()
                    {
                        Status = 200,
                        ReponsePayload = respPayload
                    };
                    _tcs!.SetResult(resp);
                }
                else
                {
                    _tcs!.SetException(new ApplicationException("Cannot deserialize bytes"));
                }

            }
            await Task.Yield();
        };
    }

    public async Task<GenericCommandResponse> InvokeAsync(string clientId, GenericCommandRequest request, CancellationToken ct = default)
    {
        _tcs = new TaskCompletionSource<GenericCommandResponse>();
        _remoteClientId = clientId;
        _commandName = request.CommandName;
        corr = request.CorrelationId!;
        string commandTopic = requestTopicPattern.Replace("{clientId}", _remoteClientId).Replace("{commandName}", _commandName);
        var responseTopic = responseTopicSub.Replace("{clientId}", _remoteClientId).Replace("{commandName}", _commandName);
        _ =_mqttClient.SubscribeAsync(responseTopic, Protocol.MqttQualityOfServiceLevel.AtMostOnce, ct);

        _ = _mqttClient.PublishAsync(
            new MqttApplicationMessageBuilder()
                .WithTopic(commandTopic)
                .WithPayload(_serializer.ToBytes(request.CommandPayload))
                .WithResponseTopic(responseTopicSuccess.Replace("{clientId}", _remoteClientId).Replace("{commandName}", _commandName))
                .WithCorrelationData(request.CorrelationId)
                .Build());
        
        return await _tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(5));

    }
}
