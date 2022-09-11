using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient
{
    public class AwsMqttClient
    {
        public IMqttClient Connection { get; private set; }
        private readonly IPropertyStoreReader getShadowBinder;
        private readonly IPropertyStoreWriter updateShadowBinder;

        public AwsMqttClient(IMqttClient c, string modelId = "") //: base(c)
        {
            Connection = c;
            _ = Connection.PublishStringAsync(
                BirthConvention.BirthTopic(Connection.Options.ClientId),
                new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                {
                    ModelId = modelId
                }.ToJson(),
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);

            getShadowBinder = new GetShadowBinder(c);
            updateShadowBinder = new UpdateShadowBinder(c);
        }

        public Task<string> GetShadowAsync(CancellationToken cancellationToken = default) => getShadowBinder.ReadPropertiesDocAsync(cancellationToken);
        public Task<int> UpdateShadowAsync(object payload, CancellationToken cancellationToken = default) => updateShadowBinder.ReportPropertyAsync(payload, cancellationToken);
    }
}
