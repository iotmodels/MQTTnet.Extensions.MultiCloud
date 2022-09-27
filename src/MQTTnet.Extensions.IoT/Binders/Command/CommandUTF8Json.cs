using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;

namespace MQTTnet.Extensions.IoT.Binders.Command;

public class CommandUTF8Json<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
{
    public CommandUTF8Json(IMqttClient client, string name) 
        : base(client, name, new UTF8JsonSerializer())
    {
        TopicTemplate = "device/{clientId}/commands/{name}";
        topicResponseSuffix = "resp";
    }
}
