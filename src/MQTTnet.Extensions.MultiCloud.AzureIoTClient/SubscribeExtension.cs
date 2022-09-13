using MQTTnet.Client;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    internal static class SubscribeExtension
    {
        private static List<string> subscriptions = new List<string>();

        public static void SubscribeWithReply(this IMqttClient client, string topic, CancellationToken cancellationToken = default)
        {
            if (!subscriptions.Contains(topic))
            {
                subscriptions.Add(topic);
                Task.Run(async () => 
                { 
                    var subAck = await client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topic).Build(), cancellationToken);
                    subAck.TraceErrors();
                });
            }
        }

        public static void ReSuscribe(this IMqttClient client)
        {
            subscriptions.ForEach(async t =>
            {
                Trace.TraceInformation($"Re-Subscribing to {t}");
                var subAck = await client.SubscribeAsync(t);
                subAck.TraceErrors();
            });
        }
    }
}
