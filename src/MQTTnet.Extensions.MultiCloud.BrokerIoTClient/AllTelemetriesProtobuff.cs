using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class AllTelemetriesProtobuff<T> : DeviceToCloudBinder<T>, ITelemetry<T>
    {
        public AllTelemetriesProtobuff(IMqttClient mqttClient)
            : base(mqttClient, string.Empty, new ProtobufSerializer())
        {
            TopicPattern = "device/{clientId}/tel";
            NameInTopic = false;
            WrapMessage = false;
        }

    }
}
