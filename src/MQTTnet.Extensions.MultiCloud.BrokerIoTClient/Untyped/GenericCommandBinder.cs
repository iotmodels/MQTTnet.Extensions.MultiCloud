using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommand
    {
        private readonly IMqttClient connection;
        public Func<GenericCommandRequest, GenericCommandResponse>? OnCmdDelegate { get; set; }

        public GenericCommand(IMqttClient c)
        {
            connection = c;
            _ = connection.SubscribeWithReplyAsync($"device/{c.Options.ClientId}/commands/+");
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"device/{c.Options.ClientId}/commands/"))
                {
                    var segments = topic.Split('/');
                    var cmdName = segments[3];
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);

                    var responseTopic = m.ApplicationMessage.ResponseTopic ?? $"{topic}/resp";

                    GenericCommandRequest req = new()
                    {
                        CommandName = cmdName,
                        CommandPayload = msg
                    };
                    if (OnCmdDelegate != null && req != null)
                    {
                        var tp = TopicParser.ParseTopic(topic);
                        GenericCommandResponse response = OnCmdDelegate.Invoke(req);
                        await connection.PublishAsync(new MqttApplicationMessageBuilder()
                            .WithTopic(responseTopic)
                            .WithPayload(Encoding.UTF8.GetBytes(response.ReponsePayload!))
                            .WithCorrelationData(m.ApplicationMessage.CorrelationData ?? Guid.Empty.ToByteArray())
                            .Build());
                    }
                }
                
            };
        }
        // public async Task InitSubscriptionsAsync() => await connection.SubscribeWithReplyAsync("$iothub/methods/POST/#");
    }
}
