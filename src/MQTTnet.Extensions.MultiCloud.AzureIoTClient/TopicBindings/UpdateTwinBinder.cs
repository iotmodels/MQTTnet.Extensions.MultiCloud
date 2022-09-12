using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class UpdateTwinBinder : IReportPropertyBinder, IPropertyStoreWriter
    {
        private readonly ConcurrentDictionary<int, TaskCompletionSource<int>> pendingRequests = new ConcurrentDictionary<int, TaskCompletionSource<int>>();
        private readonly IMqttClient connection;

        public UpdateTwinBinder(IMqttClient connection)
        {
            this.connection = connection;
            var subAck = connection.SubscribeAsync("$iothub/twin/res/#").Result;
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith("$iothub/twin/res/204"))
                {
                    (int rid, int twinVersion) = TopicParser.ParseTopic(topic);
                    if (pendingRequests.TryRemove(rid, out var tcs))
                    {
                        tcs.SetResult(twinVersion);
                    } 
                    else
                    {
                        Trace.TraceWarning($"RID: {rid} not found in pending requests. Actual : {string.Join(" ", pendingRequests.Keys.ToArray<int>())}");
                    }

                }
                await Task.Yield();
            };
        }

        public async Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            var rid = RidCounter.NextValue();
            var puback = await connection.PublishStringAsync($"$iothub/twin/PATCH/properties/reported/?$rid={rid}", Json.Stringify(payload), MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (puback.ReasonCode == 0)
            {
                if (!pendingRequests.TryAdd(rid, tcs))
                {
                    Trace.TraceWarning($"UpdTwinBinder: RID {rid} not added to pending requests");
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
