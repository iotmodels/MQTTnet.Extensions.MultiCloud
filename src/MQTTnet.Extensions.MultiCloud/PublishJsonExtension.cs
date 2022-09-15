using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public static class PublishJsonExtension
    {
        public static async Task<MqttClientPublishResult> PublishJsonAsync(this IMqttClient client,
            string topic,
            object? payload = null,
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce,
            bool retained = false, CancellationToken stoppingToken = default)
        {
            string? jsonPayload = (payload is string) ? payload as string : Json.Stringify(payload!);
            return await client.PublishStringAsync(topic, jsonPayload, qos, retained, stoppingToken);
        }
    }
}
