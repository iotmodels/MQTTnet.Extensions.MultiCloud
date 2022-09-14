using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith("$iothub/twin/res/204"))
                {
                    (int rid, int twinVersion) = TopicParser.ParseTopic(topic);
                    if (pendingRequests.TryGetValue(rid, out var tcs))
                    {
                        tcs.SetResult(twinVersion);
                    }
                    else
                    {
                        Trace.TraceWarning($"RID: UpdateTwinBinder {rid} not found in pending requests. Topic: {topic}");
                    }

                }
                else if (topic.StartsWith("$iothub/twin/res/400"))
                {
                    (int rid, int twinVersion) = TopicParser.ParseTopic(topic);
                    if (pendingRequests.TryGetValue(rid, out var tcs))
                    {
                        Trace.TraceError($"Error for RID {rid} {Encoding.UTF8.GetString(m.ApplicationMessage.Payload)}");
                        tcs.SetException(new ApplicationException($"Error for RID {rid}"));
                    }
                    else
                    {
                        Trace.TraceWarning($"RID: UpdateTwinBinder {rid} not found in pending requests. Topic: {topic}");
                    }
                }
                await Task.Yield();
            };
        }



        public async Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            await connection.SubscribeWithReply("$iothub/twin/res/#");
            var rid = RidCounter.NextValue();
            var puback = await connection.PublishJsonAsync($"$iothub/twin/PATCH/properties/reported/?$rid={rid}", payload, MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (puback.ReasonCode == 0)
            {
                if (pendingRequests.TryAdd(rid, tcs))
                {
                    Trace.TraceWarning($"UpdTwinBinder: RID {rid} added to pending requests");
                }
                else
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
