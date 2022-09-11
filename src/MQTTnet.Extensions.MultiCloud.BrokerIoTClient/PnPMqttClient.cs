using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients.Connections;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public class PnPMqttClient
    {
        public IMqttClient Connection { get; private set; }
        public PnPMqttClient(IMqttClient c, string modelId = "")
        {
            Connection = c;
            _ = Connection.PublishStringAsync(
                BirthConvention.BirthTopic(Connection.Options.ClientId),
                new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                {
                    ModelId = modelId
                }.ToJson(),
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);
        }
    }
}
