using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Diagnostics;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.Binders;

public abstract class CloudToDeviceBinder<T, TResp> : ICloudToDevice<T, TResp>
{
    private readonly string _name;
    private readonly IMqttClient _connection;

    protected bool NameInTopic = true;

    protected bool UnwrapRequest = false;
    protected bool WrapResponse = false;

    public Func<T, Task<TResp>>? OnMessage { get; set; }

    //protected Action<string>? PreProcessMessage;
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
            if (topic.StartsWith(topicTemplate!.Replace("/#", string.Empty)))
            {
                if (OnMessage != null)
                {
                    var tp = TopicParser.ParseTopic(topic);
                    if (PreProcessMessage != null)
                    {
                        PreProcessMessage(tp);
                    }

                    T req = serializer.FromBytes<T>(m.ApplicationMessage.Payload, UnwrapRequest ? _name : string.Empty)!;
                    if (req != null)
                    {
                        TResp resp = await OnMessage.Invoke(req);
                        byte[] responseBytes = serializer.ToBytes(resp, WrapResponse ? _name : string.Empty);

                        string? resTopic = responseTopic?
                            .Replace("{rid}", tp.Rid.ToString())
                            .Replace("{version}", tp.Version.ToString());

                        _ = connection.PublishBinaryAsync(resTopic, responseBytes);
                    }
                    else
                    {
                        Trace.TraceWarning($"Cannot parse incoming message name: {_name} payload: {Encoding.UTF8.GetString(m.ApplicationMessage.Payload)}");
                    }
                }
            }
        };
    }

    private string? topicTemplate;
    protected string? TopicTemplate
    {
        get
        {
            return topicTemplate;
        }
        set
        {

            string topic = value?.Replace("{clientId}", _connection.Options.ClientId)!;

            if (NameInTopic)
            {
                topic = topic!.Replace("{name}", _name);
            }
            else
            {
                topic = topic!.Replace("/{name}", string.Empty);
            }


            topicTemplate = topic;
            _ = _connection.SubscribeWithReplyAsync(topic);
        }
    }

    private string? responseTopic;
    protected string? ResponseTopic
    {
        get
        {
            return responseTopic;
        }
        set
        {
            string topic = value?.Replace("{clientId}", _connection.Options.ClientId)!;

            if (NameInTopic)
            {
                topic = topic!.Replace("{name}", _name);
            }
            else
            {
                topic = topic!.Replace("/{name}", string.Empty);
            }
            responseTopic = topic;
        }
    }
}
