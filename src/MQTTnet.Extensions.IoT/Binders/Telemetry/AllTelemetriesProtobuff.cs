using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.Telemetry
{
    public class AllTelemetriesProtobuff<T> : DeviceToCloudBinder<T>, ITelemetry<T>
    {
        public AllTelemetriesProtobuff(IMqttClient mqttClient) 
            : base(mqttClient, string.Empty, new ProtoBuffSerializer())
        {
            topicPattern = "grpc/{clientId}/tel";
            this.nameInTopic = false;
            this.wrapMessage = false;
        }

    }
}
