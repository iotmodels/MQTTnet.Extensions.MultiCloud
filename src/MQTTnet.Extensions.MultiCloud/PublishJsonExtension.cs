using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
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
            bool retained = false,  CancellationToken stoppingToken = default)
        {
            return await client.PublishStringAsync(topic, (payload is string) ? payload as string : Json.Stringify(payload!), qos, retained, stoppingToken);
        }
    }
}
