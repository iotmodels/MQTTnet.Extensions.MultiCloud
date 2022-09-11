using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class Command<T, TResponse> : ICommand<T, TResponse>
        where T : IBaseCommandRequest<T>, new()
        where TResponse : BaseCommandResponse
    {
        public Func<T, Task<TResponse>>? OnCmdDelegate { get; set; }

        public Command(IMqttClient connection, string commandName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync($"pnp/{connection.Options.ClientId}/commands/#").Result;
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                var fullCommandName = string.IsNullOrEmpty(componentName) ? commandName : $"{componentName}*{commandName}";

                if (topic.Equals($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}"))
                {
                    T req = new T().DeserializeBody(Encoding.UTF8.GetString(m.ApplicationMessage.Payload));
                    if (OnCmdDelegate != null && req != null)
                    {
                        TResponse response = await OnCmdDelegate.Invoke(req);
                        _ = connection.PublishStringAsync($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}/resp/{response.Status}", Json.Stringify(response));
                    }
                }
            };
        }
    }
}
