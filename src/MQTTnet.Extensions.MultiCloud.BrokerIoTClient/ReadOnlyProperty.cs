using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    private readonly TaskCompletionSource _tcs;
    private readonly IMqttClient _client;
    private readonly string _name;
    private readonly string _topic;

    public T? Value { get; set; }
    public int Version { get; set; }

    public ReadOnlyProperty(IMqttClient mqttClient) : this(mqttClient, string.Empty) { }
    public ReadOnlyProperty(IMqttClient mqttClient, string name)
        : base(mqttClient, name)
    {
        _client = mqttClient;
        _name = name;
        _tcs = new TaskCompletionSource();
        TopicPattern = "device/{clientId}/props/{name}";
        WrapMessage = false;
        Retain = true;
        _topic = TopicPattern.Replace("{clientId}", _client.Options.ClientId).Replace("{name}", _name);
        _client.ApplicationMessageReceivedAsync += async m =>
        {
            if (m.ApplicationMessage.Topic == _topic)
            {
                var ser = new UTF8JsonSerializer();
                if (ser.TryReadFromBytes(m.ApplicationMessage.Payload, _name, out T propVal))
                {
                    Value = propVal;
                    _tcs.TrySetResult();
                }
            }
            await Task.Yield();
        };
    }

    public void InitProperty(string initialState)
    {
       _ = _client.SubscribeAsync(_topic);
        _tcs.Task.Wait(5000);
    }

    public  Task SendMessageAsync(CancellationToken cancellationToken = default) => SendMessageAsync(Value!, cancellationToken);
    
}
