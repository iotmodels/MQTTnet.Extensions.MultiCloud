using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public static class PublishBytesExtension
    {
        public static async Task<MqttClientPublishResult> PublishBytesAsync(this IMqttClient client,
            string topic,
            byte[] payload,
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce,
            bool retained = false, 
            CancellationToken stoppingToken = default)
        {
            return await client.PublishAsync(new MqttApplicationMessageBuilder()
                                            .WithTopic(topic)
                                            .WithPayload(payload)
                                            .WithRetainFlag(retained)
                                            .WithQualityOfServiceLevel(qos)
                                            .Build(), 
                                        stoppingToken);
        }
    }
}
