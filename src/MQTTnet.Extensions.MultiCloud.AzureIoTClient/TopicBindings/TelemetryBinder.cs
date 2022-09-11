using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class Telemetry<T> : ITelemetry<T>
    {
        private readonly IMqttClient connection;
        private readonly string deviceId;
        private readonly string moduleId;
        private readonly string name;
        private readonly string componentName;

        public Telemetry(IMqttClient connection, string name, string componentName = "", string moduleId = "")
        {
            this.connection = connection;
            this.name = name;
            this.componentName = componentName;
            deviceId = connection.Options.ClientId;
            this.moduleId = moduleId;
        }

        public async Task<MqttClientPublishResult> SendTelemetryAsync(T payload, CancellationToken cancellationToken = default)
        {
            string topic = $"devices/{deviceId}";

            if (!string.IsNullOrEmpty(moduleId))
            {
                topic += $"/modules/{moduleId}";
            }
            topic += "/messages/events/";

            if (!string.IsNullOrEmpty(componentName))
            {
                topic += $"$.sub={componentName}";
            }

            Dictionary<string, T> typedPayload = new Dictionary<string, T>
            {
                { name, payload }
            };

            return await connection.PublishStringAsync(topic, Json.Stringify(typedPayload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, cancellationToken);
        }
    }
}