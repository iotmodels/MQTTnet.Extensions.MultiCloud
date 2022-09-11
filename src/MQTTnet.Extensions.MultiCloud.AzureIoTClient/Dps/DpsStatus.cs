using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Dps
{
    public class DpsStatus
    {
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("registrationState")]
        public RegistrationState RegistrationState { get; set; } = new RegistrationState();
    }
}
