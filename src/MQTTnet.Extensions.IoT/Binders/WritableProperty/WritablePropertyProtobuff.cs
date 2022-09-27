using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class WritablePropertyProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, IWritableProperty<T, TResp>
    {
        public T? Value { get; set; }

        public WritablePropertyProtobuff(IMqttClient connection, string name, MessageParser parser)
            : base(connection, name, new ProtoBuffSerializer(parser))
        {
            TopicTemplate = "grpc/{clientId}/props/{name}/set";
            topicResponseSuffix = "ack";
        }
    }
}
