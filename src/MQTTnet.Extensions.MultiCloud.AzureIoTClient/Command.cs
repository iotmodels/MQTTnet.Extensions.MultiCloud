using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class Command<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public Command(IMqttClient client, string name)
    : base(client, name)
    {
        RequestTopicPattern = "$iothub/methods/POST/{name}/#";
        ResponseTopicPattern = "$iothub/methods/res/200/?$rid={rid}";
    }
}
