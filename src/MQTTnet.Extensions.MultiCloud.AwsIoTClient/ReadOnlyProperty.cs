using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient
{
    public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
    {
        public ReadOnlyProperty(IMqttClient mqttClient, string name)
            : base(mqttClient, name)
        {
            TopicPattern = "$aws/things/{clientId}/shadow/update";
            WrapMessage = true;
            Retain = false;
        }
    }
}
