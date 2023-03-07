using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class ReadOnlyProperty<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    private readonly string _name;
    public T? Value { get; set; }
    public int Version { get; set; }
    public ReadOnlyProperty(IMqttClient mqttClient, string name)
        : base(mqttClient, name)
    {
        _name = name;
        TopicPattern = "$iothub/twin/PATCH/properties/reported/?$rid=1";
        WrapMessage = true;
        Retain = false;
    }

    public void InitProperty(string initialState)
    {
        JsonDocument doc = JsonDocument.Parse(initialState);
        JsonElement reported = doc.RootElement.GetProperty("reported");
        Version = reported.GetProperty("$version").GetInt32();
        if (reported.TryGetProperty(_name, out JsonElement element))
        {
            Value = element.Deserialize<T>()!;
        }
    }

    public Task SendMessageAsync(CancellationToken cancellationToken = default) => SendMessageAsync(Value!, cancellationToken);

}
