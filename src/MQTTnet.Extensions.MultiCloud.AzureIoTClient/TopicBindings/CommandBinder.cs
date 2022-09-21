using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class Command<T, TResponse> : ICommand<T, TResponse> 
        where T : IBaseCommandRequest<T>, new()
        where TResponse : IBaseCommandResponse, new()
    {
        public Func<T, Task<TResponse>> OnCmdDelegate { get; set; }
        public Command(IMqttClient connection, string commandName, string componentName = "")
        {
            var fullCommandName = string.IsNullOrEmpty(componentName) ? commandName : $"{componentName}*{commandName}";
            connection.SubscribeWithReplyAsync($"$iothub/methods/POST/#").Wait();
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"$iothub/methods/POST/{fullCommandName}"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    T req = new T().DeserializeBody(msg);
                    if (OnCmdDelegate != null && req != null)
                    {
                        (int rid, _) = TopicParser.ParseTopic(topic);
                        TResponse response = await OnCmdDelegate.Invoke(req);
                        string respPayload = Json.Stringify(response.ReponsePayload);
                        _ = connection.PublishJsonAsync($"$iothub/methods/res/{response.Status}/?$rid={rid}", respPayload);
                    }
                }
                await Task.Yield();
            };
        }
    }
}
