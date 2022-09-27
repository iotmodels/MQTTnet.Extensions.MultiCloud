using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.PnPTopicBindings
{
    public class Telemetry<T> : ITelemetry<T>
    {
        private readonly IMqttClient connection;
        private readonly string deviceId;
        private readonly string moduleId;
        private readonly string name;
        private readonly string component;

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
            return await connection.PublishJsonAsync(topic, typedPayload, Protocol.MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);
        }
    }
}
