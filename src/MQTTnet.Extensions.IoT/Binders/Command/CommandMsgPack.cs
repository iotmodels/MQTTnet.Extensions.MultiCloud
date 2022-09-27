using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Command;

public class CommandMsgPack<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandMsgPack(IMqttClient client, string name ) : base(client, name, new MsgPackSerializer())
    {
        TopicTemplate = "device/{clientId}/commands/{name}";
    }
}
