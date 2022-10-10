using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class Command<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public Command(IMqttClient client, string name)
    : base(client, name)
    {
        RequestTopicPattern = "$iothub/methods/POST/#";
        ResponseTopicPattern = "$iothub/methods/res/200/?$rid={rid}";
    }
}

public class Command<T> : CloudToDeviceBinder<T, string>, ICommand<T>
{
    public Command(IMqttClient client, string name)
    : base(client, name)
    {
        RequestTopicPattern = "$iothub/methods/POST/#";
        ResponseTopicPattern = "$iothub/methods/res/200/?$rid={rid}";
    }
}

public class Command : CloudToDeviceBinder<string, string>, ICommand
{
    public Command(IMqttClient client, string name)
    : base(client, name)
    {
        RequestTopicPattern = "$iothub/methods/POST/#";
        ResponseTopicPattern = "$iothub/methods/res/200/?$rid={rid}";
    }
}