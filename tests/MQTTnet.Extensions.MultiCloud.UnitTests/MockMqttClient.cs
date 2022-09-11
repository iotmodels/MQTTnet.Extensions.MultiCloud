using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Packets;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.UnitTests
{
    internal class MockMqttClient : IMqttClient
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MockMqttClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public bool IsConnected => throw new NotImplementedException();

        public string BaseClientLibraryInfo => "mock client";

        public MqttClientOptions Options => new MqttClientOptions() { ClientId = "mock" };

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

        public Task<int> SubscribeAsync(string topic, CancellationToken token = default)
        {
            numSubscriptions++;
            //options.TopicFilters.ForEach(t => Trace.TraceInformation(t.Topic));
            return Task.FromResult(0);
        }

        public Task<int> UnsubscribeAsync(string topic, CancellationToken token = default)
        {
            numSubscriptions--;
            return Task.FromResult(0);
        }

        public Task DisconnectAsync(CancellationToken token = default)
        {
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
            return Task.FromResult(new MqttClientSubscribeResult());
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
