using Avro;
using Google.Protobuf.WellKnownTypes;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using payload_size.Serializers;
using System.Text.Json;
using System.Xml.Linq;

namespace payload_size.Binders;

public class ReadOnlyPropertyAvro<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
{
    private readonly string _name;
    public T? Value { get; set; }
    public int Version { get; set; }
    public ReadOnlyPropertyAvro(IMqttClient mqttClient, Schema schema)
        : this(mqttClient, string.Empty, schema) { }

    public ReadOnlyPropertyAvro(IMqttClient mqttClient, string name, Schema schema)
        : base(mqttClient, name, new AvroSerializer<T>(schema))
    {
        _name = name;
        TopicPattern = "device/{clientId}/props";
        WrapMessage = false;
        Retain = true;
    }

    public void InitProperty(string initialState)
    {
        throw new NotImplementedException();
    }

    public  Task SendMessageAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
