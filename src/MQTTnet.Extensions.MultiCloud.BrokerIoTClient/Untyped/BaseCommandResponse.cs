using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public abstract class BaseCommandResponse
    {
        [JsonIgnore]
        public int Status { get; set; }
    }
}
