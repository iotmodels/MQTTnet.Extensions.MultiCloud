using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class TwinRequestResponseBinder
{
    internal int lastRid = -1;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> pendingGetTwinRequests = new();
    private readonly ConcurrentDictionary<int, TaskCompletionSource<int>> pendingUpdateTwinRequests = new();
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
                string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? Array.Empty<byte>());
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
            else if (topic.StartsWith("$iothub/twin/res/204"))
            {
                string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload ?? Array.Empty<byte>());
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
            else if (topic.StartsWith("$iothub/twin/res"))
            {
                Trace.TraceWarning("topic: " + m.ApplicationMessage.Topic);
                Trace.TraceWarning("msg : " + Encoding.UTF8.GetString(m.ApplicationMessage.Payload!));
            }
        };
    }

    public async Task<string> GetTwinAsync(CancellationToken cancellationToken = default)
    {
        await connection.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("$iothub/twin/res/#")
            .Build(), 
            cancellationToken);

        var rid = RidCounter.NextValue();
        lastRid = rid; // for testing
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var puback = await connection.PublishBinaryAsync(
            $"$iothub/twin/GET/?$rid={rid}",
            Array.Empty<byte>(),
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
        await connection.SubscribeWithReplyAsync("$iothub/twin/res/#", cancellationToken);
        byte[] patchBytes;
        if (payload is string @string)
        {
            patchBytes = Encoding.UTF8.GetBytes(@string);
        }
        else
        {
            patchBytes = new UTF8JsonSerializer().ToBytes(payload);
        }
        
        var rid = RidCounter.NextValue(); 
        var puback = await connection.PublishBinaryAsync(
            $"$iothub/twin/PATCH/properties/reported/?$rid={rid}",
            patchBytes,
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
            if (topic.Contains('?'))
            {
                var qs = HttpUtility.ParseQueryString(segments[^1]);
                if (int.TryParse(qs["$rid"], out rid))
                {
                    twinVersion = Convert.ToInt32(qs["$version"]);
                }
            }
            return (rid, twinVersion);
        }
    }
}
