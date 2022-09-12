using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Threading.Tasks;
using System.Threading;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class PnPMqttClient
    {
        public IMqttClient Connection { get; private set; }
        public string InitialState { get; set; }
        public PnPMqttClient(IMqttClient c, string modelId = "")
        {
            Connection = c;
            var pubAck = Connection.PublishStringAsync(
                BirthConvention.BirthTopic(Connection.Options.ClientId),
                new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                {
                    ModelId = modelId
                }.ToJson(),
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true).Result;
            InitialState = string.Empty;
        }
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) => 
            Connection.PublishStringAsync($"pnp/{Connection.Options.ClientId}/telemetry", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);
    }
}
