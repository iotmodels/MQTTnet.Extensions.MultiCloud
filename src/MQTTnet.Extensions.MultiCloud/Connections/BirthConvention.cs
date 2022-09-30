using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Connections;

public class BirthConvention
{
    public enum ConnectionStatus { offline, online }

    public static string BirthTopic(string clientId) => $"registry/{clientId}/status";

    public class BirthMessage
    {
        public BirthMessage(ConnectionStatus connectionStatus)
        {
            ConnectionStatus = connectionStatus;
            When = DateTime.Now;
            ModelId = "";
        }
        [JsonPropertyName("model-id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ModelId { get; set; }
        [JsonPropertyName("when")]
        public DateTime When { get; set; }
        [JsonPropertyName("status")]
        public ConnectionStatus ConnectionStatus { get; set; }
    }

    public static byte[] LastWillPayload() =>
        new UTF8JsonSerializer().ToBytes(new BirthMessage(ConnectionStatus.offline));
    public static byte[] LastWillPayload(string modelId) =>
        new UTF8JsonSerializer().ToBytes(new BirthMessage(ConnectionStatus.offline) { ModelId = modelId });
}
