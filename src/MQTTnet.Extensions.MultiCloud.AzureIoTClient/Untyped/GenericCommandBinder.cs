using MQTTnet.Client;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericCommand
    {
        private readonly IMqttClient connection;
        public Func<GenericCommandRequest, GenericCommandResponse>? OnCmdDelegate { get; set; }

        public GenericCommand(IMqttClient c)
        {
            connection = c;
            _ = connection.SubscribeWithReplyAsync("$iothub/methods/POST/#");
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"$iothub/methods/POST/"))
                {
                    var segments = topic.Split('/');
                    var cmdName = segments[3];
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    GenericCommandRequest req = new GenericCommandRequest()
                    {
                        CommandName = cmdName,
                        CommandPayload = msg
                    };
                    if (OnCmdDelegate != null && req != null)
                    {
                        (int rid, _) = TopicParser.ParseTopic(topic);
                        GenericCommandResponse response = OnCmdDelegate.Invoke(req);
                        _ = connection.PublishStringAsync($"$iothub/methods/res/{response.Status}/?$rid={rid}", response.ReponsePayload);
                    }
                }
                await Task.Yield();
            };
        }
        // public async Task InitSubscriptionsAsync() => await connection.SubscribeWithReplyAsync("$iothub/methods/POST/#");
    }
}
