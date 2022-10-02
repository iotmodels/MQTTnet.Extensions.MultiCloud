using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public class AwsWritablePropertyUTFJson<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
    {
        public T? Value { get; set; }
        public int? Version { get; set; }

        public AwsWritablePropertyUTFJson(IMqttClient c, string name)
            : base(c, name, new UTF8JsonSerializer())
        {
            RequestTopicPattern = "$aws/things/{deviceId}/shadow/#";
            ResponseTopicPattern = "$aws/things/{deviceId}/shadow/accepted";
            UnwrapRequest = true;
            WrapResponse = true;
            PreProcessMessage = tp =>
            {
                Version = tp.Version;
            };
        }

        public Task SendMessageAsync(Ack<T> payload, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
