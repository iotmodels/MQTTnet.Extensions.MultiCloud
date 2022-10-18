using MQTTnet.Client;
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


        public AwsMqttClient(IMqttClient c) //: base(c)
        {
            Connection = c;
            getShadowBinder = new ShadowRequestResponseBinder(c);
        }

        public Task<string> GetShadowAsync(CancellationToken cancellationToken = default) =>
            getShadowBinder.GetShadowAsync(cancellationToken);
        public Task<string> UpdateShadowAsync(object payload, CancellationToken cancellationToken = default) =>
            getShadowBinder.UpdateShadowAsync(payload, cancellationToken);
    }
}
