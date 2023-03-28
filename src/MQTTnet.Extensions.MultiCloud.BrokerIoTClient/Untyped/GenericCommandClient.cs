using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Server;
using System.Xml.Linq;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped;

public class GenericCommandClient : RequestResponseBinder<GenericCommandRequest, GenericCommandResponse>
{
    public GenericCommandClient(IMqttClient client) : base(client, string.Empty, false)
    {
        
    }
}
