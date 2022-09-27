using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty
{
    public class HubReadOnlyPropertyUTFJson<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
    {
        public HubReadOnlyPropertyUTFJson(IMqttClient mqttClient, string name) : base(mqttClient, name, new UTF8JsonSerializer())
        {
            topicPattern = "$iothub/twin/PATCH/properties/reported/?$rid=1";
            wrapMessage = true;
            nameInTopic = false;
            retain = false;
        }
    }
}
