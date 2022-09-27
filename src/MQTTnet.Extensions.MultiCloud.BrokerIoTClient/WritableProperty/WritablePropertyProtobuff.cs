using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.WritableProperty
{
    public class WritablePropertyProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, IWritableProperty<T, TResp>
    {
        public T? Value { get; set; }

        public WritablePropertyProtobuff(IMqttClient connection, string name, MessageParser parser)
            : base(connection, name, new ProtoBuffSerializer(parser))
        {
            TopicTemplate = "grpc/{clientId}/props/{name}/set";
            ResponseTopic = "grpc/{clientId}/props/{name}/ack";
        }
    }
}
