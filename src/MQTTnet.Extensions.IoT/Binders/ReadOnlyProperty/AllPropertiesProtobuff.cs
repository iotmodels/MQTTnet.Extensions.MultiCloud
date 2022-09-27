using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty
{
    public class AllPropertiesProtobuff<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
    {
        public AllPropertiesProtobuff(IMqttClient mqttClient)
            : base(mqttClient, string.Empty, new ProtoBuffSerializer())
        {
            topicPattern = "grpc/{clientId}/props";
            nameInTopic = false;
            wrapMessage = false;
        }

    }
}
