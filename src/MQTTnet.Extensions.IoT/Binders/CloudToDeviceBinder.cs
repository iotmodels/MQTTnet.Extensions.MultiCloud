using MQTTnet.Client;

namespace MQTTnet.Extensions.IoT.Binders;

public abstract class CloudToDeviceBinder<T, TResp> : ICloudToDevice<T, TResp>
{
    protected string topicTemplate = "device/{clientId}/commands/{name}";
    protected string topicResponseSuffix = "resp";
    public Func<T, Task<TResp>>? OnMessage { get; set; }

    public CloudToDeviceBinder(IMqttClient connection, string name, IMessageSerializer serializer)
    {
        string topic = topicTemplate.Replace("{clientId}", connection.Options.ClientId).Replace("{name}", name);
        _ = connection.SubscribeAsync(topic);
        connection.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            if (topic.Equals(topic))
            {
                if (OnMessage != null)
                {
                    T req = serializer.FromBytes<T>(m.ApplicationMessage.Payload);
                    TResp resp = await OnMessage.Invoke(req);
                    byte[] responseBytes = serializer.ToBytes(resp);
                    _ = connection.PublishBinaryAsync($"{topic}/{topicResponseSuffix}", responseBytes);
                }
            }
        };
    }
}
