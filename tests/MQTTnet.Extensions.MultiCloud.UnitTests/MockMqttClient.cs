using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Packets;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    internal class MockMqttClient : IMqttClient
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MockMqttClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public bool IsConnected => throw new NotImplementedException();


        public MqttClientOptions Options => new() { ClientId = "mock" };

        public string payloadReceived;
        public string topicRecceived;

        internal int numSubscriptions = 0;

        public event Func<MqttClientConnectedEventArgs, Task> ConnectedAsync;
        public event Func<MqttClientConnectingEventArgs, Task> ConnectingAsync;
        public event Func<MqttClientDisconnectedEventArgs, Task> DisconnectedAsync;
        public event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceivedAsync;
        public event Func<InspectMqttPacketEventArgs, Task> InspectPackage;

        //public event EventHandler<DisconnectEventArgs> OnMqttClientDisconnected;
        //public event Func<MqttMessage, Task> OnMessage;



        public void SimulateNewMessage(string topic, string payload)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();
            var msgReceived = new MqttApplicationMessageReceivedEventArgs(Options.ClientId, msg, new MqttPublishPacket(), (ea, ct) => null);
            ApplicationMessageReceivedAsync.Invoke(msgReceived);
        }

        public void SimulateNewBinaryMessage(string topic, byte[] payload)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();
            var msgReceived = new MqttApplicationMessageReceivedEventArgs(Options.ClientId, msg, new MqttPublishPacket(), (ea, ct) => null);
            ApplicationMessageReceivedAsync.Invoke(msgReceived);
        }

        public Delegate[] GetInvocationList() => ApplicationMessageReceivedAsync.GetInvocationList();

        //public Task<int> PublishAsync(string topic, object payload, int qos = 0, bool retain = false, CancellationToken token = default)
        //{
        //    string? jsonPayload;
        //    if (payload is string)
        //    {
        //        jsonPayload = payload as string;
        //    }
        //    else
        //    {
        //        jsonPayload = JsonSerializer.Serialize(payload);
        //    }

        //    topicRecceived = topic;
        //    payloadReceived = jsonPayload!;// != null ? Encoding.UTF8.GetString(payload) : string.Empty;
        //    return Task.FromResult(0);
        //}
        public string SubscribedTopicReceived { get; set; }
        public Task<int> SubscribeAsync(string topic, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            SubscribedTopicReceived = topic;
            numSubscriptions++;
            //options.TopicFilters.ForEach(t => Trace.TraceInformation(t.Topic));
            return Task.FromResult(0);
        }

        public string UnsubscribeTopicReceived { get; set; }
        public Task<int> UnsubscribeAsync(string topic, CancellationToken token = default)
        {
            UnsubscribeTopicReceived = topic;
            token.ThrowIfCancellationRequested();
            numSubscriptions--;
            return Task.FromResult(0);
        }

#pragma warning disable CA1822 // Mark members as static
        public Task DisconnectAsync(CancellationToken token = default)
#pragma warning restore CA1822 // Mark members as static
        {
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync(MqttClientDisconnectOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PingAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendExtendedAuthenticationExchangeDataAsync(MqttExtendedAuthenticationExchangeData data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MqttClientSubscribeResult> SubscribeAsync(MqttClientSubscribeOptions options, CancellationToken cancellationToken = default)
        {
            numSubscriptions++;
            //options.TopicFilters.ForEach(t => Trace.TraceInformation(t.Topic));
            var subAck = new MqttClientSubscribeResult();
            return Task.FromResult(subAck);
        }

        public Task<MqttClientUnsubscribeResult> UnsubscribeAsync(MqttClientUnsubscribeOptions options, CancellationToken cancellationToken = default)
        {
            numSubscriptions--;
            return Task.FromResult(new MqttClientUnsubscribeResult());
        }

        public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
        {
            string? jsonPayload = Encoding.UTF8.GetString(applicationMessage.Payload);

            //if (jsonPayload is string)
            //{
            //    jsonPayload = payload as string;
            //}
            //else
            //{
            //    jsonPayload = JsonSerializer.Serialize(payload);
            //}

            topicRecceived = applicationMessage.Topic;
            payloadReceived = jsonPayload!;// != null ? Encoding.UTF8.GetString(payload) : string.Empty;
            return Task.FromResult(new MqttClientPublishResult());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
