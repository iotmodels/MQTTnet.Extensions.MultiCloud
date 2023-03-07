using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient;

public class ReadOnlyProperty<T> : IReadOnlyProperty<T>
{
    readonly IMqttClient _connection;
    readonly string _name;
    public T? Value { get; set; }
    public int Version { get; set; }
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

    public void InitProperty(string initialState)
    {
        throw new System.NotImplementedException();
    }

    public Task SendMessageAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
