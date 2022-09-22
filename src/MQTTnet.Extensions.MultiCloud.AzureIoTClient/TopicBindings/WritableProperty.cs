using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class WritableProperty<T> : IWritableProperty<T>
    {
        public PropertyAck<T> PropertyValue { get; set; }

        public string PropertyName { get; set; }
        private readonly string componentName;
        private readonly IPropertyStoreWriter updateTwin;
        private readonly DesiredUpdatePropertyBinder<T> desiredBinder;

        public Func<PropertyAck<T>, PropertyAck<T>> OnProperty_Updated
        {
            get => desiredBinder.OnProperty_Updated;
            set => desiredBinder.OnProperty_Updated = value;
        }

        public WritableProperty(IMqttClient connection, string name, string component = "")
        {
            PropertyName = name;
            componentName = component;
            PropertyValue = new PropertyAck<T>(name, componentName);
            updateTwin = new UpdateTwinBinder(connection);
            desiredBinder = new DesiredUpdatePropertyBinder<T>(connection, updateTwin, name, componentName);
        }

        public async Task<int> ReportPropertyAsync(CancellationToken token = default) => await updateTwin.ReportPropertyAsync(PropertyValue.ToAckDict(), token);

        public async Task InitPropertyAsync(string twin, T defaultValue, CancellationToken cancellationToken = default)
        {
            await desiredBinder.InitSusbscriptionsAsync();
            await Task.Yield();
            PropertyValue = InitFromTwin(twin, PropertyName, componentName, defaultValue);
            if (desiredBinder.OnProperty_Updated != null && PropertyValue.DesiredVersion > 1)
            {
                var ack = desiredBinder.OnProperty_Updated.Invoke(PropertyValue);
                if (ack != null)
                {
                    PropertyValue = ack;
                }
            }
            _ = updateTwin.ReportPropertyAsync(PropertyValue.ToAckDict(), cancellationToken).ConfigureAwait(false);
        }
        public Task InitPropertyAsync(byte[] defaultValue, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        private PropertyAck<T> InitFromTwin(string twinJson, string propName, string componentName, T defaultValue)
        {
            if (string.IsNullOrEmpty(twinJson))
            {
                Trace.TraceWarning("InitFromTwin: Cannot initialize from empty twin");
                return new PropertyAck<T>(propName, componentName) { Value = defaultValue, Version = 0, Status = 203, Description = "Init from default value" };
            }

            var root = JsonNode.Parse(twinJson);
            var desired = root?["desired"];
            var reported = root?["reported"];
            T desired_Prop = default;
            int desiredVersion = desired["$version"].GetValue<int>();
            PropertyAck<T> result = new PropertyAck<T>(propName, componentName) { DesiredVersion = desiredVersion };

            bool desiredFound = false;
            if (!string.IsNullOrEmpty(componentName))
            {
                if (desired[componentName] != null &&
                    desired[componentName]["__t"] != null &&
                    desired[componentName]["__t"]?.GetValue<string>() == "c" &&
                    desired[componentName][propName] != null)
                {
                    desired_Prop = desired[componentName][propName].Deserialize<T>();
                    desiredFound = true;
                }
            }
            else
            {
                if (desired[propName] != null)
                {
                    desired_Prop = desired[propName].Deserialize<T>();
                    desiredFound = true;
                }
            }

            bool reportedFound = false;
            T reported_Prop = default;
            int reported_Prop_version = 0;
            int reported_Prop_status = 001;
            string reported_Prop_description = string.Empty;

            if (!string.IsNullOrEmpty(componentName))
            {
                if (reported[componentName] != null &&
                    reported[componentName]["__t"]?.GetValue<string>() == "c" &&
                    reported[componentName][propName] != null)
                {
                    reported_Prop = reported[componentName][propName]["value"].Deserialize<T>();
                    reported_Prop_version = reported[componentName][propName]["av"]?.GetValue<int>() ?? -1;
                    reported_Prop_status = reported[componentName][propName]["ac"].GetValue<int>();
                    reported_Prop_description = reported[componentName][propName]["ad"]?.GetValue<string>();
                    reportedFound = true;
                }
            }
            else
            {
                if (reported[propName] != null)
                {
                    reported_Prop = reported[propName]["value"].Deserialize<T>();

                    reported_Prop_version = reported[propName]["av"]?.GetValue<int>() ?? -1;
                    reported_Prop_status = reported[propName]["ac"].GetValue<int>();
                    reported_Prop_description = reported[propName]["ad"]?.GetValue<string>();
                    reportedFound = true;
                }
            }

            if (!desiredFound && !reportedFound)
            {
                result = new PropertyAck<T>(propName, componentName)
                {
                    //DesiredVersion = desiredVersion,
                    Version = reported_Prop_version,
                    Value = defaultValue,
                    Status = 203,
                    Description = "Init from default value"
                };
            }

            if (!desiredFound && reportedFound)
            {
                result = new PropertyAck<T>(propName, componentName)
                {
                    DesiredVersion = 0,
                    Version = reported_Prop_version,
                    Value = reported_Prop,
                    Status = reported_Prop_status,
                    Description = reported_Prop_description,
                    LastReported = reported_Prop
                };
            }

            if (desiredFound && reportedFound)
            {
                if (desiredVersion >= reported_Prop_version)
                {
                    result = new PropertyAck<T>(propName, componentName)
                    {
                        DesiredVersion = desiredVersion,
                        Value = desired_Prop,
                        Version = desiredVersion,
                        LastReported = reported_Prop
                    };
                }
            }


            if (desiredFound && !reportedFound)
            {
                result = new PropertyAck<T>(propName, componentName)
                {
                    DesiredVersion = desiredVersion,
                    Version = desiredVersion,
                    Value = desired_Prop
                };
            }
            return result;
        }


    }
}
