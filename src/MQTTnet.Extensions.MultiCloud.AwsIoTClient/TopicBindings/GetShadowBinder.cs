using MQTTnet.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public class GetShadowBinder : IPropertyStoreReader
    {
        private readonly ConcurrentQueue<TaskCompletionSource<string>> pendingGetShadowRequests;
        private readonly IMqttClient connection;
        private readonly string topicBase;

        public GetShadowBinder(IMqttClient conn)
        {
            connection = conn;
            pendingGetShadowRequests = new ConcurrentQueue<TaskCompletionSource<string>>();
            string deviceId = conn.Options.ClientId;
            topicBase = $"$aws/things/{deviceId}/shadow";
            var subAck = connection.SubscribeAsync(topicBase + "/get/accepted").Result;
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                if (topic.StartsWith(topicBase + "/get/accepted"))
                {
                    string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    if (pendingGetShadowRequests.TryDequeue(out var pendingGetShadowRequest))
                    {
                        pendingGetShadowRequest.SetResult(msg);
                    }
                }
                await Task.Yield();
            };
        }

        public async Task<string> ReadPropertiesDocAsync(CancellationToken cancellationToken = default)
        {
            var pendingGetShadowRequest = new TaskCompletionSource<string>();
            pendingGetShadowRequests.Enqueue(pendingGetShadowRequest);
            await connection.PublishStringAsync(topicBase + "/get", string.Empty, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, cancellationToken);
            return await pendingGetShadowRequest.Task.TimeoutAfter(TimeSpan.FromSeconds(15));
        }
    }
}
