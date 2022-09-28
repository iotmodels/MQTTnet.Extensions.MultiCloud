﻿using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Server;
using System.Xml.Linq;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;

public class TelemetryProtobuff<T> : DeviceToCloudBinder<T>, ITelemetry<T>
{
    public TelemetryProtobuff(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new ProtoBuffSerializer())
    {
        topicPattern = "grpc/{clientId}/tel";
    }
}