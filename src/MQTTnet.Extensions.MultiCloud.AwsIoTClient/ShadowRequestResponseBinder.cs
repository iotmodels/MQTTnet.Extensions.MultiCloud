using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient;

public class ShadowRequestResponseBinder
{
    internal int lastRid = -1;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetshadowRequests = new();
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingUpdateShadowRequests = new();
    public Func<string, Task<string>>? OnMessage { get; set; }

    private readonly IMqttClient connection;
    private readonly string topicBase = string.Empty;
    public ShadowRequestResponseBinder(IMqttClient connection)
    {
        this.connection = connection;
        string deviceId = connection.Options.ClientId;
        topicBase = $"$aws/things/{deviceId}/shadow";

        connection.ApplicationMessageReceivedAsync += async m =>
        {
            var topic = m.ApplicationMessage.Topic;
            string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? Array.Empty<byte>());
            if (topic.StartsWith(topicBase + "/get/accepted"))
            {
                //(int rid, _) = TopicParser.ParseTopic(topic);
                if (pendingGetshadowRequests.TryGetValue(RidCounter.Current, out var tcs))
                {
                    tcs.SetResult(msg);
                    Trace.TraceWarning($"GetshadowBinder: RID {RidCounter.Current} found in pending requests");
                }
                else
                {
                    Trace.TraceWarning($"GetshadowBinder: RID {RidCounter.Current} not found pending requests");
                }
            }
            if (topic.StartsWith(topicBase + "/update/accepted"))
            {
                if (pendingUpdateShadowRequests.TryGetValue(RidCounter.Current, out var tcs))
                {
                    tcs.SetResult(msg);
                    Trace.TraceWarning($"UpdateshadowBinder: RID {RidCounter.Current} found in pending requests");
                }
                else
                {
                    Trace.TraceWarning($"UpdateshadowBinder: RID {RidCounter.Current} not found pending requests");
                }

            }
            await Task.Yield();
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
        await connection.SubscribeWithReplyAsync(topicBase + "/update/+", cancellationToken: cancellationToken);
        var rid = RidCounter.NextValue();

        var shadowUpdate = new
        {
            state = new
            {
                reported = payload
            }
        };

        var puback = await connection.PublishBinaryAsync(
            topicBase + "/update",
            new ShadowSerializer().ToBytes(shadowUpdate),
            MqttQualityOfServiceLevel.AtMostOnce,
            false,
            cancellationToken);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (puback.ReasonCode == 0)
        {
            if (pendingUpdateShadowRequests.TryAdd(rid, tcs))
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
    private static class RidCounter
    {
        private static int counter = 0;
        internal static int Current => counter;
        internal static int NextValue() => Interlocked.Increment(ref counter);
        internal static void Reset() => counter = 0;
    }

    private class TopicParser
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
