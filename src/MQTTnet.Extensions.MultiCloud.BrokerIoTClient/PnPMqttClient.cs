using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class PnPMqttClient
    {
        public IMqttClient Connection { get; private set; }
        public string InitialState { get; set; }
        public PnPMqttClient(IMqttClient c, string modelId = "")
        {
            Connection = c;
            var pubAck = Connection.PublishJsonAsync(
                BirthConvention.BirthTopic(Connection.Options.ClientId),
                new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                {
                    ModelId = modelId
                },
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true).Result;
            InitialState = string.Empty;
        }
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) =>
            Connection.PublishJsonAsync($"pnp/{Connection.Options.ClientId}/telemetry", payload, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);
    }
}
