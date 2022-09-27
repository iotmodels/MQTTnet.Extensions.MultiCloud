using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class HubWritablePropertyUTFJson<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
    {
        public T? Value { get; set; }
        public int? Version { get; set; }

        public HubWritablePropertyUTFJson(IMqttClient c, string name)
            : base(c, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "$iothub/twin/PATCH/properties/desired/#";
            ResponseTopic = "$iothub/twin/PATCH/properties/reported/?$rid={rid}&version={version}";
            unwrapRequest = true;
            wrapResponse = true;
            PreProcessMessage = tp =>
            {
                Version = tp.Version;
            };
        }
    }
}
