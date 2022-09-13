using MQTTnet.Client;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public interface IHubMqttClient
    {
        IMqttClient Connection { get; set; }
        Func<GenericCommandRequest, Task<CommandResponse>> OnCommandReceived { get; set; }
        Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived { get; set; }

        Task<string> GetTwinAsync(CancellationToken cancellationToken = default);
        Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default);
        Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default);
        Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default);
    }
}