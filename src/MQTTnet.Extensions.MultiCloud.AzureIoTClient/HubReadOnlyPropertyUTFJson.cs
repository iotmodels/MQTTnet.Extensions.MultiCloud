using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
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
