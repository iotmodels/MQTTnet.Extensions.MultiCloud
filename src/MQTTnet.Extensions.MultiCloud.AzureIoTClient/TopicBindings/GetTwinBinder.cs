using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class GetTwinBinder : IPropertyStoreReader
    {
        private static readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetTwinRequests = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
        private readonly IMqttClient connection;

        internal int lastRid = -1;

        public GetTwinBinder(IMqttClient conn)
        {
            connection = conn;
            var subAck = connection.SubscribeAsync("$iothub/twin/res/#").Result;
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                if (topic.StartsWith("$iothub/twin/res/200"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    (int rid, _) = TopicParser.ParseTopic(topic);
                    if (pendingGetTwinRequests.TryRemove(rid, out var tcs))
                    {
                        tcs.SetResult(msg);
                    }
                }
                await Task.Yield();
            };
        }

        public async Task<string> ReadPropertiesDocAsync(CancellationToken cancellationToken = default)
        {
            var rid = RidCounter.NextValue();
            lastRid = rid; // for testing
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var puback = await connection.PublishStringAsync($"$iothub/twin/GET/?$rid={rid}", string.Empty, MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);

            if (puback.ReasonCode == 0)
            {
                pendingGetTwinRequests.TryAdd(rid, tcs);
            }
            else
            {
                Trace.TraceError($"Error '{puback}' publishing twin GET");
            }
            return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
        }

    }
}
