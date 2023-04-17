using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommandResponse : IGenericCommandResponse
    {
        public string? ReponsePayload { get; set; }
        public int Status { get; set; }
    }
}
