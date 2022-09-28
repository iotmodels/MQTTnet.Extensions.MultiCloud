using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings;

public class ShadowRequestResponseBinder
{
    internal int lastRid = -1;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetshadowRequests = new();
    public Func<string, Task<string>>? OnMessage { get; set; }

    readonly IMqttClient connection;
    readonly string topicBase = String.Empty;
    public ShadowRequestResponseBinder(IMqttClient connection)
    {
        this.connection = connection;
        string deviceId = connection.Options.ClientId;
        topicBase = $"$aws/things/{deviceId}/shadow";

        connection.ApplicationMessageReceivedAsync += async m =>
        {
            await Task.Yield();
           
            var topic = m.ApplicationMessage.Topic;
            if (topic.StartsWith(topicBase + "/+/accepted"))
            {
                string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? Array.Empty<byte>());
                (int rid, _) = TopicParser.ParseTopic(topic);
                if (pendingGetshadowRequests.TryGetValue(rid, out var tcs))
                {
                    tcs.SetResult(msg);
                    Trace.TraceWarning($"GetshadowBinder: RID {rid} found in pending requests");
                }
                else
                {
                    Trace.TraceWarning($"GetshadowBinder: RID {rid} not found pending requests");
                }
            }
        };
    }

    public async Task<string> GetShadowAsync(CancellationToken cancellationToken = default)
    {
        await connection.SubscribeAsync(topicBase + "/get/accepted", cancellationToken: cancellationToken);
        var rid = RidCounter.NextValue();
        lastRid = rid; // for testing
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var puback = await connection.PublishBinaryAsync(
            topicBase + "/get",
            Array.Empty<byte>(),
            MqttQualityOfServiceLevel.AtMostOnce,
            false,
            cancellationToken);

        if (puback.ReasonCode == 0)
        {
            if (pendingGetshadowRequests.TryAdd(rid, tcs))
            {
                Trace.TraceWarning($"GetshadowBinder: RID {rid} added to pending requests");
            }
            else
            {
                Trace.TraceWarning($"GetshadowBinder: RID {rid} not added to pending requests");
            }
        }
        else
        {
            Trace.TraceError($"Error '{puback}' publishing shadow GET");
        }
        return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
    }

    public async Task<string> UpdateShadowAsync(object payload, CancellationToken cancellationToken = default)
    {
        await connection.SubscribeWithReplyAsync("$iothub/shadow/res/#", cancellationToken: cancellationToken);
        var rid = RidCounter.NextValue();

        var puback = await connection.PublishBinaryAsync(
            topicBase + "/shadow/update",
            new UTF8JsonSerializer().ToBytes(payload),
            MqttQualityOfServiceLevel.AtMostOnce,
            false,
            cancellationToken);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (puback.ReasonCode == 0)
        {
            if (pendingGetshadowRequests.TryAdd(rid, tcs))
            {
                Trace.TraceWarning($"UpdshadowBinder: RID {rid} added to pending requests");
            }
            else
            {
                Trace.TraceWarning($"UpdshadowBinder: RID {rid} not added to pending requests");
            }
        }
        else
        {
            Trace.TraceError($"Error '{puback}' publishing shadow GET");
        }
        return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
    }


    [DebuggerStepThrough()]
    static class RidCounter
    {
        private static int counter = 0;
        internal static int Current => counter;
        internal static int NextValue() => Interlocked.Increment(ref counter);
        internal static void Reset() => counter = 0;
    }

    class TopicParser
    {
        internal static (int rid, int shadowVersion) ParseTopic(string topic)
        {
            var segments = topic.Split('/');
            int shadowVersion = -1;
            int rid = -1;
            if (topic.Contains('?'))
            {
                var qs = HttpUtility.ParseQueryString(segments[^1]);
                _ = int.TryParse(qs["$rid"], out rid);
                shadowVersion = Convert.ToInt32(qs["$version"]);
            }
            return (rid, shadowVersion);
        }
    }
}
