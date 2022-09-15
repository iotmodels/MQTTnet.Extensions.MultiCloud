using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud
{
    public class GenericCommandResponse : BaseCommandResponse
    {
        [JsonPropertyName("payload")]
        public string? ReponsePayload { get; set; }
    }
}
