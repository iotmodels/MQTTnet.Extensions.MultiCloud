using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Clients
{
    public abstract class BaseCommandResponse
    {
        [JsonIgnore]
        public int Status { get; set; }
    }
}
