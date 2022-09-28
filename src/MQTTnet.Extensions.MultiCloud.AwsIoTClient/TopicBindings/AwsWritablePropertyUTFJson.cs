using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            TopicTemplate = "$aws/things/{deviceId}/shadow/#";
            ResponseTopic = "$iothub/shadow/PATCH/properties/reported/?$rid={rid}&version={version}";
            UnwrapRequest = true;
            WrapResponse = true;
            PreProcessMessage = tp =>
            {
                Version = tp.Version;
            };
        }
    }
}
