using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class ReadOnlyProperty<T> : IReadOnlyProperty<T>
    {
        private readonly IPropertyStoreWriter updateTwin;
        public string PropertyName { get; }
        private readonly string component;
        public T PropertyValue { get; set; }
        public int Version { get; set; }

        public ReadOnlyProperty(IMqttClient connection, string name, string component = "")
        {
            updateTwin = new UpdateTwinBinder(connection);
            PropertyName = name;
            this.component = component;
        }

        public async Task<int> ReportPropertyAsync(CancellationToken cancellationToken = default)
        {
            bool asComponent = !string.IsNullOrEmpty(component);
            Version = await updateTwin.ReportPropertyAsync(ToJsonDict(asComponent), cancellationToken);
            return Version;
        }

        private Dictionary<string, object> ToJsonDict(bool asComponent = false)
        {
            Dictionary<string, object> result;
            if (asComponent == false)
            {
                result = new Dictionary<string, object> { { PropertyName, PropertyValue } };
            }
            else
            {
                Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
                {
                    { component, new Dictionary<string, object>() }
                };
                dict[component].Add("__t", "c");
                dict[component].Add(PropertyName, PropertyValue);
                result = dict.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
            }
            return result;
        }

    }
}
