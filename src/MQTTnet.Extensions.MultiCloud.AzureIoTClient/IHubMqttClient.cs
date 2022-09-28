using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using System.Text.Json.Nodes;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public interface IHubMqttClient
    {
        IMqttClient Connection { get; set; }
        Func<GenericCommandRequest, GenericCommandResponse> OnCommandReceived { get; set; }
        Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived { get; set; }

        Task<string> GetTwinAsync(CancellationToken cancellationToken = default);
        Task<string> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default);
        Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default);
        //Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default);
    }
}