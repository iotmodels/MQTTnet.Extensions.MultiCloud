using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class HubWritablePropertyUTFJson<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
    {
        public T? Value { get; set; }
        public HubWritablePropertyUTFJson(IMqttClient c, string name)
            : base(c, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "$iothub/twin/PATCH/properties/desired";
            ResponseTopic = "$iothub/twin/PATCH/properties/desired/ack";
        }
    }
}
