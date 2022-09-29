using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class Command<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public Command(IMqttClient client, string name)
    : base(client, name)
    {
        TopicTemplate = "$iothub/methods/POST/{name}/#";
        ResponseTopic = "$iothub/methods/res/200/?$rid={rid}";
    }
}
