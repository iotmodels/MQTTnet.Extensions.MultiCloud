using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericCommandResponse : BaseCommandResponse
    {
        [JsonPropertyName("payload")]
        public string? ReponsePayload { get; set; }
    }
}
