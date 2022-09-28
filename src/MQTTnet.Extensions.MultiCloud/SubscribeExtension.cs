using MQTTnet.Client;
using System.Diagnostics;

namespace MQTTnet.Extensions.MultiCloud
{
    public static class SubscribeExtension
    {
        private static readonly List<string> subscriptions = new List<string>();

        public static async Task SubscribeWithReplyAsync(this IMqttClient client, string topic, CancellationToken cancellationToken = default)
        {
            if (!subscriptions.Contains(topic))
            {
                subscriptions.Add(topic);

                var subAck = await client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topic).Build(), cancellationToken);
                subAck.TraceErrors();

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
