﻿using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace Serializers;

public class CommandProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandProtobuff(IMqttClient client, string name, MessageParser parser)
        : base(client, name, new ProtobufSerializer(parser))
    {
        UnwrapRequest = false;
        RequestTopicPattern = "device/{clientId}/cmd/{name}";
        SubscribeTopicPattern = "device/{clientId}/cmd/{name}";
        ResponseTopicPattern = "device/{clientId}/cmd/{name}/resp";
    }
}
