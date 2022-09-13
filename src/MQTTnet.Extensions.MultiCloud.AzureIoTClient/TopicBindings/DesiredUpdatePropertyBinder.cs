using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class DesiredUpdatePropertyBinder<T>
    {
        public Func<PropertyAck<T>, PropertyAck<T>> OnProperty_Updated = null;
        public DesiredUpdatePropertyBinder(IMqttClient connection, IReportPropertyBinder updTwinBinder, string propertyName, string componentName = "")
        {
            connection.SubscribeWithReply("$iothub/twin/PATCH/properties/desired/#");
            connection.ApplicationMessageReceivedAsync += async m =>
             {
                 var topic = m.ApplicationMessage.Topic;
                 if (topic.StartsWith("$iothub/twin/PATCH/properties/desired"))
                 {
                     string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                     JsonNode desired = JsonNode.Parse(msg);
                     JsonNode desiredProperty = PropertyParser.ReadPropertyFromDesired(desired, propertyName, componentName);
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
                                 Value = desiredProperty.Deserialize<T>(),
                                 Version = desired?["$version"]?.GetValue<int>() ?? 0
                             };
                             var ack = OnProperty_Updated(property);
                             if (ack != null)
                             {
                                 updTwinBinder.ReportPropertyAsync(ack.ToAckDict()).RunSynchronously();
                             }
                         }
                     }
                 }
                 await Task.Yield();
             };
        }


    }
}
