using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud
{
    public abstract class BaseCommandResponse
    {
        [JsonIgnore]
        public int Status { get; set; }
    }
}
