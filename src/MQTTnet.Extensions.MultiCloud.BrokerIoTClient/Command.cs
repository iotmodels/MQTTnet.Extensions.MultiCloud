using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class Command<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public Command(IMqttClient client, string name)
        : base(client, name)
    {
        SubscribeTopicPattern = "device/{clientId}/commands/{name}";
        RequestTopicPattern = "device/{clientId}/commands/{name}";
        ResponseTopicPattern = "device/{clientId}/commands/{name}/resp";
    }
}

public class Command<T> : CloudToDeviceBinder<T, string>, ICommand<T>
{
    public Command(IMqttClient client, string name)
        : base(client, name)
    {
        SubscribeTopicPattern = "device/{clientId}/commands/{name}";
        RequestTopicPattern = "device/{clientId}/commands/{name}";
        ResponseTopicPattern = "device/{clientId}/commands/{name}/resp";
    }
}

public class Command : CloudToDeviceBinder<string, string>, ICommand
{
    public Command(IMqttClient client, string name)
        : base(client, name)
    {
        SubscribeTopicPattern = "device/{clientId}/commands/{name}";
        RequestTopicPattern = "device/{clientId}/commands/{name}";
        ResponseTopicPattern = "device/{clientId}/commands/{name}/resp";
    }
}

