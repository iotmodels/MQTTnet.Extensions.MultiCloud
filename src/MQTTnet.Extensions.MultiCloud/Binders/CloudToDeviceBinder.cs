using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System.Diagnostics;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class CloudToDeviceBinder<T, TResp> : ICloudToDevice<T, TResp>
{
    private readonly string _name;
    private readonly IMqttClient _connection;

    protected bool UnwrapRequest = false;
    protected bool WrapResponse = false;

    protected bool RetainResponse = false;

    public Func<T, Task<TResp>>? OnMessage { get; set; }

    protected Action<TopicParameters>? PreProcessMessage;

    public CloudToDeviceBinder(IMqttClient connection, string name)
        : this(connection, name, new UTF8JsonSerializer()) { }

    public CloudToDeviceBinder(IMqttClient connection, string name, IMessageSerializer serializer)
    {
        _connection = connection;
        _name = name;

        connection.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            if (topic.StartsWith(requestTopicPattern!.Replace("/#", string.Empty)))
            {
                if (OnMessage != null)
                {
                    var tp = TopicParser.ParseTopic(topic);
                    PreProcessMessage?.Invoke(tp);

                    T req = serializer.FromBytes<T>(m.ApplicationMessage.Payload, UnwrapRequest ? _name : string.Empty)!;
                    if (req != null)
                    {
                        TResp resp = await OnMessage.Invoke(req);
                        byte[] responseBytes = serializer.ToBytes(resp, WrapResponse ? _name : string.Empty);

                        string? resTopic = responseTopicPattern?
                            .Replace("{rid}", tp.Rid.ToString())
                            .Replace("{version}", tp.Version.ToString());

                        _ = connection.PublishAsync(
                            new MqttApplicationMessageBuilder()
                                .WithTopic(resTopic)
                                .WithPayload(responseBytes)
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                .WithRetainFlag(RetainResponse)
                                .Build());
                    }
                    else
                    {
                        Trace.TraceWarning($"Cannot parse incoming message name: {_name} payload: {Encoding.UTF8.GetString(m.ApplicationMessage.Payload)}");
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
            _ = _connection.SubscribeWithReplyAsync(requestTopicPattern);
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
}
