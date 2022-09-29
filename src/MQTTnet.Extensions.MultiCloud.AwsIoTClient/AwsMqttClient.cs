using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient
{
    public class AwsMqttClient
    {
        public IMqttClient Connection { get; private set; }
        private readonly ShadowRequestResponseBinder getShadowBinder;


        public AwsMqttClient(IMqttClient c, string modelId = "") //: base(c)
        {
            Connection = c;
            var birthMsg =
                new UTF8JsonSerializer().ToBytes(
                        new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                        {
                            ModelId = modelId
                        });
            _ = Connection.PublishBinaryAsync(
                BirthConvention.BirthTopic(Connection.Options.ClientId),
                birthMsg,
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);

            getShadowBinder = new ShadowRequestResponseBinder(c);
        }

        public Task<string> GetShadowAsync(CancellationToken cancellationToken = default) =>
            getShadowBinder.GetShadowAsync(cancellationToken);
        public Task<string> UpdateShadowAsync(object payload, CancellationToken cancellationToken = default) =>
            getShadowBinder.UpdateShadowAsync(payload, cancellationToken);
    }
}
