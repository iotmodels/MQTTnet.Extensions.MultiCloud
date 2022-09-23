using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.PnPTopicBindings
{
    public abstract class Component
    {
        private readonly string name;
        private readonly UpdatePropertyBinder update;

        public Component(IMqttClient connection, string name)
        {
            this.name = name;
            update = new UpdatePropertyBinder(connection, name);
        }

        public async Task<int> ReportPropertyAsync(CancellationToken token)
        {

            Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
                {
                    { name, new Dictionary<string, object>() }
                };
            dict[name] = ToJsonDict();
            dict[name].Add("__t", "c");
            _ = await update.ReportPropertyAsync(dict, token);
            return 0;
        }
        public abstract Dictionary<string, object> ToJsonDict();
    }
}
