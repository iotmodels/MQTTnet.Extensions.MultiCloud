using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommand
    {
        private readonly IMqttClient connection;
        private readonly IMessageSerializer _serializer;

        public Func<GenericCommandRequest, Task<GenericCommandResponse>>? OnCmdDelegate { get; set; }

        public GenericCommand(IMqttClient c)
        {
            _serializer = new UTF8JsonSerializer();
            connection = c;
            _ = connection.SubscribeWithReplyAsync($"device/{c.Options.ClientId}/commands/+");
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"device/{c.Options.ClientId}/commands/"))
                {
                    var segments = topic.Split('/');
                    var cmdName = segments[3];

                    if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, string.Empty, out object reqPayload))
                    {
                        var responseTopic = m.ApplicationMessage.ResponseTopic ?? $"{topic}/resp";

                        if (OnCmdDelegate != null && reqPayload != null)
                        {
                            //var tp = TopicParser.ParseTopic(topic);
                            GenericCommandRequest req = new()
                            {
                                CommandName = cmdName,
                                CommandPayload = reqPayload
                            };


                            GenericCommandResponse response = await OnCmdDelegate.Invoke(req);
                            await connection.PublishAsync(new MqttApplicationMessageBuilder()
                                .WithTopic(responseTopic)

                                .WithPayload(_serializer.ToBytes(response.ReponsePayload))
                                .WithUserProperty("status", response.Status.ToString())

                                .WithCorrelationData(m.ApplicationMessage.CorrelationData ?? Guid.Empty.ToByteArray())
                                .Build());
                        }
                    }
                }
                
            };
        }
        // public async Task InitSubscriptionsAsync() => await connection.SubscribeWithReplyAsync("$iothub/methods/POST/#");
    }
}
