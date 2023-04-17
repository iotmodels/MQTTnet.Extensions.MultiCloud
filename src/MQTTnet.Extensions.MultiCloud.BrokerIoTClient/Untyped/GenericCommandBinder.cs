using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;

public class GenericCommand : IGenericCommand
{
    private readonly IMqttClient connection;
    private readonly IMessageSerializer _serializer;

    public Func<IGenericCommandRequest, Task<IGenericCommandResponse>>? OnCmdDelegate { get; set; }

    public GenericCommand(IMqttClient c)
    {
        _serializer = new Utf8JsonSerializer();
        connection = c;
        _ = connection.SubscribeWithReplyAsync($"device/{c.Options.ClientId}/commands/+");
        connection.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            if (topic.StartsWith($"device/{c.Options.ClientId}/commands/"))
            {
                var segments = topic.Split('/');
                var cmdName = segments[3];

                if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, string.Empty, out string reqPayload))
                {
                    var responseTopic = m.ApplicationMessage.ResponseTopic ?? $"{topic}/resp";

                    if (OnCmdDelegate != null)
                    {
                        GenericCommandRequest req = new()
                        {
                            CommandName = cmdName,
                            CommandPayload = reqPayload,
                            //CorrelationId = m.ApplicationMessage.CorrelationData
                        };

                        IGenericCommandResponse response = await OnCmdDelegate.Invoke(req);
                        await connection.PublishAsync(new MqttApplicationMessageBuilder()
                            .WithTopic(responseTopic)
                            .WithPayload(_serializer.ToBytes(response.ReponsePayload))
                            .WithUserProperty("status", response.Status.ToString())
                            .WithCorrelationData(m.ApplicationMessage.CorrelationData)
                            .Build());
                    }
                }
            }
        };
    }
}
