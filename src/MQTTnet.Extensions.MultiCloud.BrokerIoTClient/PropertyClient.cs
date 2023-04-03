using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class PropertyClient<T>
{
    readonly IMqttClient _mqttClient;
    readonly IMessageSerializer _messageSerializer;
    readonly string _name;

    public string TopicPatternValue { get; set; } = "device/{clientId}/props/{name}";
    public string TopicPatternSet { get; set; } = "device/{clientId}/props/{name}/set";
    public string TopicPatternAck { get; set; } = "device/{clientId}/props/{name}/ack";

    
    public Action<string,T>? OnPropertyUpdated { get; set; } = null;

    public PropertyClient(IMqttClient client, string name) : this(client, name, new Utf8JsonSerializer()) { }

    public PropertyClient(IMqttClient client, string name, IMessageSerializer messageSerializer)
    {
        _mqttClient = client;
        _name = name;
        _messageSerializer = messageSerializer;
    }

    public async Task<MqttClientSubscribeResult> StartAsync(string deviceFilter, CancellationToken cancellationToken = default)
    {
        var topicFilter = TopicPatternValue.Replace("{clientId}", deviceFilter).Replace("{name}", _name);
        var subAck = await _mqttClient.SubscribeAsync(topicFilter + "/#", Protocol.MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken);
        _mqttClient.ApplicationMessageReceivedAsync += async m =>
        {
            string topic = m.ApplicationMessage.Topic;
            string deviceId = topic.Split('/')[1];
            string expectedTopic = TopicPatternValue.Replace("{clientId}", deviceId).Replace("{name}", _name);
            if (topic.StartsWith(expectedTopic))
            {
                if (_messageSerializer.TryReadFromBytes(m.ApplicationMessage.Payload, _name, out T propValue))
                {
                    OnPropertyUpdated?.Invoke(deviceId,propValue);
                }
            }
            await Task.Yield();
        };
        return subAck;
    }

    public async Task<MqttClientPublishResult> UpdatePropertyAsync(string deviceId, T newValue, bool retain = false, CancellationToken cancellationToken = default)
    {
        string setTopic = TopicPatternSet.Replace("{clientId}", deviceId).Replace("{name}", _name);
        byte[] bytes = _messageSerializer.ToBytes(newValue);
        return await _mqttClient.PublishBinaryAsync(setTopic, bytes, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, retain, cancellationToken);
    }
}
