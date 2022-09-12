using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class DesiredUpdatePropertyBinder<T>
    {
        public Func<PropertyAck<T>, Task<PropertyAck<T>>> OnProperty_Updated = null;
        public DesiredUpdatePropertyBinder(IMqttClient connection, string propertyName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync("$iothub/twin/PATCH/properties/desired/#").Result;
            subAck.TraceErrors();
            IPropertyStoreWriter updateTwin = new UpdateTwinBinder(connection);
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
                         if (OnProperty_Updated != null)
                         {
                             var property = new PropertyAck<T>(propertyName, componentName)
                             {
                                 Value = desiredProperty.Deserialize<T>(),
                                 Version = desired?["$version"]?.GetValue<int>() ?? 0
                             };
                             var ack = await OnProperty_Updated(property);
                             if (ack != null)
                             {
                                 _ = updateTwin.ReportPropertyAsync(ack.ToAckDict());
                             }
                         }
                     }
                 }
             };
        }


    }
}
