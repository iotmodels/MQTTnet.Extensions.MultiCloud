using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public abstract class Component
    {
        private readonly string name;
        private readonly IPropertyStoreWriter update;

        public Component(IMqttClient connection, string name)
        {
            this.name = name;
            update = new UpdateShadowBinder(connection);
        }

        public async Task<int> ReportPropertyAsync(CancellationToken token = default)
        {
            Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
                {
                    { name, new Dictionary<string, object>() }
                };
            dict[name] = ToJsonDict();
            dict[name].Add("__t", "c");
            var v = await update.ReportPropertyAsync(dict, token);
            return v;
        }

        public abstract Dictionary<string, object> ToJsonDict();
    }
}
