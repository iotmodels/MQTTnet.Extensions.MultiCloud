using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{

    public class Command<T, TResponse> : ICommand<T, TResponse>
        where T : IBaseCommandRequest<T>, new()
        where TResponse : BaseCommandResponse
    {
        public Func<T, TResponse>? OnCmdDelegate { get; set; }

        public Command(IMqttClient connection, string commandName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync($"pnp/{connection.Options.ClientId}/commands/#").Result;
            subAck.TraceErrors();
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                var fullCommandName = string.IsNullOrEmpty(componentName) ? commandName : $"{componentName}*{commandName}";

                if (topic.Equals($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    T req = new T().DeserializeBody(msg);
                    if (OnCmdDelegate != null && req != null)
                    {
                        //(int rid, _) = TopicParser.ParseTopic(topic);
                        TResponse response = OnCmdDelegate.Invoke(req);
                        connection.PublishJsonAsync($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}/resp/{response.Status}", response).RunSynchronously();
                    }
                }
                await Task.Yield();
            };
        }
    }
}
