using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class UpdatePropertyBinder : IReportPropertyBinder
    {
        readonly IMqttClient connection;
        readonly string name;
        public UpdatePropertyBinder(IMqttClient connection, string propName)
        {
            this.connection = connection;
            name = propName;
        }

        public async Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            await connection.PublishStringAsync($"pnp/{connection.Options.ClientId}/props/{name}", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true, cancellationToken);
            return 0; //versions not supported on plain MQTT
        }
    }
}
