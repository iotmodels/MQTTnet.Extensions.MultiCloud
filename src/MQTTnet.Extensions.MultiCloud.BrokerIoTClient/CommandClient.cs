using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class CommandClient<T, TResp> : RequestResponseBinder<T, TResp>
    {
        public CommandClient(IMqttClient client, string commandName)
            : base(client, commandName) 
        {
            commandTopicPattern = "device/{clientId}/commands/{commandName}";
            commandResponseSuffix = "/resp";
        }
    }
}
