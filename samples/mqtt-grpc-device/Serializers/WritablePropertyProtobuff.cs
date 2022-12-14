using Google.Protobuf;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using System.Threading;
using System.Threading.Tasks;

namespace mqtt_grpc_device.Serializers
{
    public class WritablePropertyProtobuff<T, TResp> : CloudToDeviceBinder<T, TResp>, IWritableProperty<T, TResp>, IDeviceToCloud<TResp>
    {
        readonly IMqttClient _connection;
        readonly string _name;
        public T? Value { get; set; } = default!;
        public int? Version { get; set; } = -1;
        public WritablePropertyProtobuff(IMqttClient connection, string name, MessageParser parser)
            : base(connection, name, new ProtobufSerializer(parser))
        {
            _connection = connection;
            _name = name;
            SubscribeTopicPattern = "device/{clientId}/props/{name}/set/#";
            RequestTopicPattern = "device/{clientId}/props/{name}/set";
            ResponseTopicPattern = "device/{clientId}/props/{name}/ack";
            RetainResponse = true;
            PreProcessMessage = tp =>
            {
                Version = tp.Version;
            };
        }

        public async Task SendMessageAsync(TResp payload, CancellationToken cancellationToken = default)
        {
            var prop = new ReadOnlyProperty<TResp>(_connection, _name)
            {
                TopicPattern = "device/{clientId}/props/{name}/ack",
                WrapMessage = false
            };
            await prop.SendMessageAsync(payload, cancellationToken);
        }

        public async Task InitPropertyAsync(string intialState, TResp defaultValue, CancellationToken cancellationToken = default)
        {
            TResp payload = default!; //TODO use generic ACK for protos
            await SendMessageAsync(payload, cancellationToken);

        }


    }
}
