using System;
using System.Text;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Clients.Connections
{
    public class BirthConvention
    {
        public enum ConnectionStatus { offline, online }

        public static string BirthTopic(string clientId) => $"pnp/{clientId}/birth";

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
            public string ModelId { get; set; }
            [JsonPropertyName("when")]
            public DateTime When { get; set; }
            [JsonPropertyName("status")]
            public ConnectionStatus ConnectionStatus { get; set; }

            public string ToJson() => Json.Stringify(this);
        }

        public static byte[] LastWillPayload() => Encoding.UTF8.GetBytes(Json.Stringify(new BirthMessage(ConnectionStatus.offline)));
        public static byte[] LastWillPayload(string modelId) => Encoding.UTF8.GetBytes(Json.Stringify(new BirthMessage(ConnectionStatus.offline) { ModelId = modelId }));
    }
}
