using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;

public class TwinRequestResponseBinder
{
    internal int lastRid = -1;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetTwinRequests = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
    private readonly ConcurrentDictionary<int, TaskCompletionSource<int>> pendingUpdateTwinRequests = new ConcurrentDictionary<int, TaskCompletionSource<int>>();
    public Func<string, Task<string>>? OnMessage { get; set; }

    private readonly IMqttClient connection;
    public TwinRequestResponseBinder(IMqttClient connection)
    {
        this.connection = connection;
        connection.ApplicationMessageReceivedAsync += async m =>
        {
            await Task.Yield();

            var topic = m.ApplicationMessage.Topic;
            if (topic.StartsWith("$iothub/twin/res/200"))
            {
                string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? new byte[0]);
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
            if (topic.StartsWith("$iothub/twin/res/204"))
            {
                string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? new byte[0]);
                (int rid, int version) = TopicParser.ParseTopic(topic);
                if (pendingUpdateTwinRequests.TryGetValue(rid, out var tcs)!)
                {
                    tcs.SetResult(version);
                    Trace.TraceWarning($"UpdateTwinBinder: RID {rid} found in pending requests");
                }
                else
                {
                    Trace.TraceWarning($"UpdateTwinBinder: RID {rid} not found pending requests");
                }
            }
        };
    }

    public async Task<string> GetTwinAsync(CancellationToken cancellationToken = default)
    {
        await connection.SubscribeAsync("$iothub/twin/res/#");
        var rid = RidCounter.NextValue();
        lastRid = rid; // for testing
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var puback = await connection.PublishBinaryAsync(
            $"$iothub/twin/GET/?$rid={rid}",
            new byte[0],
            MqttQualityOfServiceLevel.AtMostOnce,
            false,
            cancellationToken);

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

    public async Task<int> UpdateTwinAsync(object payload, CancellationToken cancellationToken = default)
    {
        await connection.SubscribeWithReplyAsync("$iothub/twin/res/#");
        var rid = RidCounter.NextValue();

        var puback = await connection.PublishBinaryAsync(
            $"$iothub/twin/PATCH/properties/reported/?$rid={rid}",
            new UTF8JsonSerializer().ToBytes(payload),
            MqttQualityOfServiceLevel.AtMostOnce,
            false,
            cancellationToken);

        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (puback.ReasonCode == 0)
        {
            if (pendingUpdateTwinRequests.TryAdd(rid, tcs))
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


    [DebuggerStepThrough()]
    private static class RidCounter
    {
        private static int counter = 0;
        internal static int Current => counter;
        internal static int NextValue() => Interlocked.Increment(ref counter);
        internal static void Reset() => counter = 0;
    }

    private class TopicParser
    {
        internal static (int rid, int twinVersion) ParseTopic(string topic)
        {
            var segments = topic.Split('/');
            int twinVersion = -1;
            int rid = -1;
            if (topic.Contains("?"))
            {
                var qs = HttpUtility.ParseQueryString(segments[segments.Length - 1]);
                int.TryParse(qs["$rid"], out rid);
                twinVersion = Convert.ToInt32(qs["$version"]);
            }
            return (rid, twinVersion);
        }
    }
}
