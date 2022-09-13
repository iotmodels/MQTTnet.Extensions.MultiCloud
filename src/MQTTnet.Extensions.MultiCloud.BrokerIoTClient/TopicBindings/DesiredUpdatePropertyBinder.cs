using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings
{
    public class DesiredUpdatePropertyBinder<T>
    {
        public Func<PropertyAck<T>, Task<PropertyAck<T>>>? OnProperty_Updated = null;
        public DesiredUpdatePropertyBinder(IMqttClient connection, string propertyName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync($"pnp/{connection.Options.ClientId}/props/{propertyName}/+").Result;
            subAck.TraceErrors();
            IReportPropertyBinder propertyBinder = new UpdatePropertyBinder(connection, propertyName);
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"pnp/{connection.Options.ClientId}/props/{propertyName}/set"))
                {
                    JsonNode desiredProperty = JsonNode.Parse(Encoding.UTF8.GetString(m.ApplicationMessage.Payload))!;
                    //JsonNode desiredProperty = PropertyParser.ReadPropertyFromDesired(desired, propertyName, componentName);
                    //var desiredProperty = desired?[propertyName];
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
                                Value = desiredProperty.Deserialize<T>()!,
                                //Version = desired?["$version"]?.GetValue<int>() ?? 0
                            };
                            var ack = await OnProperty_Updated(property);
                            if (ack != null)
                            {
                                //_ = updateTwin.SendRequestWaitForResponse(ack);
                                _ = propertyBinder.ReportPropertyAsync(ack);
                            }
                        }
                    }
                }
            };
        }
    }
}
