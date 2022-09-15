using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud
{
    public class CommandResponse : BaseCommandResponse
    {
        [JsonPropertyName("payload")]
        public string? ReponsePayload { get; set; }
    }
}
