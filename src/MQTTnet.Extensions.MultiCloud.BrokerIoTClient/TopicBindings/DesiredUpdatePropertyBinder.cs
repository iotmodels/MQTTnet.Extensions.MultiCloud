using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class DesiredUpdatePropertyBinder<T>
    {
        IMqttClient connection;
        string name;
        string cName;
        public Func<PropertyAck<T>, PropertyAck<T>>? OnProperty_Updated = null;

        public DesiredUpdatePropertyBinder(IMqttClient c, IPropertyStoreWriter propertyBinder, string propertyName, string componentName = "")
        {
            name = propertyName;
            cName = componentName;
            connection = c;
            c.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"pnp/{connection.Options.ClientId}/props/{propertyName}/set"))
                {
                    //JsonNode desiredProperty = JsonNode.Parse(Encoding.UTF8.GetString(m.ApplicationMessage.Payload))!;
                    //JsonNode desiredProperty = PropertyParser.ReadPropertyFromDesired(desired, propertyName, componentName);
                    //var desiredProperty = desired?[propertyName];
                    
                    var desiredProperty =  m.ApplicationMessage.Payload;

                    if (desiredProperty != null)
                    {
                        if (OnProperty_Updated == null)
                        {
                            Trace.TraceWarning($"Desired property {propertyName} received, but no handler found.");
                        }
                        else
                        {
                            var property = new PropertyAck<T>(propertyName, componentName)
                            {
                                ValueBytes = m.ApplicationMessage.Payload
                                //Value = desiredProperty.Deserialize<T>()!,
                                //Version = desired?["$version"]?.GetValue<int>() ?? 0
                            };
                            var ack = OnProperty_Updated(property);
                            if (ack != null)
                            {
                                _ = propertyBinder.ReportPropertyAsync(ack.ValueBytes);
                            }
                        }
                    }
                }
                await Task.Yield();
            };
        }

        public async Task InitSubscriptions(IMqttClient connection)
        {
            var subAck = await connection.SubscribeAsync($"pnp/{connection.Options.ClientId}/props/{name}/+");
            subAck.TraceErrors();
        }
    }
}
