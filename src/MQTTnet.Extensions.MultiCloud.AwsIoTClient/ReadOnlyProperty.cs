using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient
{
    public class ReadOnlyProperty<T> : IReadOnlyProperty<T>
    {
        readonly IMqttClient _connection;
        readonly string _name;
        public ReadOnlyProperty(IMqttClient mqttClient, string name)
        {
            _connection = mqttClient;
            _name = name;
        }
        public async Task SendMessageAsync(T payload, CancellationToken cancellationToken = default)
        {
            ShadowSerializer serializer = new();
            var topic = $"$aws/things/{_connection.Options.ClientId}/shadow/update";
            var payloadBytes = serializer.ToBytes(payload, _name);
            await _connection.PublishBinaryAsync(topic, payloadBytes, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, cancellationToken);
        }
    }
}
