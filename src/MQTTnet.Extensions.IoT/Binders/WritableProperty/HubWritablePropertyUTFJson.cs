using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class HubWritablePropertyUTFJson<T> : CloudToDeviceBinder<T, Ack<T>>, IWritableProperty<T>
    {
        int version = -1;
        public T? Value { get; set; }
        public HubWritablePropertyUTFJson(IMqttClient c, string name)
            : base(c, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "$iothub/twin/PATCH/properties/desired/#";
            ResponseTopic = "$iothub/twin/res/204/?$rid=15&version=151";
            unwrapRequest = true;
            wrapResponse = true;
            PreProcessMessage = topic =>
            {
                (_, version) = TopicParser.ParseTopic(topic);
            };
            PostProcessMessage = () => version.ToString();
            
        }
    }
}
