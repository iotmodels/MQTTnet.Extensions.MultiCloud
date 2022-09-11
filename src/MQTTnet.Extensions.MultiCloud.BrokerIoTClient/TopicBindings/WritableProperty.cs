﻿using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class WritableProperty<T> : IWritableProperty<T>
    {
        public PropertyAck<T> PropertyValue { get; set; }
        readonly string propertyName;
        readonly string componentName;
        //readonly UpdateTwinBinder updateTwin;
        readonly IReportPropertyBinder updatePropertyBinder;
        readonly DesiredUpdatePropertyBinder<T>? desiredBinder;

        public Func<PropertyAck<T>, Task<PropertyAck<T>>> OnProperty_Updated
        {
            get => desiredBinder!.OnProperty_Updated!;
            set => desiredBinder!.OnProperty_Updated = value;
        }

        public string PropertyName => propertyName;

        public WritableProperty(IMqttClient connection, string name, string component = "")
        {
            propertyName = name;
            componentName = component;
            //updateTwin = new UpdateTwinBinder(connection);
            updatePropertyBinder = new UpdatePropertyBinder(connection, name);
            PropertyValue = new PropertyAck<T>(name, componentName);
            desiredBinder = new DesiredUpdatePropertyBinder<T>(connection, name, componentName);
        }

        public async Task<int> ReportPropertyAsync(CancellationToken token = default) => await updatePropertyBinder.ReportPropertyAsync(PropertyValue, token);

        public async Task InitPropertyAsync(string twin, T defaultValue, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(twin))
            {
                Trace.TraceWarning("twin not expected");
            }

            PropertyValue = new PropertyAck<T>(propertyName, componentName)
            {
                Value = defaultValue,
            };
            _ = await updatePropertyBinder.ReportPropertyAsync(PropertyValue, cancellationToken);
        }
    }
}
