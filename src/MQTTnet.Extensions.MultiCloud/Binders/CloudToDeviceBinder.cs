using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class CloudToDeviceBinder<T, TResp> : ICloudToDevice<T, TResp>
{
    private readonly string _name;
    private readonly IMqttClient _connection;

    public bool UnwrapRequest = false;
    public bool WrapResponse = false;
    public bool RetainResponse = false;
    public bool CleanRetained = false;
    

    public Func<T, Task<TResp>>? OnMessage { get; set; }

    protected Action<TopicParameters>? PreProcessMessage;

    public CloudToDeviceBinder(IMqttClient connection, string name)
        : this(connection, name, new UTF8JsonSerializer<T>(), new UTF8JsonSerializer<TResp>()) { }

    public CloudToDeviceBinder(IMqttClient connection, string name, IMessageSerializer<T> reqSerializer, IMessageSerializer<TResp> respSerializer)
    {
        _connection = connection;
        _name = name;

        connection.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            if (topic.StartsWith(requestTopicPattern!.Replace("/#", string.Empty)))
            {
                if (reqSerializer.TryReadFromBytes(m.ApplicationMessage.Payload, UnwrapRequest ? _name : string.Empty, out T req))
                {
                    var tp = TopicParser.ParseTopic(topic);
                    PreProcessMessage?.Invoke(tp);

                    TResp resp = await OnMessage?.Invoke(req)!;

                    if (resp != null)
                    {
                        byte[] responseBytes = respSerializer.ToBytes(resp, WrapResponse ? _name : string.Empty);
                        string? resTopic = responseTopicPattern?.Replace("{rid}", tp.Rid!).Replace("{version}", tp.Version.ToString());
                        _ = connection.PublishAsync(
                            new MqttApplicationMessageBuilder()
                                .WithTopic(resTopic)
                                .WithPayload(responseBytes)
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                .WithRetainFlag(RetainResponse)
                                .Build());

                        if (CleanRetained && m.ApplicationMessage.Retain) // clean retain once received
                        {
                            _ = connection.PublishBinaryAsync(topic, null, MqttQualityOfServiceLevel.AtLeastOnce, true);
                        }
                    }
                }
            }
        };
    }

    private string? requestTopicPattern;
    protected string? RequestTopicPattern
    {
        get => requestTopicPattern;
        set
        {
            requestTopicPattern = value?.Replace("{clientId}", _connection.Options.ClientId).Replace("{name}", _name)!;
            
        }
    }

    private string? responseTopicPattern;
    protected string? ResponseTopicPattern
    {
        get => responseTopicPattern;
        set
        {
            responseTopicPattern = value?.Replace("{clientId}", _connection.Options.ClientId).Replace("{name}", _name)!;
        }
    }

    private string? subscribeTopicPattern;
    protected string? SubscribeTopicPattern 
    { 
        get => subscribeTopicPattern;
        set
        {
            subscribeTopicPattern = value?.Replace("{clientId}", _connection.Options.ClientId).Replace("{name}", _name)!;
            _ = _connection.SubscribeWithReplyAsync(subscribeTopicPattern);
        }
    }
}
