using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface ITelemetry<T>
    {
        Task<MqttClientPublishResult> SendTelemetryAsync(T payload, CancellationToken cancellationToken = default);
        Task<MqttClientPublishResult> SendTelemetryAsync(byte[] payload, CancellationToken cancellationToken = default);
    }
}