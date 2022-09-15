using MQTTnet.Client;
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
        private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetTwinRequests = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
        private readonly IMqttClient connection;

        internal int lastRid = -1;

        public GetTwinBinder(IMqttClient conn)
        {
            connection = conn;
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                await Task.Yield();

                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith("$iothub/twin/res/200"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    (int rid, _) = TopicParser.ParseTopic(topic);
                    if (pendingGetTwinRequests.TryGetValue(rid, out var tcs))
                    {
                        tcs.SetResult(msg);
                        Trace.TraceWarning($"GetTwinBinder: RID {rid} found in pending requests");
                    }
                    else
                    {
                        Trace.TraceWarning($"GetTwinBinder: RID {rid} not found pending requests");
                    }
                }
            };
        }

        public async Task<string> ReadPropertiesDocAsync(CancellationToken cancellationToken = default)
        {
            await connection.SubscribeWithReplyAsync("$iothub/twin/res/#");
            var rid = RidCounter.NextValue();
            lastRid = rid; // for testing
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var puback = await connection.PublishJsonAsync($"$iothub/twin/GET/?$rid={rid}", string.Empty, MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);

            if (puback.ReasonCode == 0)
            {
                if (pendingGetTwinRequests.TryAdd(rid, tcs))
                {
                    Trace.TraceWarning($"GetTwinBinder: RID {rid} added to pending requests");
                }
                else
                {
                    Trace.TraceWarning($"GetTwinBinder: RID {rid} not added to pending requests");
                }
            }
            else
            {
                Trace.TraceError($"Error '{puback}' publishing twin GET");
            }
            return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
        }

    }
}
