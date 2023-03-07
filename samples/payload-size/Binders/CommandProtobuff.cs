using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using payload_size.Serializers;

namespace payload_size.Binders;

public class CommandProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandProtobuff(IMqttClient client, string name, MessageParser parser)
        : base(client, name, new ProtobufSerializer<T>(parser), new ProtobufSerializer<TResp>(parser))
    {
        UnwrapRequest = false;
        RequestTopicPattern = "device/{clientId}/cmd/{name}";
        ResponseTopicPattern = "device/{clientId}/cmd/{name}/resp";
    }
}
