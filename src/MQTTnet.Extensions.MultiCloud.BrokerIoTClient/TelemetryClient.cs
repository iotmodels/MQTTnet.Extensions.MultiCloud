using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class TelemetryClient<T>
    {            
        const string topicPattern = "device/{clientId}/telemetry/{name}";
        readonly IMqttClient _mqttClient;
        readonly IMessageSerializer _serializer;
        readonly bool _unwrap = false;
        readonly string _name = string.Empty;
        public Action<string,T>? OnTelemetry { get; set; }

        public TelemetryClient(IMqttClient client, string name) 
            : this(client, name, new UTF8JsonSerializer())
        {
            
        }

        public TelemetryClient(IMqttClient client, string name, IMessageSerializer ser)
        {
            _mqttClient = client;
            _serializer = ser;
            _unwrap = !string.IsNullOrEmpty(name);
            _name = name;
            _mqttClient.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                string deviceId = topic.Split('/')[1];
                if (topic.Contains("/telemetry/" + _name))
                {
                    if (_serializer.TryReadFromBytes<T>(m.ApplicationMessage.Payload, _unwrap ? _name : string.Empty, out T msg))
                    {
                        OnTelemetry?.Invoke(deviceId,msg);
                    }
                }
                await Task.Yield();
            };

        }

        public async Task<MqttClientSubscribeResult> StartAsync(string deviceFilter)
        {
            var subAck =  await _mqttClient.SubscribeAsync(topicPattern.Replace("{clientId}", deviceFilter).Replace("{name}", _name));
            return subAck;
        }
    }
}
