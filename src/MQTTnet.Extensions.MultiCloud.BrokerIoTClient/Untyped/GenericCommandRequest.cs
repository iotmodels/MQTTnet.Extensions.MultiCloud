using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommandRequest
    {
        public string? CommandName { get; set; }
        public object? RequestPayload { get; set; }
        [JsonIgnore]
        public byte[]? CorrelationId { get; set; }
    }
}
