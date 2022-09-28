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
    public class HubTelemetryUTF8Json<T> : DeviceToCloudBinder<T>, ITelemetry<T>
    {
        public HubTelemetryUTF8Json(IMqttClient mqttClient, string name)
        : base(mqttClient, name, new UTF8JsonSerializer())
        {
            topicPattern = "devices/{clientId}/messages/events/";
            wrapMessage = true;
        }
    }
}
