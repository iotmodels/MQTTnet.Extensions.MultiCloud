using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Command;

public class CommandUTF8Json<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandUTF8Json(IMqttClient client, string name)
        : base(client, name, new UTF8JsonSerializer())
    {
        TopicTemplate = "device/{clientId}/commands/{name}";
        ResponseTopic = "device/{clientId}/commands/{name}/resp";
    }
}
