using MQTTnet.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class ReadOnlyProperty<T> : IReadOnlyProperty<T>
    {
        readonly IReportPropertyBinder updateBinder;
        public string PropertyName { get; }
        //readonly string component;

        public T PropertyValue { get; set; }
        public int Version { get; set; }

        public ReadOnlyProperty(IMqttClient connection, string name, string component = "")
        {
            string propFullName = name;
            if (!string.IsNullOrEmpty(component))
            {
                propFullName = component + "*" + name;
            }
            updateBinder = new UpdatePropertyBinder(connection, propFullName);
            PropertyName = name;
            PropertyValue = default!;
            //this.component = component;
        }

        public async Task<int> ReportPropertyAsync(CancellationToken cancellationToken = default)
        {
            //bool asComponent = !string.IsNullOrEmpty(component);
            await updateBinder.ReportPropertyAsync(PropertyValue!, cancellationToken);
            return -1;
        }

        //Dictionary<string, object> ToJsonDict(bool asComponent = false)
        //{
        //    Dictionary<string, object> result;
        //    if (asComponent == false)
        //    {
        //        result = new Dictionary<string, object> { { PropertyName, PropertyValue } };
        //    }
        //    else
        //    {
        //        Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
        //        {
        //            { component, new Dictionary<string, object>() }
        //        };
        //        dict[component].Add("__t", "c");
        //        dict[component].Add(PropertyName, PropertyValue);
        //        result = dict.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
        //    }
        //    return result;
        //}

    }
}
