using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommandRequest : IGenericCommandRequest
    {
        public string? CommandName { get; set; }
        public string? CommandPayload { get; set; }
        [JsonIgnore]
        public byte[]? CorrelationId { get; set; }
    }
}
