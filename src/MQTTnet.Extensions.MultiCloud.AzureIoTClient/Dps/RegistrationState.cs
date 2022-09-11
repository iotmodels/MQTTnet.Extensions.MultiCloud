using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Dps
{
    public class RegistrationState
    {
        [JsonPropertyName("registrationId")]
        public string RegistrationId { get; set; } = string.Empty;

        [JsonPropertyName("assignedHub")]
        public string AssignedHub { get; set; } = string.Empty;

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("subStatus")]
        public string SubStatus { get; set; } = string.Empty;

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
