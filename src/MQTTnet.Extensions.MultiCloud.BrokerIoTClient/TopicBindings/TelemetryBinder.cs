using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class Telemetry<T> : ITelemetry<T>
    {
        readonly IMqttClient connection;
        readonly string deviceId;
        readonly string moduleId;
        readonly string name;
        readonly string component;

        public Telemetry(IMqttClient connection, string name, string component = "", string moduleId = "")
        {
            this.connection = connection;
            this.name = name;
            this.component = component;
            deviceId = connection.Options.ClientId;
            this.moduleId = moduleId;
        }

        public async Task<MqttClientPublishResult> SendTelemetryAsync(T payload, CancellationToken cancellationToken = default)
        {
            string topic = $"pnp/{deviceId}";

            if (!string.IsNullOrEmpty(component))
            {
                topic += $"/{component}";
            }
            if (!string.IsNullOrEmpty(moduleId))
            {
                topic += $"/modules/{moduleId}";
            }
            topic += "/telemetry";


            Dictionary<string, T> typedPayload = new Dictionary<string, T>
            {
                { name, payload }
            };
            return await connection.PublishStringAsync(topic, Json.Stringify(typedPayload), Protocol.MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);
        }
    }
}
