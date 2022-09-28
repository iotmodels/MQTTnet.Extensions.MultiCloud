using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class Command<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public Command(IMqttClient client, string name)
        : base(client, name)
    {
        TopicTemplate = "device/{clientId}/commands/{name}";
        ResponseTopic = "device/{clientId}/commands/{name}/resp";
    }
}
