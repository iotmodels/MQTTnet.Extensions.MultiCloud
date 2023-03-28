using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommandResponse : BaseCommandResponse
    {
        public object? ReponsePayload { get; set; }
    }
}
