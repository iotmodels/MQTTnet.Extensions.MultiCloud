using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public abstract class BaseCommandResponse
    {
        [JsonIgnore]
        public int Status { get; set; }
    }
}
