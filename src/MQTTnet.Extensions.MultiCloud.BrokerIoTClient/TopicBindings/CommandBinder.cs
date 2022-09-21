using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class Command<T, TResponse> : ICommand<T, TResponse>
        where T : IBaseCommandRequest<T>, new()
        where TResponse : IBaseCommandResponse
    {
        public Func<T, TResponse>? OnCmdDelegate { get; set; }

        public Command(IMqttClient connection, string commandName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync($"pnp/{connection.Options.ClientId}/commands/{commandName}").Result;
            subAck.TraceErrors();
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                var fullCommandName = string.IsNullOrEmpty(componentName) ? commandName : $"{componentName}*{commandName}";

                if (topic.Equals($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}"))
                {
                    //T req = new T().DeserializeBody(Encoding.UTF8.GetString(m.ApplicationMessage.Payload));
                    // TODO use payloadFormatIdicator
                    T req = new T().DeserializeBody(m.ApplicationMessage.Payload);
                    if (OnCmdDelegate != null && req != null)
                    {
                        TResponse response = OnCmdDelegate.Invoke(req);
                        _ = connection.PublishBytesAsync($"pnp/{connection.Options.ClientId}/commands/{fullCommandName}/resp/{response.Status}", 
                            response.ResponseBytes);
                    }
                }
                await Task.Yield();
            };
        }
    }
}
