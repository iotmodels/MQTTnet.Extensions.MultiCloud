using MQTTnet.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public class ReadOnlyProperty<T> : IReadOnlyProperty<T>
    {
        private readonly IPropertyStoreWriter updateBinder;
        public string PropertyName { get; }

        private readonly string component;


        public T PropertyValue { get; set; }
        public int Version { get; set; }

        public ReadOnlyProperty(IMqttClient connection, string name, string component = "")
        {
            updateBinder = new UpdateShadowBinder(connection);
            PropertyName = name;
            PropertyValue = default!;
            this.component = component;
        }

        public async Task<int> ReportPropertyAsync(CancellationToken cancellationToken = default)
        {
            bool asComponent = !string.IsNullOrEmpty(component);
            return await updateBinder.ReportPropertyAsync(ToJsonDict(asComponent), cancellationToken);
        }

        private Dictionary<string, object> ToJsonDict(bool asComponent = false)
        {
            Dictionary<string, object> result;
            if (asComponent == false)
            {
                result = new Dictionary<string, object> { { PropertyName, PropertyValue! } };
            }
            else
            {
                Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
                {
                    { component, new Dictionary<string, object>() }
                };
                dict[component].Add("__t", "c");
                dict[component].Add(PropertyName, PropertyValue!);
                result = dict.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
            }
            return result;
        }

        public async Task<int> ReportPropertyAsync(byte[] payload, CancellationToken cancellationToken = default)
        {
            return await updateBinder.ReportPropertyAsync(payload, cancellationToken);
        }
    }
}
