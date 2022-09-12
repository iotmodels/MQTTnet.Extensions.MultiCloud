using MQTTnet.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public class UpdateShadowBinder : IReportPropertyBinder, IPropertyStoreWriter
    {
        private readonly ConcurrentQueue<TaskCompletionSource<int>> pendingRequests;
        private readonly IMqttClient connection;

        public UpdateShadowBinder(IMqttClient connection)
        {
            this.connection = connection;
            pendingRequests = new ConcurrentQueue<TaskCompletionSource<int>>();
            _ = connection.SubscribeAsync($"$aws/things/{connection.Options.ClientId}/shadow/update/accepted");
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                await Task.Yield();
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"$aws/things/{connection.Options.ClientId}/shadow/update/accepted"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    JsonNode? node = JsonNode.Parse(msg);
                    int version = node!["version"]!.GetValue<int>();
                    if (pendingRequests.TryDequeue(out var pendingRequest))
                    {
                        pendingRequest.SetResult(version);
                    }
                }
            };
        }

        public async Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<int>();
            pendingRequests.Enqueue(tcs);
            Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "state", new Dictionary<string, object>()
                    {
                       { "reported", payload}
                    }
                }
            };
            var puback = await connection.PublishStringAsync($"$aws/things/{connection.Options.ClientId}/shadow/update", Json.Stringify(data), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, cancellationToken);
            if (puback.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                Trace.TraceError("Error publishing message: " + puback);
                throw new ApplicationException("Publishing Exception");
            }
            return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(15));
        }
    }
}
