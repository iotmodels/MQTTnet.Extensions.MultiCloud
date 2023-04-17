using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericCommand : IGenericCommand
    {
        private readonly IMqttClient connection;
        public Func<IGenericCommandRequest, Task<IGenericCommandResponse>>? OnCmdDelegate { get; set; }

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
                    string msg = m.ApplicationMessage.ConvertPayloadToString();
                    GenericCommandRequest req = new()
                    {
                        CommandName = cmdName,
                        CommandPayload = msg
                    };
                    if (OnCmdDelegate != null && req != null)
                    {
                        var tp = TopicParser.ParseTopic(topic);
                        IGenericCommandResponse response = await OnCmdDelegate.Invoke(req);
                        _ = connection.PublishStringAsync($"$iothub/methods/res/{response.Status}/?$rid={tp.Rid}", response.ReponsePayload);
                    }
                }
                await Task.Yield();
            };
        }
        // public async Task InitSubscriptionsAsync() => await connection.SubscribeWithReplyAsync("$iothub/methods/POST/#");
    }
}
