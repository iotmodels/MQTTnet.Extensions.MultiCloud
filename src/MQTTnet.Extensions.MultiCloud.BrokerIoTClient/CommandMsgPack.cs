using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class CommandMsgPack<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandMsgPack(IMqttClient client, string name) : base(client, name, new MsgPackSerializer())
    {
        TopicTemplate = "device/{clientId}/commands/{name}";
    }
}
