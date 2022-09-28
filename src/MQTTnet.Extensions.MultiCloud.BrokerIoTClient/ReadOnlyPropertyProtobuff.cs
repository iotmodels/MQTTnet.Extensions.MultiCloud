﻿using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyPropertyProtobuff<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    public ReadOnlyPropertyProtobuff(IMqttClient mqttClient, string name) : base(mqttClient, name, new ProtobufSerializer())
    {
        TopicPattern = "grpc/{clientId}/props";
        WrapMessage = false;
        NameInTopic = false;
        Retain = true;
    }
}
