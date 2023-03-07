using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class TelemetryClient<T>
    {            
        const string topicPattern = "device/{clientId}/telemetry/{name}";
        readonly IMqttClient _mqttClient;
        readonly IMessageSerializer<T> _serializer;
        readonly bool _unwrap = false;
        readonly string _name = string.Empty;
        public Action<T>? OnTelemetry { get; set; }

        public TelemetryClient(IMqttClient client, string name) 
            : this(client, name, new UTF8JsonSerializer<T>())
        {
            
        }

        public TelemetryClient(IMqttClient client, string name, IMessageSerializer<T> ser)
        {
            _mqttClient = client;
            _serializer = ser;
            _unwrap = !string.IsNullOrEmpty(name);
            _name = name;
            _mqttClient.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.Contains("/telemetry/" + _name))
                {
                    if (_serializer.TryReadFromBytes(m.ApplicationMessage.Payload, _unwrap ? _name : string.Empty, out T msg))
                    {
                        OnTelemetry?.Invoke(msg);
                    }
                }
                await Task.Yield();
            };

        }

        public async Task<MqttClientSubscribeResult> Start(string deviceFilter)
        {
            var subAck =  await _mqttClient.SubscribeAsync(topicPattern.Replace("{clientId}", deviceFilter).Replace("{name}", _name));
            return subAck;
        }
    }
}
