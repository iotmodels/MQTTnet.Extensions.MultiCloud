using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericCommandResponse : IGenericCommandResponse
    {
        [JsonPropertyName("payload")]
        public string? ReponsePayload { get; set; }

        [JsonIgnore]
        public int Status { get; set; }
    }
}
