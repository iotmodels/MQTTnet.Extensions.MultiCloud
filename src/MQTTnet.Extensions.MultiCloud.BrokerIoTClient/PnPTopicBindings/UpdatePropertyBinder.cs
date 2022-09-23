using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.PnPTopicBindings
{
    public class UpdatePropertyBinder : IPropertyStoreWriter
    {
        private readonly IMqttClient connection;
        private readonly string name;
        public UpdatePropertyBinder(IMqttClient connection, string propName)
        {
            this.connection = connection;
            name = propName;
        }

        public async Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            await connection.PublishJsonAsync($"pnp/{connection.Options.ClientId}/props/{name}", payload, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true, cancellationToken);
            return 0; //versions not supported on plain MQTT
        }
    }
}
