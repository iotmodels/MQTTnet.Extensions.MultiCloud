using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient
{
    public class WritableProperty<T> : IWritableProperty<T>
    {
        public T? Value { get; set; }
        public int? Version { get; set; }
        
        public Func<T, Task<Ack<T>>>? OnMessage { get; set; }

        readonly ShadowSerializer serializer = new();
        const string subscribeTopicPattern = "$aws/things/{clientId}/shadow/update/delta";
        const string responseTopic = "$aws/things/{clientId}/shadow/update";

        readonly IMqttClient _connection;
        readonly string _name;

        public WritableProperty(IMqttClient c, string name)
        {
            _connection = c;
            _name = name;
            var deltaTopic = subscribeTopicPattern.Replace("{clientId}", c.Options.ClientId);
            _ = c.SubscribeAsync(deltaTopic);
            c.ApplicationMessageReceivedAsync += async msg =>
            {
                var topic = msg.ApplicationMessage.Topic;
                if (topic == deltaTopic)
                {
                    if (serializer.TryReadFromBytes(msg.ApplicationMessage.Payload, name, out T req))
                    {
                        Ack<T> resp = await OnMessage?.Invoke(req)!;
                        if (resp != null)
                        {
                            var resTopic = responseTopic.Replace("{clientId}", c.Options.ClientId);
                            byte[] responseBytes = serializer.ToBytes(resp.Value, name, serializer.Version);
                            _ = c.PublishAsync(
                            new MqttApplicationMessageBuilder()
                                .WithTopic(resTopic)
                                .WithPayload(responseBytes)
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                .Build());
                        }
                    }
                }
            };
        }

        public async Task SendMessageAsync(Ack<T> payload, CancellationToken cancellationToken = default)
        {
            var resTopic = responseTopic.Replace("{clientId}", _connection.Options.ClientId);
            await _connection.PublishAsync(new MqttApplicationMessageBuilder()
                                .WithTopic(resTopic)
                                .WithPayload(serializer.ToBytes(payload.Value, _name, payload.Version!.Value))
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                .Build(), cancellationToken);
        }

        public async Task InitPropertyAsync(string initialState, T defaultValue, CancellationToken cancellationToken = default)
        {
            JsonDocument shadowDoc = JsonDocument.Parse(initialState);
            JsonElement versionEl = shadowDoc.RootElement.GetProperty("version");
            if (versionEl.TryGetInt32(out int version))
            {
                Version = version;
            }

            JsonElement desiredNode = shadowDoc.RootElement.GetProperty("state").GetProperty("desired");
            if (desiredNode.TryGetProperty(_name, out JsonElement desiredVal))
            {
                Value = desiredVal.Deserialize<T>()!;
            }
            else
            {
                Value = defaultValue;
            }

            var resTopic = responseTopic.Replace("{clientId}", _connection.Options.ClientId);
            await _connection.PublishAsync(new MqttApplicationMessageBuilder()
                                .WithTopic(resTopic)
                                .WithPayload(serializer.ToBytes(defaultValue, _name, Version!.Value))
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                .Build(), cancellationToken);
        }
    }
}
