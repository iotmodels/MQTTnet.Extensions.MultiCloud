﻿using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class WritableProperty<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
{
    public T? Value { get; set; }
    public int? Version { get; set; }

    public WritableProperty(IMqttClient c, string name)
        : base(c, name)
    {
        TopicTemplate = "$iothub/twin/PATCH/properties/desired/#";
        ResponseTopic = "$iothub/twin/PATCH/properties/reported/?$rid={rid}&version={version}";
        UnwrapRequest = true;
        WrapResponse = true;
        PreProcessMessage = tp =>
        {
            Version = tp.Version;
        };
    }
}
