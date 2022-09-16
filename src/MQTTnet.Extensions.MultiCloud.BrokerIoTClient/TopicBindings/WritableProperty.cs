using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class WritableProperty<T> : IWritableProperty<T>
    {
        public PropertyAck<T> PropertyValue { get; set; }

        private readonly string propertyName;
        private readonly string componentName;

        //readonly UpdateTwinBinder updateTwin;
        private readonly IPropertyStoreWriter updatePropertyBinder;
        private readonly DesiredUpdatePropertyBinder<T>? desiredBinder;

        public Func<PropertyAck<T>, PropertyAck<T>> OnProperty_Updated
        {
            get => desiredBinder?.OnProperty_Updated!;
            set => desiredBinder!.OnProperty_Updated = value;
        }

        public string PropertyName => propertyName;

        readonly IMqttClient connection;

        public WritableProperty(IMqttClient c, string name, string component = "")
        {
            connection = c;
            propertyName = name;
            componentName = component;
            updatePropertyBinder = new UpdatePropertyBinder(c, name);
            PropertyValue = new PropertyAck<T>(name, componentName);
            desiredBinder = new DesiredUpdatePropertyBinder<T>(c, updatePropertyBinder, name, componentName);
        }

        public async Task<int> ReportPropertyAsync(CancellationToken token = default) => await updatePropertyBinder.ReportPropertyAsync(PropertyValue, token);

        public async Task InitPropertyAsync(string twin, T defaultValue, CancellationToken cancellationToken = default)
        {
            await desiredBinder!.InitSubscriptions(connection);
            if (!string.IsNullOrEmpty(twin))
            {
                Trace.TraceWarning("twin not expected");
            }

            PropertyValue = new PropertyAck<T>(propertyName, componentName)
            {
                Value = defaultValue,
            };
            await updatePropertyBinder.ReportPropertyAsync(PropertyValue, cancellationToken);
        }
    }
}
