using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class WritablePropertyUTFJson<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
    {
        public T? Value { get; set; }
        public int? Version { get; set; }

        public WritablePropertyUTFJson(IMqttClient c, string name) 
            : base(c, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "device/{clientId}/props/{name}/set";
            ResponseTopic = "device/{clientId}/props/{name}/ack";
        }
    }
}
