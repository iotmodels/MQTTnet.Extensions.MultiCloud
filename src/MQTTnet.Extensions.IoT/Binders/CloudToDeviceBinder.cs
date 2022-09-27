﻿using MQTTnet.Client;
using System.Xml.Linq;

namespace MQTTnet.Extensions.IoT.Binders;

public abstract class CloudToDeviceBinder<T, TResp> : ICloudToDevice<T, TResp>
{
    string _name;
    IMqttClient _connection;
    
    protected bool nameInTopic = true;
    public Func<T, Task<TResp>>? OnMessage { get; set; }

    protected Action<string>? PreProcessMessage;
    protected Func<string>? PostProcessMessage;

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
                    if (PreProcessMessage != null)
                    {
                        PreProcessMessage(topic);
                    }

                    T req = serializer.FromBytes<T>(m.ApplicationMessage.Payload);
                    TResp resp = await OnMessage.Invoke(req);
                    byte[] responseBytes = serializer.ToBytes(resp);
                    
                    string? resTopic = responseTopic;
                    if (PostProcessMessage != null)
                    {
                        string rid = PostProcessMessage();
                        resTopic = responseTopic?.Replace("{rid}", rid);
                    }

                    _ = connection.PublishBinaryAsync(resTopic, responseBytes);
                }
            }
        };
    }

    string? topicTemplate;
    protected string? TopicTemplate
    {
        get
        {
            return topicTemplate;
        }
        set
        {
            
            string topic = value?.Replace("{clientId}", _connection.Options.ClientId)!;

            if (nameInTopic)
            {
                topic = topic!.Replace("{name}", _name);
            }
            else
            {
                topic = topic!.Replace("/{name}", String.Empty);
            }


            topicTemplate = topic;
            _ = _connection.SubscribeAsync(topic);
        }
    }

    string? responseTopic;
    protected string? ResponseTopic
    {
        get
        {
            return responseTopic;
        }
        set
        {
            string topic = value?.Replace("{clientId}", _connection.Options.ClientId)!;

            if (nameInTopic)
            {
                topic = topic!.Replace("{name}", _name);
            }
            else
            {
                topic = topic!.Replace("/{name}", String.Empty);
            }
            responseTopic = topic;
        }
    }
}
