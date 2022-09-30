using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class WritablePropertyProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, IWritableProperty<T, TResp>
    {
        public T? Value { get; set; } = default!;
        public int? Version { get; set; } = -1;
        public WritablePropertyProtobuff(IMqttClient connection, string name, MessageParser parser)
            : base(connection, name, new ProtobufSerializer(parser))
        {
            TopicTemplate = "device/{clientId}/props/{name}/set";
            ResponseTopic = "device/{clientId}/props/{name}/ack";
            RetainResponse = true;
        }
    }
}
